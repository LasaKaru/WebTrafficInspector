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
    /// Enhanced testing methods for specific OWASP Top 10 2025 categories
    /// A01: Broken Access Control
    /// A02: Cryptographic Failures
    /// A06: Vulnerable and Outdated Components
    /// A07: Identification and Authentication Failures
    /// </summary>
    public partial class OWASPT10_2025_ScannerService
    {
        #region A01: Enhanced Broken Access Control Testing

        /// <summary>
        /// Enhanced IDOR testing with multiple ID formats
        /// </summary>
        private async Task<List<string>> TestEnhancedIDORAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing enhanced IDOR vulnerabilities on {url}...", CurrentCategory = "IDOR", Message = $"Testing enhanced IDOR vulnerabilities on {url}...", ProgressPercentage = 0 });

            // Extract current ID from URL if present
            var currentId = ExtractIdFromUrl(url);

            foreach (var testValue in AdvancedPayloads.IDORTestValues)
            {
                try
                {
                    // Test path parameter
                    var testUrl = url.Contains("{id}")
                        ? url.Replace("{id}", testValue)
                        : $"{url}/{testValue}";

                    var response = await _httpClient.GetAsync(testUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (DetectIDORVulnerability(content, response, testValue, currentId))
                        {
                            results.Add($"IDOR vulnerability detected accessing: {testValue}");
                            results.Add(GenerateIDORPoC(url, testValue, currentId));
                        }
                    }

                    // Test query parameter variations
                    var queryUrls = new[]
                    {
                        $"{url}?id={testValue}",
                        $"{url}?user_id={testValue}",
                        $"{url}?userId={testValue}",
                        $"{url}?account={testValue}"
                    };

                    foreach (var queryUrl in queryUrls)
                    {
                        var queryResponse = await _httpClient.GetAsync(queryUrl);

                        if (queryResponse.IsSuccessStatusCode)
                        {
                            var content = await queryResponse.Content.ReadAsStringAsync();

                            if (DetectIDORVulnerability(content, queryResponse, testValue, currentId))
                            {
                                results.Add($"IDOR vulnerability detected in query parameter with ID: {testValue}");
                                results.Add(GenerateIDORPoC(queryUrl, testValue, currentId));
                            }
                        }
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing IDOR: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Enhanced CORS misconfiguration testing
        /// </summary>
        private async Task<List<string>> TestEnhancedCORSAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing enhanced CORS misconfigurations on {url}...", CurrentCategory = "CORS", Message = $"Testing enhanced CORS misconfigurations on {url}...", ProgressPercentage = 0 });

            foreach (var origin in AdvancedPayloads.CORSTestOrigins)
            {
                try
                {
                    var headers = new Dictionary<string, string>
                    {
                        { "Origin", origin }
                    };

                    var response = await TestRequestWithHeaders(url, headers);

                    if (response.Headers.Contains("Access-Control-Allow-Origin"))
                    {
                        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();

                        if (allowedOrigin == "*" || allowedOrigin == origin || allowedOrigin == "null")
                        {
                            var hasCredentials = response.Headers.Contains("Access-Control-Allow-Credentials") &&
                                               response.Headers.GetValues("Access-Control-Allow-Credentials")
                                                       .FirstOrDefault() == "true";

                            results.Add($"CORS misconfiguration detected: Origin {origin} is allowed");

                            if (hasCredentials)
                            {
                                results.Add("CRITICAL: Credentials are allowed with CORS!");
                            }

                            results.Add(GenerateCORSPoC(url, origin, allowedOrigin));
                        }
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing CORS: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Test for path-based access control bypass
        /// </summary>
        private async Task<List<string>> TestPathBasedAccessControlAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing path-based access control bypass on {url}...", CurrentCategory = "Access Control", Message = $"Testing path-based access control bypass on {url}...", ProgressPercentage = 0 });

            var bypassTechniques = new[]
            {
                url.Replace("/admin", "/admin/"),
                url.Replace("/admin", "/admin.."),
                url.Replace("/admin", "/./admin"),
                url.Replace("/admin", "/admin/."),
                url.Replace("/admin", "//admin"),
                url + "/.",
                url + "/..",
                url.ToUpper(),
                url.Replace("/", "\\")
            };

            foreach (var testUrl in bypassTechniques)
            {
                try
                {
                    var response = await _httpClient.GetAsync(testUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        results.Add($"Path-based access control bypass: {testUrl}");
                        results.Add(GenerateAccessControlBypassPoC(url, testUrl));
                    }

                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing path bypass: {ex.Message}");
                }
            }

            return results;
        }

        #endregion

        #region A02: Enhanced Cryptographic Failures Testing

        /// <summary>
        /// Enhanced weak cryptography detection
        /// </summary>
        private async Task<List<string>> TestEnhancedCryptographicFailuresAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing enhanced cryptographic failures on {url}...", CurrentCategory = "Cryptographic Failures", Message = $"Testing enhanced cryptographic failures on {url}...", ProgressPercentage = 0 });

            // Test weak encryption algorithms
            results.AddRange(await TestWeakEncryptionAsync(url));

            // Test for exposed sensitive data
            results.AddRange(await TestSensitiveDataExposureAsync(url));

            // Test weak hashing
            results.AddRange(await TestWeakHashingAsync(url));

            return results;
        }

        private async Task<List<string>> TestWeakEncryptionAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                var weakAlgorithms = new[]
                {
                    "DES", "RC4", "MD5", "SHA1", "ECB",
                    "des_", "rc4_", "md5(", "sha1("
                };

                foreach (var algorithm in weakAlgorithms)
                {
                    if (content.Contains(algorithm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add($"Weak cryptographic algorithm detected: {algorithm}");
                        results.Add(GenerateWeakCryptoPoC(url, algorithm));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing weak encryption: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> TestSensitiveDataExposureAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                var sensitivePatterns = new Dictionary<string, string>
                {
                    { "password", @"password[""']?\s*[:=]\s*[""']?[\w@#$%]+[""']?" },
                    { "api_key", @"api[_-]?key[""']?\s*[:=]\s*[""']?[\w-]+[""']?" },
                    { "secret", @"secret[""']?\s*[:=]\s*[""']?[\w-]+[""']?" },
                    { "token", @"token[""']?\s*[:=]\s*[""']?[\w.-]+[""']?" },
                    { "credit_card", @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b" },
                    { "ssn", @"\b\d{3}-\d{2}-\d{4}\b" }
                };

                foreach (var pattern in sensitivePatterns)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern.Value,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        results.Add($"Sensitive data exposure detected: {pattern.Key}");
                        results.Add(GenerateSensitiveDataExposurePoC(url, pattern.Key));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing sensitive data exposure: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> TestWeakHashingAsync(string url)
        {
            var results = new List<string>();

            try
            {
                // Test password reset functionality
                var resetUrl = $"{url}/reset-password";
                var response = await _httpClient.GetAsync(resetUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Check for predictable tokens
                    if (content.Contains("token=") &&
                        (content.Contains("md5") || content.Length < 32))
                    {
                        results.Add("Weak password reset token detected");
                        results.Add(GenerateWeakHashingPoC(url));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing weak hashing: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region A06: Enhanced Vulnerable Components Testing

        /// <summary>
        /// Enhanced detection of vulnerable and outdated components
        /// </summary>
        private async Task<List<string>> TestEnhancedVulnerableComponentsAsync(string url)
        {
            var results = new List<string>();

            //OnScanProgress?.Invoke($"Testing for vulnerable components on {url}...");
            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing enhanced CORS misconfigurations on {url}...", CurrentCategory = "CORS", Message = $"Testing enhanced CORS misconfigurations on {url}...", ProgressPercentage = 0 });

            // Test for known vulnerable libraries
            results.AddRange(await DetectVulnerableLibrariesAsync(url));

            // Test for outdated frameworks
            results.AddRange(await DetectOutdatedFrameworksAsync(url));

            // Test for vulnerable dependencies
            results.AddRange(await DetectVulnerableDependenciesAsync(url));

            return results;
        }

        private async Task<List<string>> DetectVulnerableLibrariesAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                var vulnerableLibraries = new Dictionary<string, string>
                {
                    { "jQuery 1.", "jQuery version 1.x (vulnerable to XSS)" },
                    { "jQuery 2.", "jQuery version 2.x (known vulnerabilities)" },
                    { "angular.js/1.2", "AngularJS 1.2.x (vulnerable to sandbox bypass)" },
                    { "bootstrap/3.", "Bootstrap 3.x (XSS vulnerabilities)" },
                    { "moment.js", "Moment.js (deprecated, has vulnerabilities)" },
                    { "lodash.js", "Lodash (check version for prototype pollution)" }
                };

                foreach (var library in vulnerableLibraries)
                {
                    if (content.Contains(library.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add($"Vulnerable library detected: {library.Value}");
                        results.Add(GenerateVulnerableComponentPoC(url, library.Key, library.Value));
                    }
                }

                // Check headers for server version
                if (response.Headers.Contains("X-Powered-By"))
                {
                    var poweredBy = response.Headers.GetValues("X-Powered-By").FirstOrDefault();
                    results.Add($"Server technology exposed: {poweredBy}");
                    results.Add(GenerateServerExposurePoC(url, poweredBy));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting vulnerable libraries: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> DetectOutdatedFrameworksAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync(url);

                // Check for framework-specific headers or patterns
                if (response.Headers.Contains("X-AspNet-Version"))
                {
                    var version = response.Headers.GetValues("X-AspNet-Version").FirstOrDefault();
                    results.Add($"ASP.NET version exposed: {version}");
                }

                if (response.Headers.Contains("X-AspNetMvc-Version"))
                {
                    var version = response.Headers.GetValues("X-AspNetMvc-Version").FirstOrDefault();
                    results.Add($"ASP.NET MVC version exposed: {version}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting outdated frameworks: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> DetectVulnerableDependenciesAsync(string url)
        {
            var results = new List<string>();

            try
            {
                // Check for common dependency files
                var dependencyFiles = new[]
                {
                    $"{url}/package.json",
                    $"{url}/composer.json",
                    $"{url}/requirements.txt",
                    $"{url}/Gemfile",
                    $"{url}/pom.xml"
                };

                foreach (var file in dependencyFiles)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(file);

                        if (response.IsSuccessStatusCode)
                        {
                            results.Add($"Exposed dependency file: {file}");
                            results.Add(GenerateDependencyExposurePoC(url, file));
                        }
                    }
                    catch
                    {
                        // Continue checking other files
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error detecting vulnerable dependencies: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region A07: Enhanced Authentication Failures Testing

        /// <summary>
        /// Enhanced authentication failure testing
        /// </summary>
        private async Task<List<string>> TestEnhancedAuthenticationFailuresAsync(string url)
        {
            var results = new List<string>();

            OnScanProgress(new OWASPT10ScanProgressEventArgs { Url = url, Status = $"Testing enhanced CORS misconfigurations on {url}...", CurrentCategory = "CORS", Message = $"Testing enhanced CORS misconfigurations on {url}...", ProgressPercentage = 0 });

            // Test for brute force protection
            results.AddRange(await TestBruteForceProtectionAsync(url));

            // Test session management
            results.AddRange(await TestEnhancedSessionManagementAsync(url));

            // Test for authentication bypass
            results.AddRange(await TestAuthenticationBypassAsync(url));

            return results;
        }

        private async Task<List<string>> TestBruteForceProtectionAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var loginUrl = url.Contains("login") ? url : $"{url}/login";
                var attempts = 0;
                var maxAttempts = 10;

                for (int i = 0; i < maxAttempts; i++)
                {
                    var postData = new StringContent(
                        $"{{\"username\":\"admin\",\"password\":\"test{i}\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.PostAsync(loginUrl, postData);
                    attempts++;

                    if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests &&
                        !response.Headers.Contains("X-RateLimit-Remaining"))
                    {
                        await Task.Delay(100);
                    }
                    else
                    {
                        // Rate limiting detected
                        break;
                    }
                }

                if (attempts >= maxAttempts)
                {
                    results.Add("No brute force protection detected");
                    results.Add(GenerateBruteForcePoC(url));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing brute force protection: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> TestEnhancedSessionManagementAsync(string url)
        {
            var results = new List<string>();

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.Headers.Contains("Set-Cookie"))
                {
                    var cookies = response.Headers.GetValues("Set-Cookie");

                    foreach (var cookie in cookies)
                    {
                        if (!cookie.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add("Cookie without HttpOnly flag detected");
                        }

                        if (!cookie.Contains("Secure", StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add("Cookie without Secure flag detected");
                        }

                        if (!cookie.Contains("SameSite", StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add("Cookie without SameSite attribute detected");
                        }
                    }

                    if (results.Any())
                    {
                        results.Add(GenerateSessionManagementPoC(url));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing session management: {ex.Message}");
            }

            return results;
        }

        private async Task<List<string>> TestAuthenticationBypassAsync(string url)
        {
            var results = new List<string>();

            try
            {
                // Test common authentication bypass techniques
                var bypassPayloads = new Dictionary<string, string>
                {
                    { "SQL Injection", "admin' OR '1'='1" },
                    { "NoSQL Injection", "{\"$gt\":\"\"}" },
                    { "Null byte", "admin%00" },
                    { "Empty password", "" }
                };

                foreach (var payload in bypassPayloads)
                {
                    var postData = new StringContent(
                        $"{{\"username\":\"{payload.Value}\",\"password\":\"{payload.Value}\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await _httpClient.PostAsync(url, postData);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        if (content.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                            content.Contains("dashboard", StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add($"Authentication bypass via {payload.Key}");
                            results.Add(GenerateAuthBypassPoC(url, payload.Key, payload.Value));
                        }
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error testing authentication bypass: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Helper Methods

        private string ExtractIdFromUrl(string url)
        {
            var segments = new Uri(url).Segments;
            return segments.Length > 0 ? segments.Last().TrimEnd('/') : "1";
        }

        private bool DetectIDORVulnerability(string content, HttpResponseMessage response,
            string testId, string currentId)
        {
            // Detect if we can access different user's data
            return response.IsSuccessStatusCode &&
                   content.Length > 100 &&
                   testId != currentId;
        }

        #endregion

        #region Additional PoC Generators

        private string GenerateAccessControlBypassPoC(string originalUrl, string bypassUrl)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Access Control Bypass Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Original URL: {originalUrl}");
            sb.AppendLine($"Bypass URL: {bypassUrl}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Implement proper authorization checks");
            return sb.ToString();
        }

        private string GenerateWeakCryptoPoC(string url, string algorithm)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Weak Cryptography Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Weak algorithm detected: {algorithm}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Use strong encryption (AES-256, RSA-2048+)");
            return sb.ToString();
        }

        private string GenerateSensitiveDataExposurePoC(string url, string dataType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Sensitive Data Exposure Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Exposed data type: {dataType}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Encrypt sensitive data, remove from responses");
            return sb.ToString();
        }

        private string GenerateWeakHashingPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Weak Hashing Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Remediation: Use bcrypt, Argon2, or PBKDF2");
            return sb.ToString();
        }

        private string GenerateVulnerableComponentPoC(string url, string component, string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Vulnerable Component Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Component: {component}");
            sb.AppendLine($"Issue: {description}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Update to latest secure version");
            return sb.ToString();
        }

        private string GenerateServerExposurePoC(string url, string serverInfo)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Server Information Exposure Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Exposed: {serverInfo}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Remove version headers");
            return sb.ToString();
        }

        private string GenerateDependencyExposurePoC(string url, string file)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Dependency File Exposure Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Exposed file: {file}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Restrict access to dependency files");
            return sb.ToString();
        }

        private string GenerateBruteForcePoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Brute Force Vulnerability Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Remediation: Implement rate limiting, CAPTCHA, account lockout");
            return sb.ToString();
        }

        private string GenerateSessionManagementPoC(string url)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Session Management Vulnerability Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine("Remediation: Use HttpOnly, Secure, and SameSite flags");
            return sb.ToString();
        }

        private string GenerateAuthBypassPoC(string url, string technique, string payload)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Authentication Bypass Proof of Concept ===");
            sb.AppendLine();
            sb.AppendLine($"Technique: {technique}");
            sb.AppendLine($"Payload: {payload}");
            sb.AppendLine();
            sb.AppendLine("Remediation: Implement proper input validation and authentication");
            return sb.ToString();
        }

        #endregion
    }
}
