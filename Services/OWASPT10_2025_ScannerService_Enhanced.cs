using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Enhanced scanner methods for advanced vulnerability testing
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region Enhanced Injection Testing

        /// <summary>
        /// Advanced SQL Injection testing with multiple techniques
        /// </summary>
        private async Task<List<string>> TestAdvancedSQLInjectionAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing advanced SQL injection vectors on {url}...", CurrentCategory = "SQL Injection", Message = $"Testing advanced SQL injection vectors on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.SQLInjectionPayloads)
            {
                try
                {
                    // Test in query parameter
                    var testUrl = $"{url}?id={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectSQLInjectionVulnerability(content, response))
                    {
                        results.Add($"SQL Injection vulnerability detected with payload: {payload}");
                        results.Add(GenerateSQLInjectionPoC(url, payload, "GET"));
                    }

                    // Test in POST body
                    var postData = new StringContent(
                        $"{{\"id\":\"{payload}\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var postResponse = await _httpClient.PostAsync(url, postData);
                    var postContent = await postResponse.Content.ReadAsStringAsync();

                    if (DetectSQLInjectionVulnerability(postContent, postResponse))
                    {
                        results.Add($"SQL Injection vulnerability detected in POST with payload: {payload}");
                        results.Add(GenerateSQLInjectionPoC(url, payload, "POST"));
                    }

                    await Task.Delay(100); // Rate limiting
                }
                catch (Exception ex)
                {
                    // Log but continue testing
                    System.Diagnostics.Debug.WriteLine($"Error testing SQL injection: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Advanced NoSQL Injection testing
        /// </summary>
        private async Task<List<string>> TestAdvancedNoSQLInjectionAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing advanced NoSQL injection vectors on {url}...", CurrentCategory = "NoSQL Injection", Message = "Testing advanced NoSQL injection vectors on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.NoSQLInjectionPayloads)
            {
                try
                {
                    // Test in query parameter
                    var testUrl = $"{url}?username={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectNoSQLInjectionVulnerability(content, response))
                    {
                        results.Add($"NoSQL Injection vulnerability detected with payload: {payload}");
                        results.Add(GenerateNoSQLInjectionPoC(url, payload));
                    }

                    // Test in JSON body
                    var jsonPayload = $"{{\"username\":{payload},\"password\":\"test\"}}";
                    var postData = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var postResponse = await _httpClient.PostAsync(url, postData);
                    var postContent = await postResponse.Content.ReadAsStringAsync();

                    if (DetectNoSQLInjectionVulnerability(postContent, postResponse))
                    {
                        results.Add($"NoSQL Injection vulnerability detected in JSON with payload: {payload}");
                        results.Add(GenerateNoSQLInjectionPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing NoSQL injection: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Advanced XSS testing with WAF bypass techniques
        /// </summary>
        private async Task<List<string>> TestAdvancedXSSAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing advanced XSS vectors on {url}...", CurrentCategory = "XSS", Message = "Testing advanced XSS vectors on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.XSSPayloads)
            {
                try
                {
                    // Test reflected XSS
                    var testUrl = $"{url}?search={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (content.Contains(payload) && !IsXSSProperlyEscaped(content, payload))
                    {
                        results.Add($"XSS vulnerability detected with payload: {payload}");
                        results.Add(GenerateXSSPoC(url, payload, "reflected"));
                    }

                    // Test stored XSS (POST)
                    var postData = new StringContent(
                        $"{{\"comment\":\"{payload}\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    await _httpClient.PostAsync(url, postData);

                    // Verify if stored
                    var verifyResponse = await _httpClient.GetAsync(url);
                    var verifyContent = await verifyResponse.Content.ReadAsStringAsync();

                    if (verifyContent.Contains(payload) && !IsXSSProperlyEscaped(verifyContent, payload))
                    {
                        results.Add($"Stored XSS vulnerability detected with payload: {payload}");
                        results.Add(GenerateXSSPoC(url, payload, "stored"));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing XSS: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Advanced Command Injection testing
        /// </summary>
        private async Task<List<string>> TestAdvancedCommandInjectionAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing advanced command injection vectors on {url}...", CurrentCategory = "Command Injection", Message = "Testing advanced command injection vectors on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.CommandInjectionPayloads)
            {
                try
                {
                    var testUrl = $"{url}?cmd={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectCommandInjectionVulnerability(content, response))
                    {
                        results.Add($"Command Injection vulnerability detected with payload: {payload}");
                        results.Add(GenerateCommandInjectionPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing command injection: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// LDAP Injection testing
        /// </summary>
        private async Task<List<string>> TestLDAPInjectionAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing LDAP injection vectors on {url}...", CurrentCategory = "LDAP Injection", Message = "Testing LDAP injection vectors on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.LDAPInjectionPayloads)
            {
                try
                {
                    var testUrl = $"{url}?username={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectLDAPInjectionVulnerability(content, response))
                    {
                        results.Add($"LDAP Injection vulnerability detected with payload: {payload}");
                        results.Add(GenerateLDAPInjectionPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing LDAP injection: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// XXE (XML External Entity) testing
        /// </summary>
        private async Task<List<string>> TestXXEAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing XXE vulnerabilities on {url}...", CurrentCategory = "XXE", Message = "Testing XXE vulnerabilities on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.XXEPayloads)
            {
                try
                {
                    var xmlContent = new StringContent(payload, Encoding.UTF8, "application/xml");
                    var response = await _httpClient.PostAsync(url, xmlContent);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectXXEVulnerability(content, response))
                    {
                        results.Add($"XXE vulnerability detected");
                        results.Add(GenerateXXEPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing XXE: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// SSRF (Server-Side Request Forgery) testing
        /// </summary>
        private async Task<List<string>> TestSSRFAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing SSRF vulnerabilities on {url}...", CurrentCategory = "SSRF", Message = "Testing SSRF vulnerabilities on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.SSRFPayloads)
            {
                try
                {
                    var testUrl = $"{url}?url={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectSSRFVulnerability(content, response, payload))
                    {
                        results.Add($"SSRF vulnerability detected with target: {payload}");
                        results.Add(GenerateSSRFPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing SSRF: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// SSTI (Server-Side Template Injection) testing
        /// </summary>
        private async Task<List<string>> TestSSTIAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing SSTI vulnerabilities on {url}...", CurrentCategory = "SSTI", Message = "Testing SSTI vulnerabilities on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.SSTIPayloads)
            {
                try
                {
                    var testUrl = $"{url}?template={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectSSTIVulnerability(content, response, payload))
                    {
                        results.Add($"SSTI vulnerability detected with payload: {payload}");
                        results.Add(GenerateSSTIPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing SSTI: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Path Traversal testing
        /// </summary>
        private async Task<List<string>> TestPathTraversalAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing path traversal vulnerabilities on {url}...", CurrentCategory = "Path Traversal", Message = "Testing path traversal vulnerabilities on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.PathTraversalPayloads)
            {
                try
                {
                    var testUrl = $"{url}?file={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectPathTraversalVulnerability(content, response))
                    {
                        results.Add($"Path Traversal vulnerability detected with payload: {payload}");
                        results.Add(GeneratePathTraversalPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing path traversal: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// File Inclusion (LFI/RFI) testing
        /// </summary>
        private async Task<List<string>> TestFileInclusionAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = "Testing file inclusion vulnerabilities on {url}...", CurrentCategory = "File Inclusion", Message = "Testing file inclusion vulnerabilities on {url}...", ProgressPercentage = 0 });

            foreach (var payload in AdvancedPayloads.FileInclusionPayloads)
            {
                try
                {
                    var testUrl = $"{url}?page={Uri.EscapeDataString(payload)}";
                    var response = await _httpClient.GetAsync(testUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    if (DetectFileInclusionVulnerability(content, response))
                    {
                        results.Add($"File Inclusion vulnerability detected with payload: {payload}");
                        results.Add(GenerateFileInclusionPoC(url, payload));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing file inclusion: {ex.Message}");
                }
            }

            return results;
        }

        #endregion

        #region Detection Helpers

        private bool DetectSQLInjectionVulnerability(string content, HttpResponseMessage response)
        {
            var sqlErrorPatterns = new[]
            {
                "SQL syntax",
                "mysql_fetch",
                "ORA-",
                "PostgreSQL",
                "SQLite",
                "Microsoft SQL Server",
                "ODBC",
                "You have an error in your SQL syntax"
            };

            return sqlErrorPatterns.Any(pattern =>
                content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private bool DetectNoSQLInjectionVulnerability(string content, HttpResponseMessage response)
        {
            var noSqlIndicators = new[]
            {
                "MongoError",
                "CouchDB",
                "authentication bypass",
                "\"ok\":1"
            };

            return noSqlIndicators.Any(indicator =>
                content.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsXSSProperlyEscaped(string content, string payload)
        {
            // Check if dangerous characters are escaped
            var escapedPayload = payload
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");

            return content.Contains(escapedPayload);
        }

        private bool DetectCommandInjectionVulnerability(string content, HttpResponseMessage response)
        {
            var commandOutputPatterns = new[]
            {
                "root:",
                "bin/bash",
                "Windows",
                "Volume Serial Number"
            };

            return commandOutputPatterns.Any(pattern =>
                content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private bool DetectLDAPInjectionVulnerability(string content, HttpResponseMessage response)
        {
            // Check for LDAP error messages or unexpected user data exposure
            return content.Contains("LDAP", StringComparison.OrdinalIgnoreCase) ||
                   content.Contains("directory", StringComparison.OrdinalIgnoreCase) ||
                   response.StatusCode == System.Net.HttpStatusCode.OK && content.Length > 1000;
        }

        private bool DetectXXEVulnerability(string content, HttpResponseMessage response)
        {
            return content.Contains("root:") ||
                   content.Contains("etc/passwd") ||
                   content.Contains("SYSTEM\\");
        }

        private bool DetectSSRFVulnerability(string content, HttpResponseMessage response, string payload)
        {
            // Check for metadata endpoints or internal network responses
            return content.Contains("aws") ||
                   content.Contains("metadata") ||
                   content.Contains("instance-id") ||
                   (payload.Contains("169.254.169.254") && response.IsSuccessStatusCode);
        }

        private bool DetectSSTIVulnerability(string content, HttpResponseMessage response, string payload)
        {
            // Check if template expression was evaluated
            if (payload.Contains("7*7") && content.Contains("49"))
                return true;

            if (payload.Contains("7*'7'") && content.Contains("7777777"))
                return true;

            return false;
        }

        private bool DetectPathTraversalVulnerability(string content, HttpResponseMessage response)
        {
            return content.Contains("root:") ||
                   content.Contains("[boot loader]") ||
                   content.Contains("etc/passwd");
        }

        private bool DetectFileInclusionVulnerability(string content, HttpResponseMessage response)
        {
            return content.Contains("root:") ||
                   content.Contains("<?php") ||
                   content.Contains("PD9waHA");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to test request with custom headers
        /// </summary>
        private async Task<HttpResponseMessage> TestRequestWithHeaders(string url, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return await _httpClient.SendAsync(request);
            }
        }

        #endregion
    }
}
