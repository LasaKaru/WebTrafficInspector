using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class IDORScannerService
    {
        private HttpClient _httpClient;
        private List<IDORScanResult> _scanHistory = new List<IDORScanResult>();

        public IDORScannerService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<IDORScanReport> ScanUrl(string url, IDORScanOptions options = null)
        {
            options = options ?? new IDORScanOptions();

            var report = new IDORScanReport
            {
                TargetUrl = url,
                StartTime = DateTime.Now,
                Vulnerabilities = new List<IDORVulnerability>()
            };

            try
            {
                // Extract numeric IDs from URL
                var idParameters = ExtractIDParameters(url);

                if (!idParameters.Any())
                {
                    report.Status = "No ID parameters found";
                    report.EndTime = DateTime.Now;
                    return report;
                }

                // Get baseline response for original ID
                var baselineResponse = await GetResponse(url);
                if (baselineResponse == null)
                {
                    report.Status = "Failed to get baseline response";
                    report.EndTime = DateTime.Now;
                    return report;
                }

                foreach (var param in idParameters)
                {
                    // Test sequential IDs
                    var sequentialVulns = await TestSequentialIDs(url, param, baselineResponse, options);
                    report.Vulnerabilities.AddRange(sequentialVulns);

                    // Test common IDs
                    var commonVulns = await TestCommonIDs(url, param, baselineResponse, options);
                    report.Vulnerabilities.AddRange(commonVulns);

                    // Test negative/special values
                    var specialVulns = await TestSpecialValues(url, param, baselineResponse, options);
                    report.Vulnerabilities.AddRange(specialVulns);

                    report.TotalTests += sequentialVulns.Count + commonVulns.Count + specialVulns.Count;
                }

                report.VulnerabilitiesFound = report.Vulnerabilities.Count;
                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.Status = report.VulnerabilitiesFound > 0 ? "VULNERABLE" : "Not Vulnerable";

                _scanHistory.Add(new IDORScanResult
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

        public async Task<IDORScanReport> ScanTrafficEntry(TrafficEntry entry, IDORScanOptions options = null)
        {
            return await ScanUrl(entry.Url, options);
        }

        private async Task<List<IDORVulnerability>> TestSequentialIDs(string url, IDParameter param, HttpResponse baseline, IDORScanOptions options)
        {
            var vulnerabilities = new List<IDORVulnerability>();
            var originalId = param.Value;

            // Test IDs around the original value
            var testRange = options.SequentialRange ?? 5;
            for (int i = -testRange; i <= testRange; i++)
            {
                if (i == 0) continue; // Skip original

                var testId = originalId + i;
                if (testId < 1 && !options.TestNegativeIDs) continue;

                var testUrl = ReplaceIDInUrl(url, param.Name, testId.ToString());
                var response = await GetResponse(testUrl);

                if (response != null && IsIDORVulnerable(baseline, response, options))
                {
                    vulnerabilities.Add(new IDORVulnerability
                    {
                        Url = testUrl,
                        Parameter = param.Name,
                        OriginalValue = originalId.ToString(),
                        TestedValue = testId.ToString(),
                        Type = IDORType.SequentialAccess,
                        Severity = DetermineSeverity(response, baseline),
                        Evidence = ExtractEvidence(response),
                        StatusCode = response.StatusCode,
                        Timestamp = DateTime.Now
                    });
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<IDORVulnerability>> TestCommonIDs(string url, IDParameter param, HttpResponse baseline, IDORScanOptions options)
        {
            var vulnerabilities = new List<IDORVulnerability>();
            var commonIDs = new[] { 1, 2, 3, 10, 100, 1000, 9999, 10000 };

            foreach (var testId in commonIDs)
            {
                if (testId == param.Value) continue; // Skip if same as original

                var testUrl = ReplaceIDInUrl(url, param.Name, testId.ToString());
                var response = await GetResponse(testUrl);

                if (response != null && IsIDORVulnerable(baseline, response, options))
                {
                    vulnerabilities.Add(new IDORVulnerability
                    {
                        Url = testUrl,
                        Parameter = param.Name,
                        OriginalValue = param.Value.ToString(),
                        TestedValue = testId.ToString(),
                        Type = IDORType.CommonIDAccess,
                        Severity = DetermineSeverity(response, baseline),
                        Evidence = ExtractEvidence(response),
                        StatusCode = response.StatusCode,
                        Timestamp = DateTime.Now
                    });
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<IDORVulnerability>> TestSpecialValues(string url, IDParameter param, HttpResponse baseline, IDORScanOptions options)
        {
            var vulnerabilities = new List<IDORVulnerability>();
            var specialValues = new[]
            {
                "0", "-1", "999999", "admin", "root", "test",
                "../1", "../../1", "1'", "1\"", "1--", "1#"
            };

            foreach (var testValue in specialValues)
            {
                var testUrl = ReplaceIDInUrl(url, param.Name, testValue);
                var response = await GetResponse(testUrl);

                if (response != null && response.StatusCode == 200 && response.Content.Length > 100)
                {
                    vulnerabilities.Add(new IDORVulnerability
                    {
                        Url = testUrl,
                        Parameter = param.Name,
                        OriginalValue = param.Value.ToString(),
                        TestedValue = testValue,
                        Type = IDORType.SpecialValueAccess,
                        Severity = "High",
                        Evidence = ExtractEvidence(response),
                        StatusCode = response.StatusCode,
                        Timestamp = DateTime.Now
                    });
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private List<IDParameter> ExtractIDParameters(string url)
        {
            var parameters = new List<IDParameter>();
            var uri = new Uri(url);

            // Check query parameters
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = uri.Query.TrimStart('?');
                var parts = query.Split('&');

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        var name = keyValue[0];
                        var value = keyValue[1];

                        // Check if parameter name suggests it's an ID
                        if (IsIDParameter(name) && int.TryParse(value, out int idValue))
                        {
                            parameters.Add(new IDParameter
                            {
                                Name = name,
                                Value = idValue,
                                Location = "Query"
                            });
                        }
                    }
                }
            }

            // Check path segments for numeric IDs
            var pathSegments = uri.AbsolutePath.Split('/');
            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (int.TryParse(pathSegments[i], out int idValue))
                {
                    parameters.Add(new IDParameter
                    {
                        Name = $"PathSegment{i}",
                        Value = idValue,
                        Location = "Path"
                    });
                }
            }

            return parameters;
        }

        private bool IsIDParameter(string name)
        {
            var idPatterns = new[] { "id", "uid", "user", "userid", "account", "profile", "item", "product", "order", "invoice", "ticket" };
            return idPatterns.Any(pattern => name.ToLower().Contains(pattern));
        }

        private async Task<HttpResponse> GetResponse(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                return new HttpResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content,
                    ContentLength = content.Length
                };
            }
            catch
            {
                return null;
            }
        }

        private bool IsIDORVulnerable(HttpResponse baseline, HttpResponse test, IDORScanOptions options)
        {
            // Must return 200 OK
            if (test.StatusCode != 200) return false;

            // Must have significant content (not just error page)
            if (test.ContentLength < 100) return false;

            // Content should be different from baseline (accessing different data)
            var similarity = CalculateSimilarity(baseline.Content, test.Content);
            if (similarity > 0.95) return false; // Too similar, probably same data

            // Content should have meaningful data patterns
            if (HasDataPatterns(test.Content))
            {
                // Check if it's not an error page
                if (!IsErrorPage(test.Content))
                {
                    return true;
                }
            }

            return false;
        }

        private double CalculateSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            var len1 = str1.Length;
            var len2 = str2.Length;
            var maxLen = Math.Max(len1, len2);

            if (maxLen == 0) return 1.0;

            var commonLength = str1.Take(Math.Min(len1, len2))
                .Zip(str2, (c1, c2) => c1 == c2)
                .Count(match => match);

            return (double)commonLength / maxLen;
        }

        private bool HasDataPatterns(string content)
        {
            // Check for common data indicators
            var dataPatterns = new[]
            {
                @"\bemail\b.*?@",
                @"\bphone\b.*?\d{3}",
                @"\baddress\b",
                @"\bname\b.*?[A-Z][a-z]+",
                @"""id"":\s*\d+",
                @"""user"":",
                @"""data"":",
                @"<td>.*?</td>"
            };

            return dataPatterns.Any(pattern => Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase));
        }

        private bool IsErrorPage(string content)
        {
            var errorPatterns = new[] { "error", "not found", "404", "403", "unauthorized", "access denied", "forbidden" };
            var lowerContent = content.ToLower();
            return errorPatterns.Any(pattern => lowerContent.Contains(pattern));
        }

        private string DetermineSeverity(HttpResponse response, HttpResponse baseline)
        {
            // Check for sensitive data in response
            var sensitivePatterns = new[]
            {
                @"\bemail\b.*?@",
                @"\bpassword\b",
                @"\bcredit\s*card\b",
                @"\bssn\b",
                @"\bapi[_\s]*key\b"
            };

            if (sensitivePatterns.Any(pattern => Regex.IsMatch(response.Content, pattern, RegexOptions.IgnoreCase)))
                return "Critical";

            return "High";
        }

        private string ExtractEvidence(HttpResponse response)
        {
            var content = response.Content;
            if (content.Length > 200)
            {
                return content.Substring(0, 200) + "...";
            }
            return content;
        }

        private string ReplaceIDInUrl(string url, string paramName, string newValue)
        {
            var uri = new Uri(url);

            // Replace in query parameters
            if (uri.Query.Contains(paramName))
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                queryParams[paramName] = newValue;

                var builder = new UriBuilder(uri)
                {
                    Query = queryParams.ToString()
                };

                return builder.ToString();
            }

            // Replace in path
            if (paramName.StartsWith("PathSegment"))
            {
                var index = int.Parse(paramName.Replace("PathSegment", ""));
                var pathSegments = uri.AbsolutePath.Split('/');
                if (index < pathSegments.Length)
                {
                    pathSegments[index] = newValue;
                    var newPath = string.Join("/", pathSegments);
                    return uri.Scheme + "://" + uri.Host + newPath;
                }
            }

            return url;
        }

        public string GenerateReport(IDORScanReport report)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("          IDOR VULNERABILITY SCAN REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Target URL: {report.TargetUrl}");
            sb.AppendLine($"Scan Time: {report.StartTime}");
            sb.AppendLine($"Duration: {report.Duration:F2} seconds");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine();
            sb.AppendLine($"Total Tests: {report.TotalTests}");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.Vulnerabilities.Any())
            {
                sb.AppendLine("VULNERABILITIES DETECTED:");
                sb.AppendLine("═════════════════════════════════════════════════");

                var grouped = report.Vulnerabilities.GroupBy(v => v.Type);
                foreach (var group in grouped)
                {
                    sb.AppendLine();
                    sb.AppendLine($"[{group.Key}] - {group.Count()} vulnerability(ies)");
                    sb.AppendLine(new string('-', 50));

                    foreach (var vuln in group.Take(10))
                    {
                        sb.AppendLine($"  Severity: {vuln.Severity}");
                        sb.AppendLine($"  Parameter: {vuln.Parameter}");
                        sb.AppendLine($"  Original Value: {vuln.OriginalValue}");
                        sb.AppendLine($"  Tested Value: {vuln.TestedValue}");
                        sb.AppendLine($"  URL: {vuln.Url}");
                        sb.AppendLine($"  Status: {vuln.StatusCode}");
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No IDOR vulnerabilities detected.");
            }

            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }

        public List<IDORScanResult> GetScanHistory() => _scanHistory;
    }

    public class IDORScanOptions
    {
        public int? SequentialRange { get; set; } = 5;
        public bool TestNegativeIDs { get; set; } = false;
        public int DelayBetweenRequests { get; set; } = 100;
    }

    public class IDORScanReport
    {
        public string TargetUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string Status { get; set; }
        public int TotalTests { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public List<IDORVulnerability> Vulnerabilities { get; set; }
    }

    public class IDORVulnerability
    {
        public string Url { get; set; }
        public string Parameter { get; set; }
        public string OriginalValue { get; set; }
        public string TestedValue { get; set; }
        public IDORType Type { get; set; }
        public string Severity { get; set; }
        public string Evidence { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class IDParameter
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Location { get; set; }
    }

    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; }
        public int ContentLength { get; set; }
    }

    public class IDORScanResult
    {
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public int TotalTests { get; set; }
    }

    public enum IDORType
    {
        SequentialAccess,
        CommonIDAccess,
        SpecialValueAccess
    }
}
