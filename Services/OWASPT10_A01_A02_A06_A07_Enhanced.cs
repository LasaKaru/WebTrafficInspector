using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Enhanced testing for A01, A02, A06, and A07 OWASP categories
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region Enhanced A01: Broken Access Control

        /// <summary>
        /// Comprehensive broken access control testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestBrokenAccessControlEnhanced(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // IDOR Testing (Enhanced)
            vulnerabilities.AddRange(await TestIDORAdvanced(url));

            // Path Traversal Testing (Enhanced)
            vulnerabilities.AddRange(await TestPathTraversalAdvanced(url));

            // Open Redirect Testing (New)
            vulnerabilities.AddRange(await TestOpenRedirect(url));

            // Missing Function Level Access Control (Enhanced)
            vulnerabilities.AddRange(await TestMissingFunctionLevelAccessControl(url));

            return vulnerabilities;
        }

        /// <summary>
        /// Advanced IDOR testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestIDORAdvanced(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();
            var parameters = ExtractParameters(url);

            var idorParamNames = new[] { "id", "user_id", "uid", "userid", "account", "accountid", "doc", "file", "key", "order", "invoice" };

            foreach (var param in parameters)
            {
                if (!idorParamNames.Any(name => param.ToLower().Contains(name)))
                    continue;

                // Get current value
                var currentValue = GetParameterValue(url, param);

                foreach (var testValue in AdvancedPayloads.IDORTestValues.Take(10))
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, testValue);
                        var result = await TestRequest(testUrl, "GET");

                        // Check if unauthorized access was granted
                        if (result.IsSuccessful && result.StatusCode == 200 &&
                            !string.IsNullOrEmpty(result.Response) &&
                            result.Response.Length > 100) // Has substantial content
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A01:2025 - Broken Access Control",
                                Type = "Insecure Direct Object Reference (IDOR)",
                                Severity = "High",
                                Description = $"IDOR vulnerability detected in parameter '{param}'. Able to access resource with value '{testValue}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"Successfully accessed resource by manipulating '{param}' from '{currentValue}' to '{testValue}'",
                                PoC = GenerateIDORPoCAdvanced(url, param, currentValue, testValue),
                                CWE = "CWE-639"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Advanced Path Traversal testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestPathTraversalAdvanced(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();
            var parameters = ExtractParameters(url);

            var pathParamNames = new[] { "file", "path", "folder", "dir", "document", "pdf", "download", "filename", "filepath" };

            foreach (var param in parameters)
            {
                if (!pathParamNames.Any(name => param.ToLower().Contains(name)))
                    continue;

                var pathPayloads = AdvancedPayloads.PathTraversalPayloads.Take(15);

                foreach (var payload in pathPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequest(testUrl, "GET");

                        if (IsPathTraversalVulnerable(result.Response, payload))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A01:2025 - Broken Access Control",
                                Type = "Path Traversal / Directory Traversal",
                                Severity = "High",
                                Description = $"Path traversal vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"Successfully accessed unauthorized file using payload: {payload}",
                                PoC = GeneratePathTraversalPoC(url, param, payload),
                                CWE = "CWE-22"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Open Redirect testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestOpenRedirect(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();
            var parameters = ExtractParameters(url);

            var redirectParamNames = new[] { "url", "redirect", "return", "returnto", "next", "goto", "destination", "redirect_uri", "return_url" };

            foreach (var param in parameters)
            {
                if (!redirectParamNames.Any(name => param.ToLower().Contains(name)))
                    continue;

                var redirectPayloads = AdvancedPayloads.OpenRedirectPayloads.Take(10);

                foreach (var payload in redirectPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequestWithHeaders(testUrl, "GET");

                        if (IsOpenRedirectVulnerable(result.Headers, result.StatusCode, payload))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A01:2025 - Broken Access Control",
                                Type = "Open Redirect",
                                Severity = "Medium",
                                Description = $"Open redirect vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = $"Status: {result.StatusCode}, Location: {result.Headers.GetValueOrDefault("Location", "N/A")}",
                                Evidence = $"Server redirected to external URL: {payload}",
                                PoC = GenerateOpenRedirectPoC(url, param, payload),
                                CWE = "CWE-601"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Missing Function Level Access Control testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestMissingFunctionLevelAccessControl(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();
            var baseUrl = GetBaseUrl(url);

            var adminPaths = new[]
            {
                "/admin", "/administrator", "/admin.php", "/admin/", "/admin/index.php",
                "/manage", "/manager", "/management",
                "/dashboard", "/panel", "/control", "/console",
                "/api/admin", "/api/users", "/api/internal",
                "/wp-admin", "/phpmyadmin", "/adminer.php",
                "/.env", "/config.php", "/backup", "/backup.sql"
            };

            foreach (var adminPath in adminPaths)
            {
                try
                {
                    var testUrl = baseUrl + adminPath;
                    var result = await TestRequest(testUrl, "GET");

                    // Check if accessible without authentication
                    if (result.IsSuccessful && result.StatusCode == 200 &&
                        !result.Response.Contains("login", StringComparison.OrdinalIgnoreCase) &&
                        !result.Response.Contains("forbidden", StringComparison.OrdinalIgnoreCase) &&
                        !result.Response.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A01:2025 - Broken Access Control",
                            Type = "Missing Function Level Access Control",
                            Severity = "Critical",
                            Description = $"Administrative or sensitive path '{adminPath}' is accessible without proper authorization",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                            Evidence = $"Path '{adminPath}' returned HTTP 200 without authentication requirement",
                            PoC = GenerateMissingFunctionLevelAccessControlPoC(testUrl, adminPath),
                            CWE = "CWE-284"
                        });
                    }
                }
                catch { continue; }
            }

            return vulnerabilities;
        }

        #endregion

        #region Enhanced A02: Security Misconfiguration

        /// <summary>
        /// Comprehensive security misconfiguration testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSecurityMisconfigurationEnhanced(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Security Headers Testing (Enhanced)
            vulnerabilities.AddRange(await TestSecurityHeadersAdvanced(url));

            // CORS Misconfiguration Testing (New)
            vulnerabilities.AddRange(await TestCORSMisconfiguration(url));

            // Clickjacking Testing (New)
            vulnerabilities.AddRange(await TestClickjacking(url));

            // HTTP Smuggling Detection (New)
            vulnerabilities.AddRange(await TestHTTPSmuggling(url));

            // Verbose Error Messages (Enhanced)
            vulnerabilities.AddRange(await TestVerboseErrors(url));

            return vulnerabilities;
        }

        /// <summary>
        /// Advanced security headers testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSecurityHeadersAdvanced(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var result = await TestRequestWithHeaders(url, "GET");

                var criticalHeaders = new Dictionary<string, string>
                {
                    { "Strict-Transport-Security", "HSTS not enabled - Site vulnerable to SSL stripping attacks" },
                    { "X-Frame-Options", "Clickjacking protection not enabled" },
                    { "X-Content-Type-Options", "MIME-sniffing protection not enabled" },
                    { "Content-Security-Policy", "CSP not implemented - XSS protection weakened" },
                    { "X-XSS-Protection", "XSS filter not enabled (legacy but still useful)" },
                    { "Referrer-Policy", "Referrer policy not set - Information leakage risk" },
                    { "Permissions-Policy", "Permissions policy not set - Feature access not restricted" }
                };

                foreach (var header in criticalHeaders)
                {
                    if (!result.Headers.ContainsKey(header.Key))
                    {
                        var severity = header.Key == "Strict-Transport-Security" || header.Key == "Content-Security-Policy"
                            ? "High" : "Medium";

                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = $"Missing Security Header: {header.Key}",
                            Severity = severity,
                            Description = header.Value,
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = $"Headers present: {string.Join(", ", result.Headers.Keys)}",
                            Evidence = $"Security header '{header.Key}' is missing from response",
                            PoC = GenerateMissingSecurityHeaderPoC(url, header.Key, header.Value),
                            CWE = "CWE-16"
                        });
                    }
                }

                // Check for insecure header values
                if (result.Headers.TryGetValue("X-Frame-Options", out var xFrameValue))
                {
                    if (xFrameValue.Equals("ALLOW", StringComparison.OrdinalIgnoreCase) ||
                        xFrameValue.Equals("ALLOWALL", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = "Insecure X-Frame-Options Configuration",
                            Severity = "Medium",
                            Description = "X-Frame-Options is set to allow framing, making the site vulnerable to clickjacking",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = $"X-Frame-Options: {xFrameValue}",
                            Evidence = $"X-Frame-Options header value '{xFrameValue}' allows framing",
                            PoC = GenerateInsecureHeaderPoC(url, "X-Frame-Options", xFrameValue),
                            CWE = "CWE-1021"
                        });
                    }
                }

                // Check for server information disclosure
                if (result.Headers.TryGetValue("Server", out var serverValue))
                {
                    if (serverValue.Contains("/") || serverValue.Contains("Apache") || serverValue.Contains("nginx"))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = "Server Information Disclosure",
                            Severity = "Low",
                            Description = "Server header reveals version information",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = $"Server: {serverValue}",
                            Evidence = $"Server header exposes: {serverValue}",
                            PoC = GenerateServerDisclosurePoC(url, serverValue),
                            CWE = "CWE-200"
                        });
                    }
                }

                // Check for X-Powered-By header
                if (result.Headers.TryGetValue("X-Powered-By", out var poweredByValue))
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A02:2025 - Security Misconfiguration",
                        Type = "Technology Stack Disclosure",
                        Severity = "Low",
                        Description = "X-Powered-By header reveals technology stack",
                        TestUrl = url,
                        TestRequest = result.Request,
                        TestResponse = $"X-Powered-By: {poweredByValue}",
                        Evidence = $"Technology disclosed: {poweredByValue}",
                        PoC = GenerateTechDisclosurePoC(url, poweredByValue),
                        CWE = "CWE-200"
                    });
                }
            }
            catch { }

            return vulnerabilities;
        }

        /// <summary>
        /// CORS misconfiguration testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestCORSMisconfiguration(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var origin in AdvancedPayloads.CORSTestOrigins.Take(5))
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Origin", origin);

                    var response = await _httpClient.SendAsync(request);
                    var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

                    if (headers.TryGetValue("Access-Control-Allow-Origin", out var allowOrigin))
                    {
                        // Check for dangerous CORS configurations
                        if (allowOrigin == "*" ||
                            allowOrigin == "null" ||
                            allowOrigin == origin)
                        {
                            var severity = (allowOrigin == "*" && headers.ContainsKey("Access-Control-Allow-Credentials"))
                                ? "Critical" : "High";

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A02:2025 - Security Misconfiguration",
                                Type = "CORS Misconfiguration",
                                Severity = severity,
                                Description = $"Insecure CORS policy allows origin: {allowOrigin}",
                                TestUrl = url,
                                TestRequest = $"GET {url}\nOrigin: {origin}",
                                TestResponse = $"Access-Control-Allow-Origin: {allowOrigin}",
                                Evidence = $"CORS policy allows potentially malicious origin: {allowOrigin}",
                                PoC = GenerateCORSMisconfigurationPoC(url, origin, allowOrigin),
                                CWE = "CWE-942"
                            });
                            break;
                        }
                    }
                }
                catch { continue; }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Clickjacking vulnerability testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestClickjacking(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var result = await TestRequestWithHeaders(url, "GET");

                // Check if X-Frame-Options or CSP frame-ancestors is missing
                var hasXFrameOptions = result.Headers.ContainsKey("X-Frame-Options");
                var hasCSPFrameAncestors = result.Headers.TryGetValue("Content-Security-Policy", out var cspValue) &&
                                          cspValue.Contains("frame-ancestors");

                if (!hasXFrameOptions && !hasCSPFrameAncestors)
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A02:2025 - Security Misconfiguration",
                        Type = "Clickjacking Vulnerability",
                        Severity = "Medium",
                        Description = "Application is vulnerable to clickjacking attacks - No frame protection headers",
                        TestUrl = url,
                        TestRequest = result.Request,
                        TestResponse = "No X-Frame-Options or CSP frame-ancestors directive found",
                        Evidence = "Neither X-Frame-Options nor CSP frame-ancestors protection is implemented",
                        PoC = GenerateClickjackingPoC(url),
                        CWE = "CWE-1021"
                    });
                }
            }
            catch { }

            return vulnerabilities;
        }

        /// <summary>
        /// HTTP Request Smuggling detection
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestHTTPSmuggling(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Test for CL.TE smuggling
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent("0\r\n\r\nX", Encoding.UTF8);
                request.Content.Headers.Add("Transfer-Encoding", "chunked");
                request.Content.Headers.ContentLength = 6;

                var stopwatch = Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request);
                stopwatch.Stop();

                // If server hangs or responds unusually, might be vulnerable
                if (stopwatch.ElapsedMilliseconds > 5000 || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A02:2025 - Security Misconfiguration",
                        Type = "Potential HTTP Request Smuggling (CL.TE)",
                        Severity = "Critical",
                        Description = "Server may be vulnerable to HTTP request smuggling attacks",
                        TestUrl = url,
                        TestRequest = "POST with conflicting Content-Length and Transfer-Encoding",
                        TestResponse = $"Status: {response.StatusCode}, Time: {stopwatch.ElapsedMilliseconds}ms",
                        Evidence = "Abnormal response to smuggling probe detected",
                        PoC = GenerateHTTPSmugglingPoC(url),
                        CWE = "CWE-444"
                    });
                }
            }
            catch { }

            return vulnerabilities;
        }

        /// <summary>
        /// Verbose error message testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestVerboseErrors(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Test various error conditions
            var errorTests = new[]
            {
                (url + "/nonexistent" + Guid.NewGuid(), "404 error"),
                (url + "?param='", "SQL error"),
                (url + "?param=<test>", "XSS reflection")
            };

            foreach (var (testUrl, errorType) in errorTests)
            {
                try
                {
                    var result = await TestRequest(testUrl, "GET");

                    if (IsVerboseError(result.Response))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = "Verbose Error Messages",
                            Severity = "Low",
                            Description = $"Application exposes verbose error messages revealing internal details",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response?.Substring(0, Math.Min(500, result.Response?.Length ?? 0)),
                            Evidence = "Stack trace or detailed error information exposed",
                            PoC = GenerateVerboseErrorPoC(testUrl, errorType),
                            CWE = "CWE-209"
                        });
                        break;
                    }
                }
                catch { continue; }
            }

            return vulnerabilities;
        }

        #endregion

        #region Enhanced A07: Authentication Failures

        /// <summary>
        /// Comprehensive authentication testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestAuthenticationFailuresEnhanced(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // JWT vulnerabilities (New)
            vulnerabilities.AddRange(await TestJWTVulnerabilities(url, originalEntry));

            // CSRF Testing (New)
            vulnerabilities.AddRange(await TestCSRF(url, originalEntry));

            // Weak Credentials (Enhanced)
            vulnerabilities.AddRange(await TestWeakCredentialsAdvanced(url));

            // Session Fixation (Enhanced)
            vulnerabilities.AddRange(await TestSessionFixation(url));

            return vulnerabilities;
        }

        /// <summary>
        /// JWT vulnerability testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestJWTVulnerabilities(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Check if JWT is present in request/response
            var jwtPattern = @"eyJ[A-Za-z0-9-_]+\.eyJ[A-Za-z0-9-_]+\.[A-Za-z0-9-_.+/]*";
            var responseHasJWT = originalEntry?.RawResponse != null &&
                                System.Text.RegularExpressions.Regex.IsMatch(originalEntry.RawResponse, jwtPattern);

            if (responseHasJWT)
            {
                // Test none algorithm attack
                var noneAlgJWT = AdvancedPayloads.JWTAttackPayloads[0]; // None algorithm JWT

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("Authorization", $"Bearer {noneAlgJWT}");

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "JWT None Algorithm Vulnerability",
                            Severity = "Critical",
                            Description = "Application accepts JWT tokens with 'none' algorithm, allowing authentication bypass",
                            TestUrl = url,
                            TestRequest = $"GET {url}\nAuthorization: Bearer {noneAlgJWT}",
                            TestResponse = $"Status: {response.StatusCode}",
                            Evidence = "JWT with 'none' algorithm was accepted",
                            PoC = GenerateJWTNoneAlgorithmPoC(url),
                            CWE = "CWE-287"
                        });
                    }
                }
                catch { }

                // Note weak secrets (would require brute force)
                vulnerabilities.Add(new OWASPT10Vulnerability
                {
                    Category = "A07:2025 - Authentication Failures",
                    Type = "Potential Weak JWT Secret",
                    Severity = "High",
                    Description = "JWT tokens detected - Secret key should be tested for weakness",
                    TestUrl = url,
                    TestRequest = originalEntry?.RawRequest ?? "N/A",
                    TestResponse = "JWT detected in response",
                    Evidence = "JWT implementation found - Secret strength should be verified",
                    PoC = GenerateJWTWeakSecretPoC(url),
                    CWE = "CWE-521"
                });
            }

            return vulnerabilities;
        }

        /// <summary>
        /// CSRF vulnerability testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestCSRF(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Check if state-changing endpoint lacks CSRF protection
            if (originalEntry?.Method == "POST" || originalEntry?.Method == "PUT" || originalEntry?.Method == "DELETE")
            {
                try
                {
                    var result = await TestRequestWithHeaders(url, originalEntry.Method);

                    // Check for CSRF token in response
                    var hasCsrfToken = result.Response?.Contains("csrf", StringComparison.OrdinalIgnoreCase) ?? false;
                    var hasCsrfHeader = result.Headers.Any(h => h.Key.Contains("csrf", StringComparison.OrdinalIgnoreCase));

                    if (!hasCsrfToken && !hasCsrfHeader)
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "Missing CSRF Protection",
                            Severity = "High",
                            Description = $"State-changing endpoint ({originalEntry.Method}) lacks CSRF protection",
                            TestUrl = url,
                            TestRequest = originalEntry.RawRequest,
                            TestResponse = "No CSRF token detected",
                            Evidence = $"{originalEntry.Method} request without CSRF protection",
                            PoC = GenerateCSRFPoC(url, originalEntry.Method),
                            CWE = "CWE-352"
                        });
                    }
                }
                catch { }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Advanced weak credentials testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestWeakCredentialsAdvanced(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();
            var loginEndpoint = FindLoginEndpoint(url);

            if (loginEndpoint == null)
                return vulnerabilities;

            var weakCredentials = new[]
            {
                ("admin", "admin"), ("admin", "password"), ("admin", "123456"),
                ("administrator", "administrator"), ("root", "root"), ("root", "toor"),
                ("user", "user"), ("test", "test"), ("guest", "guest"),
                ("admin", "admin123"), ("admin", "P@ssw0rd")
            };

            foreach (var (username, password) in weakCredentials.Take(5)) // Limit to prevent lockouts
            {
                try
                {
                    var result = await TestLogin(loginEndpoint, username, password);

                    if (result.IsSuccessful && IsLoginSuccessful(result.Response))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "Weak Default Credentials",
                            Severity = "Critical",
                            Description = $"Application accepts weak credentials: {username}/{password}",
                            TestUrl = loginEndpoint,
                            TestRequest = result.Request,
                            TestResponse = "Login successful",
                            Evidence = $"Authentication successful with credentials: {username}/{password}",
                            PoC = GenerateWeakCredentialsPoC(loginEndpoint, username, password),
                            CWE = "CWE-798"
                        });
                        break;
                    }

                    // Small delay to avoid triggering rate limiting
                    await Task.Delay(500);
                }
                catch { continue; }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Session fixation testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSessionFixation(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // First request to get session
                var result1 = await TestRequestWithHeaders(url, "GET");
                var sessionId1 = ExtractSessionId(result1.Headers);

                if (!string.IsNullOrEmpty(sessionId1))
                {
                    // Second request with same session
                    var request2 = new HttpRequestMessage(HttpMethod.Get, url);
                    request2.Headers.Add("Cookie", $"PHPSESSID={sessionId1}");

                    var response2 = await _httpClient.SendAsync(request2);
                    var headers2 = response2.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
                    var sessionId2 = ExtractSessionId(headers2);

                    // If session ID doesn't change, vulnerable to fixation
                    if (sessionId1 == sessionId2)
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "Session Fixation Vulnerability",
                            Severity = "High",
                            Description = "Application doesn't regenerate session ID after authentication",
                            TestUrl = url,
                            TestRequest = $"GET {url}\nCookie: PHPSESSID={sessionId1}",
                            TestResponse = $"Session ID remained: {sessionId2}",
                            Evidence = "Session ID was not regenerated",
                            PoC = GenerateSessionFixationPoC(url),
                            CWE = "CWE-384"
                        });
                    }
                }
            }
            catch { }

            return vulnerabilities;
        }

        #endregion

        #region Detection Helper Methods

        private bool IsPathTraversalVulnerable(string response, string payload)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Check for typical file disclosure
            var indicators = new[]
            {
                "root:x:", "/bin/bash", "win.ini", "[boot loader]",
                "[extensions]", "[fonts]", "System.IO.FileNotFoundException"
            };

            return indicators.Any(indicator => response.Contains(indicator));
        }

        private bool IsOpenRedirectVulnerable(Dictionary<string, string> headers, int statusCode, string payload)
        {
            // Check if status code is a redirect
            if (statusCode != 301 && statusCode != 302 && statusCode != 303 && statusCode != 307 && statusCode != 308)
                return false;

            // Check if Location header points to external site
            if (headers.TryGetValue("Location", out var location))
            {
                return location.Contains("evil.com") || location.Contains("attacker.com") ||
                       location.StartsWith("http://") && !location.Contains("localhost");
            }

            return false;
        }

        private bool IsVerboseError(string response)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            var verboseErrorPatterns = new[]
            {
                "Stack trace:", "at ", ".java:", ".cs:",
                "System.Exception", "printStackTrace",
                "Fatal error:", "Warning:", "Notice:",
                "Debug mode", "Development mode",
                "Application error", "Internal Server Error",
                "Database error", "Query failed"
            };

            return verboseErrorPatterns.Any(pattern =>
                response.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private string GetParameterValue(string url, string param)
        {
            try
            {
                var uri = new Uri(url);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                return queryParams[param] ?? "";
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Advanced PoC Generators (continued next message due to length)

        // Implementation continued in next part...
        #endregion
    }
}
