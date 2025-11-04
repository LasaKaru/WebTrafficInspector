using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced security analysis and vulnerability detection
    /// </summary>
    public class SecurityAnalyzerService
    {
        private readonly List<SecurityPattern> _vulnerabilityPatterns;
        private readonly List<string> _sensitiveDataPatterns;

        public SecurityAnalyzerService()
        {
            _vulnerabilityPatterns = new List<SecurityPattern>
            {
                new SecurityPattern
                {
                    Type = "Debug Information Exposure",
                    Pattern = @"(stacktrace|exception|error|debug|trace)",
                    Severity = "Medium",
                    Description = "Response contains debug information",
                    Recommendation = "Disable debug mode in production"
                },
                new SecurityPattern
                {
                    Type = "Directory Listing",
                    Pattern = @"Index of /|Directory listing for",
                    Severity = "Medium",
                    Description = "Directory listing enabled",
                    Recommendation = "Disable directory listing"
                },
                new SecurityPattern
                {
                    Type = "Weak Encryption",
                    Pattern = @"(SSL_?v?[23]|TLS_?v?1\.?[01]|RC4|MD5)",
                    Severity = "High",
                    Description = "Weak encryption protocol detected",
                    Recommendation = "Use TLS 1.2 or higher"
                }
            };

            _sensitiveDataPatterns = new List<string>();
        }

        /// <summary>
        /// Perform comprehensive security analysis on a traffic entry
        /// </summary>
        public SecurityReport AnalyzeEntry(TrafficEntry entry)
        {
            var report = new SecurityReport
            {
                EntryId = entry.Id,
                Url = $"{entry.Host}{entry.Path}",
                Timestamp = entry.Timestamp
            };

            // Analyze request
            if (!string.IsNullOrEmpty(entry.RawRequest))
            {
                report.RequestIssues.AddRange(AnalyzeText(entry.RawRequest, "Request"));
                report.SensitiveDataInRequest.AddRange(FindSensitiveData(entry.RawRequest));
                report.InjectionAttempts.AddRange(DetectInjections(entry.RawRequest));
            }

            // Analyze response
            if (!string.IsNullOrEmpty(entry.RawResponse))
            {
                report.ResponseIssues.AddRange(AnalyzeText(entry.RawResponse, "Response"));
                report.SensitiveDataInResponse.AddRange(FindSensitiveData(entry.RawResponse));
                report.SecurityHeaders = AnalyzeSecurityHeaders(entry.RawResponse);
            }

            // Analyze URL
            report.UrlIssues.AddRange(AnalyzeUrl(entry.Path));

            // Populate new properties
            report.HighSeverityCount = report.RequestIssues.Count(i => i.Severity == "High" || i.Severity == "Critical") +
                                     report.ResponseIssues.Count(i => i.Severity == "High" || i.Severity == "Critical") +
                                     report.UrlIssues.Count(i => i.Severity == "High" || i.Severity == "Critical") +
                                     report.InjectionAttempts.Count(i => i.Severity == "High" || i.Severity == "Critical");
            report.MediumSeverityCount = report.RequestIssues.Count(i => i.Severity == "Medium") +
                                        report.ResponseIssues.Count(i => i.Severity == "Medium") +
                                        report.UrlIssues.Count(i => i.Severity == "Medium") +
                                        report.InjectionAttempts.Count(i => i.Severity == "Medium");
            report.LowSeverityCount = report.RequestIssues.Count(i => i.Severity == "Low") +
                                     report.ResponseIssues.Count(i => i.Severity == "Low") +
                                     report.UrlIssues.Count(i => i.Severity == "Low") +
                                     report.InjectionAttempts.Count(i => i.Severity == "Low");

            // Combine all issues
            report.Issues.AddRange(report.RequestIssues);
            report.Issues.AddRange(report.ResponseIssues);
            report.Issues.AddRange(report.UrlIssues);
            report.Issues.AddRange(report.InjectionAttempts);

            // Combine sensitive data
            report.SensitiveDataFound.AddRange(report.SensitiveDataInRequest);
            report.SensitiveDataFound.AddRange(report.SensitiveDataInResponse);

            // Calculate risk score
            report.RiskScore = CalculateRiskScore(report);
            report.RiskLevel = DetermineRiskLevel(report.RiskScore);

            return report;
        }

        /// <summary>
        /// Analyze multiple entries for security issues
        /// </summary>
        public List<SecurityReport> AnalyzeSession(List<TrafficEntry> entries)
        {
            return entries.Select(AnalyzeEntry).ToList();
        }

        /// <summary>
        /// Find common vulnerabilities across session
        /// </summary>
        public SessionSecuritySummary GetSessionSummary(List<SecurityReport> reports)
        {
            return new SessionSecuritySummary
            {
                TotalEntries = reports.Count,
                HighRiskEntries = reports.Count(r => r.RiskLevel == "High"),
                MediumRiskEntries = reports.Count(r => r.RiskLevel == "Medium"),
                LowRiskEntries = reports.Count(r => r.RiskLevel == "Low"),
                TotalIssuesFound = reports.Sum(r => r.TotalIssues),
                CommonVulnerabilities = GetTopVulnerabilities(reports),
                SensitiveDataExposures = reports.Sum(r => r.SensitiveDataInRequest.Count + r.SensitiveDataInResponse.Count),
                InsecureEndpoints = reports.Where(r => r.UrlIssues.Any(i => i.Severity == "High")).Select(r => r.Url).Distinct().ToList()
            };
        }

        private List<SecurityIssue> AnalyzeText(string text, string location)
        {
            var issues = new List<SecurityIssue>();

            foreach (var pattern in _vulnerabilityPatterns)
            {
                if (Regex.IsMatch(text, pattern.Pattern, RegexOptions.IgnoreCase))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = pattern.Type,
                        Severity = pattern.Severity,
                        Description = pattern.Description,
                        Location = location,
                        Recommendation = pattern.Recommendation
                    });
                }
            }

            return issues;
        }

        private List<SensitiveData> FindSensitiveData(string text)
        {
            var findings = new List<SensitiveData>();

            // Credit Card Numbers
            var ccPattern = @"\b(?:\d{4}[-\s]?){3}\d{4}\b";
            foreach (Match match in Regex.Matches(text, ccPattern))
            {
                findings.Add(new SensitiveData
                {
                    Type = "Credit Card",
                    Value = MaskSensitiveValue(match.Value),
                    Position = match.Index,
                    Severity = "Critical"
                });
            }

            // Email Addresses
            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            foreach (Match match in Regex.Matches(text, emailPattern))
            {
                findings.Add(new SensitiveData
                {
                    Type = "Email",
                    Value = MaskSensitiveValue(match.Value),
                    Position = match.Index,
                    Severity = "Medium"
                });
            }

            // Social Security Numbers
            var ssnPattern = @"\b\d{3}-\d{2}-\d{4}\b";
            foreach (Match match in Regex.Matches(text, ssnPattern))
            {
                findings.Add(new SensitiveData
                {
                    Type = "SSN",
                    Value = MaskSensitiveValue(match.Value),
                    Position = match.Index,
                    Severity = "Critical"
                });
            }

            // API Keys (basic patterns)
            var apiKeyPatterns = new[]
            {
                @"api[_-]?key['""\s:=]+([A-Za-z0-9_\-]{20,})",
                @"apikey['""\s:=]+([A-Za-z0-9_\-]{20,})",
                @"access[_-]?token['""\s:=]+([A-Za-z0-9_\-]{20,})",
                @"secret[_-]?key['""\s:=]+([A-Za-z0-9_\-]{20,})"
            };

            foreach (var pattern in apiKeyPatterns)
            {
                foreach (Match match in Regex.Matches(text, pattern, RegexOptions.IgnoreCase))
                {
                    if (match.Groups.Count > 1)
                    {
                        findings.Add(new SensitiveData
                        {
                            Type = "API Key/Token",
                            Value = MaskSensitiveValue(match.Groups[1].Value),
                            Position = match.Index,
                            Severity = "High"
                        });
                    }
                }
            }

            // Passwords in plain text
            var passwordPattern = @"password['""\s:=]+([^'""&\s]{6,})";
            foreach (Match match in Regex.Matches(text, passwordPattern, RegexOptions.IgnoreCase))
            {
                if (match.Groups.Count > 1)
                {
                    findings.Add(new SensitiveData
                    {
                        Type = "Password",
                        Value = "******",
                        Position = match.Index,
                        Severity = "Critical"
                    });
                }
            }

            // IP Addresses (internal)
            var ipPattern = @"\b(?:10\.|172\.(?:1[6-9]|2[0-9]|3[0-1])\.|192\.168\.)\d{1,3}\.\d{1,3}\b";
            foreach (Match match in Regex.Matches(text, ipPattern))
            {
                findings.Add(new SensitiveData
                {
                    Type = "Internal IP",
                    Value = match.Value,
                    Position = match.Index,
                    Severity = "Low"
                });
            }

            return findings;
        }

        private List<SecurityIssue> DetectInjections(string text)
        {
            var issues = new List<SecurityIssue>();

            // SQL Injection patterns
            var sqlPatterns = new[]
            {
                @"(\bUNION\b.*\bSELECT\b)",
                @"(\bOR\b\s+['""]?\d+['""]?\s*=\s*['""]?\d+)",
                @"(\bAND\b\s+['""]?\d+['""]?\s*=\s*['""]?\d+)",
                @"(';.*--)",
                @"(\bDROP\b.*\bTABLE\b)",
                @"(\bEXEC\b.*\bXP_)",
                @"(1=1|2=2)"
            };

            foreach (var pattern in sqlPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "SQL Injection Attempt",
                        Severity = "High",
                        Description = "Potential SQL injection pattern detected in request",
                        Location = "Request Body/Parameters",
                        Recommendation = "Validate input, use parameterized queries"
                    });
                    break; // Only report once
                }
            }

            // XSS patterns
            var xssPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"javascript:",
                @"onerror\s*=",
                @"onload\s*=",
                @"<iframe",
                @"eval\s*\("
            };

            foreach (var pattern in xssPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "XSS Attempt",
                        Severity = "High",
                        Description = "Potential Cross-Site Scripting pattern detected",
                        Location = "Request Parameters",
                        Recommendation = "Sanitize input, implement Content Security Policy"
                    });
                    break;
                }
            }

            // Command Injection
            var cmdPatterns = new[]
            {
                @"[;&|]\s*(?:cat|ls|pwd|wget|curl|nc|bash|sh)\s",
                @"\$\([^)]+\)",
                @"`[^`]+`"
            };

            foreach (var pattern in cmdPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "Command Injection Attempt",
                        Severity = "Critical",
                        Description = "Potential command injection pattern detected",
                        Location = "Request Parameters",
                        Recommendation = "Validate and sanitize all user input, avoid system calls"
                    });
                    break;
                }
            }

            // Path Traversal
            if (Regex.IsMatch(text, @"\.\./|\.\.\\"))
            {
                issues.Add(new SecurityIssue
                {
                    Type = "Path Traversal Attempt",
                    Severity = "High",
                    Description = "Path traversal pattern detected (../ or ..\\)",
                    Location = "URL/Parameters",
                    Recommendation = "Validate file paths, use whitelist approach"
                });
            }

            return issues;
        }

        private List<SecurityIssue> AnalyzeUrl(string url)
        {
            var issues = new List<SecurityIssue>();

            // Check for sensitive data in URL
            if (Regex.IsMatch(url, @"(password|token|key|secret|api[-_]?key)", RegexOptions.IgnoreCase))
            {
                issues.Add(new SecurityIssue
                {
                    Type = "Sensitive Data in URL",
                    Severity = "High",
                    Description = "Sensitive parameter name found in URL",
                    Location = "URL",
                    Recommendation = "Use POST requests with encrypted body for sensitive data"
                });
            }

            // Check for unencrypted admin/login pages
            if (Regex.IsMatch(url, @"/(admin|login|auth|panel)", RegexOptions.IgnoreCase))
            {
                issues.Add(new SecurityIssue
                {
                    Type = "Sensitive Endpoint",
                    Severity = "Medium",
                    Description = "Sensitive endpoint detected - ensure HTTPS is used",
                    Location = "URL",
                    Recommendation = "Always use HTTPS for authentication endpoints"
                });
            }

            return issues;
        }

        private SecurityHeadersAnalysis AnalyzeSecurityHeaders(string response)
        {
            var analysis = new SecurityHeadersAnalysis();
            var responseLines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in responseLines)
            {
                var headerName = line.Split(':')[0].Trim().ToLower();
                var headerValue = line.Contains(':') ? line.Substring(line.IndexOf(':') + 1).Trim() : "";

                switch (headerName)
                {
                    case "strict-transport-security":
                        analysis.HasHSTS = true;
                        analysis.HSTSValue = headerValue;
                        break;
                    case "content-security-policy":
                        analysis.HasCSP = true;
                        analysis.CSPValue = headerValue;
                        break;
                    case "x-frame-options":
                        analysis.HasXFrameOptions = true;
                        analysis.XFrameOptionsValue = headerValue;
                        break;
                    case "x-content-type-options":
                        analysis.HasXContentTypeOptions = true;
                        break;
                    case "x-xss-protection":
                        analysis.HasXSSProtection = true;
                        break;
                    case "referrer-policy":
                        analysis.HasReferrerPolicy = true;
                        break;
                }
            }

            // Generate recommendations
            if (!analysis.HasHSTS)
                analysis.MissingHeaders.Add("Strict-Transport-Security - Prevents protocol downgrade attacks");
            if (!analysis.HasCSP)
                analysis.MissingHeaders.Add("Content-Security-Policy - Mitigates XSS attacks");
            if (!analysis.HasXFrameOptions)
                analysis.MissingHeaders.Add("X-Frame-Options - Prevents clickjacking");
            if (!analysis.HasXContentTypeOptions)
                analysis.MissingHeaders.Add("X-Content-Type-Options - Prevents MIME sniffing");

            analysis.SecurityScore = ((analysis.HasHSTS ? 25 : 0) +
                                     (analysis.HasCSP ? 30 : 0) +
                                     (analysis.HasXFrameOptions ? 20 : 0) +
                                     (analysis.HasXContentTypeOptions ? 15 : 0) +
                                     (analysis.HasXSSProtection ? 10 : 0));

            return analysis;
        }

        private int CalculateRiskScore(SecurityReport report)
        {
            int score = 0;

            // Issues contribute to risk
            score += report.RequestIssues.Count(i => i.Severity == "Critical") * 20;
            score += report.RequestIssues.Count(i => i.Severity == "High") * 10;
            score += report.RequestIssues.Count(i => i.Severity == "Medium") * 5;
            score += report.RequestIssues.Count(i => i.Severity == "Low") * 2;

            score += report.ResponseIssues.Count * 5;
            score += report.InjectionAttempts.Count * 15;
            score += report.SensitiveDataInRequest.Count * 10;
            score += report.SensitiveDataInResponse.Count * 8;

            // Security headers reduce risk
            if (report.SecurityHeaders != null)
            {
                score -= (100 - report.SecurityHeaders.SecurityScore) / 10;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private string DetermineRiskLevel(int score)
        {
            if (score >= 70) return "High";
            if (score >= 40) return "Medium";
            return "Low";
        }

        private string MaskSensitiveValue(string value)
        {
            if (value.Length <= 8)
                return "****";
            return value.Substring(0, 4) + "****" + value.Substring(value.Length - 4);
        }

        private List<VulnerabilityCount> GetTopVulnerabilities(List<SecurityReport> reports)
        {
            var allIssues = reports.SelectMany(r => r.RequestIssues.Concat(r.ResponseIssues));

            return allIssues
                .GroupBy(i => i.Type)
                .Select(g => new VulnerabilityCount
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Severity = g.First().Severity
                })
                .OrderByDescending(v => v.Count)
                .Take(10)
                .ToList();
        }
    }

    public class SecurityReport
    {
        public int EntryId { get; set; }
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public List<SecurityIssue> RequestIssues { get; set; } = new List<SecurityIssue>();
        public List<SecurityIssue> ResponseIssues { get; set; } = new List<SecurityIssue>();
        public List<SecurityIssue> UrlIssues { get; set; } = new List<SecurityIssue>();
        public List<SensitiveData> SensitiveDataInRequest { get; set; } = new List<SensitiveData>();
        public List<SensitiveData> SensitiveDataInResponse { get; set; } = new List<SensitiveData>();
        public List<SecurityIssue> InjectionAttempts { get; set; } = new List<SecurityIssue>();
        public SecurityHeadersAnalysis SecurityHeaders { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public int HighSeverityCount { get; set; }
        public int MediumSeverityCount { get; set; }
        public int LowSeverityCount { get; set; }
        public List<SecurityIssue> Issues { get; set; } = new List<SecurityIssue>();
        public List<SensitiveData> SensitiveDataFound { get; set; } = new List<SensitiveData>();

        public int TotalIssues => RequestIssues.Count + ResponseIssues.Count + UrlIssues.Count +
                                 SensitiveDataInRequest.Count + SensitiveDataInResponse.Count +
                                 InjectionAttempts.Count;
    }

    public class SecurityIssue
    {
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Recommendation { get; set; }
    }

    public class SensitiveData
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
        public string Severity { get; set; }
    }

    public class SecurityHeadersAnalysis
    {
        public bool HasHSTS { get; set; }
        public string HSTSValue { get; set; }
        public bool HasCSP { get; set; }
        public string CSPValue { get; set; }
        public bool HasXFrameOptions { get; set; }
        public string XFrameOptionsValue { get; set; }
        public bool HasXContentTypeOptions { get; set; }
        public bool HasXSSProtection { get; set; }
        public bool HasReferrerPolicy { get; set; }
        public List<string> MissingHeaders { get; set; } = new List<string>();
        public int SecurityScore { get; set; }
    }

    public class SecurityPattern
    {
        public string Type { get; set; }
        public string Pattern { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }

    public class SessionSecuritySummary
    {
        public int TotalEntries { get; set; }
        public int HighRiskEntries { get; set; }
        public int MediumRiskEntries { get; set; }
        public int LowRiskEntries { get; set; }
        public int TotalIssuesFound { get; set; }
        public List<VulnerabilityCount> CommonVulnerabilities { get; set; }
        public int SensitiveDataExposures { get; set; }
        public List<string> InsecureEndpoints { get; set; }
    }

    public class VulnerabilityCount
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public string Severity { get; set; }
    }
}
