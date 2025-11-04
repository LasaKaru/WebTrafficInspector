using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Xml.Linq;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class ResponseValidatorService
    {
        public ValidationResult ValidateResponse(TrafficEntry entry, ValidationRule rule)
        {
            var result = new ValidationResult
            {
                RuleName = rule.Name,
                EntryId = entry.Id,
                Url = entry.Url,
                Timestamp = DateTime.Now
            };

            var checks = new List<ValidationCheck>();

            // Status code validation
            if (rule.ExpectedStatusCode.HasValue)
            {
                checks.Add(new ValidationCheck
                {
                    Name = "Status Code",
                    Expected = rule.ExpectedStatusCode.Value.ToString(),
                    Actual = entry.StatusCode.ToString(),
                    Passed = entry.StatusCode == rule.ExpectedStatusCode.Value,
                    Severity = ValidationSeverity.High
                });
            }

            // Status code range validation
            if (rule.ExpectedStatusCodeRange != null && rule.ExpectedStatusCodeRange.Length == 2)
            {
                var inRange = entry.StatusCode >= rule.ExpectedStatusCodeRange[0] &&
                             entry.StatusCode <= rule.ExpectedStatusCodeRange[1];
                checks.Add(new ValidationCheck
                {
                    Name = "Status Code Range",
                    Expected = $"{rule.ExpectedStatusCodeRange[0]}-{rule.ExpectedStatusCodeRange[1]}",
                    Actual = entry.StatusCode.ToString(),
                    Passed = inRange,
                    Severity = ValidationSeverity.High
                });
            }

            // Content type validation
            if (!string.IsNullOrEmpty(rule.ExpectedContentType))
            {
                var contentTypeMatch = entry.ContentType?.Contains(rule.ExpectedContentType, StringComparison.OrdinalIgnoreCase) == true;
                checks.Add(new ValidationCheck
                {
                    Name = "Content Type",
                    Expected = rule.ExpectedContentType,
                    Actual = entry.ContentType ?? "None",
                    Passed = contentTypeMatch,
                    Severity = ValidationSeverity.Medium
                });
            }

            // Response time validation
            if (rule.MaxResponseTime.HasValue)
            {
                checks.Add(new ValidationCheck
                {
                    Name = "Response Time",
                    Expected = $"<= {rule.MaxResponseTime.Value}ms",
                    Actual = $"{entry.Duration}ms",
                    Passed = entry.Duration <= rule.MaxResponseTime.Value,
                    Severity = ValidationSeverity.Medium
                });
            }

            // Response size validation
            if (rule.MinResponseSize.HasValue || rule.MaxResponseSize.HasValue)
            {
                var sizeOk = true;
                var expected = "";

                if (rule.MinResponseSize.HasValue)
                {
                    sizeOk = sizeOk && entry.Size >= rule.MinResponseSize.Value;
                    expected = $">= {rule.MinResponseSize.Value} bytes";
                }
                if (rule.MaxResponseSize.HasValue)
                {
                    sizeOk = sizeOk && entry.Size <= rule.MaxResponseSize.Value;
                    expected += (expected.Length > 0 ? " and " : "") + $"<= {rule.MaxResponseSize.Value} bytes";
                }

                checks.Add(new ValidationCheck
                {
                    Name = "Response Size",
                    Expected = expected,
                    Actual = $"{entry.Size} bytes",
                    Passed = sizeOk,
                    Severity = ValidationSeverity.Low
                });
            }

            // Header validation
            if (rule.RequiredHeaders != null && rule.RequiredHeaders.Any())
            {
                foreach (var requiredHeader in rule.RequiredHeaders)
                {
                    var headerPresent = entry.RawResponse?.Contains($"{requiredHeader}:", StringComparison.OrdinalIgnoreCase) == true;
                    checks.Add(new ValidationCheck
                    {
                        Name = $"Required Header: {requiredHeader}",
                        Expected = "Present",
                        Actual = headerPresent ? "Present" : "Missing",
                        Passed = headerPresent,
                        Severity = ValidationSeverity.Medium
                    });
                }
            }

            // Forbidden headers validation
            if (rule.ForbiddenHeaders != null && rule.ForbiddenHeaders.Any())
            {
                foreach (var forbiddenHeader in rule.ForbiddenHeaders)
                {
                    var headerPresent = entry.RawResponse?.Contains($"{forbiddenHeader}:", StringComparison.OrdinalIgnoreCase) == true;
                    checks.Add(new ValidationCheck
                    {
                        Name = $"Forbidden Header: {forbiddenHeader}",
                        Expected = "Absent",
                        Actual = headerPresent ? "Present" : "Absent",
                        Passed = !headerPresent,
                        Severity = ValidationSeverity.High
                    });
                }
            }

            // Body pattern validation
            if (rule.BodyMustContain != null && rule.BodyMustContain.Any())
            {
                foreach (var pattern in rule.BodyMustContain)
                {
                    var found = false;
                    if (rule.UseRegex)
                    {
                        found = !string.IsNullOrEmpty(entry.RawResponse) &&
                               Regex.IsMatch(entry.RawResponse, pattern, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        found = entry.RawResponse?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true;
                    }

                    checks.Add(new ValidationCheck
                    {
                        Name = $"Body Contains: {pattern}",
                        Expected = "Found",
                        Actual = found ? "Found" : "Not Found",
                        Passed = found,
                        Severity = ValidationSeverity.Medium
                    });
                }
            }

            // Body must not contain validation
            if (rule.BodyMustNotContain != null && rule.BodyMustNotContain.Any())
            {
                foreach (var pattern in rule.BodyMustNotContain)
                {
                    var found = false;
                    if (rule.UseRegex)
                    {
                        found = !string.IsNullOrEmpty(entry.RawResponse) &&
                               Regex.IsMatch(entry.RawResponse, pattern, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        found = entry.RawResponse?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true;
                    }

                    checks.Add(new ValidationCheck
                    {
                        Name = $"Body Must Not Contain: {pattern}",
                        Expected = "Not Found",
                        Actual = found ? "Found" : "Not Found",
                        Passed = !found,
                        Severity = ValidationSeverity.High
                    });
                }
            }

            // JSON validation
            if (rule.ValidateJson)
            {
                var jsonValid = IsValidJson(entry.RawResponse);
                checks.Add(new ValidationCheck
                {
                    Name = "Valid JSON",
                    Expected = "Valid",
                    Actual = jsonValid ? "Valid" : "Invalid",
                    Passed = jsonValid,
                    Severity = ValidationSeverity.High
                });
            }

            // JSON schema validation (basic)
            if (!string.IsNullOrEmpty(rule.JsonSchemaPath) && IsValidJson(entry.RawResponse))
            {
                var schemaValid = ValidateJsonStructure(entry.RawResponse, rule.JsonSchemaPath);
                checks.Add(new ValidationCheck
                {
                    Name = "JSON Schema",
                    Expected = "Matches schema",
                    Actual = schemaValid ? "Matches" : "Does not match",
                    Passed = schemaValid,
                    Severity = ValidationSeverity.High
                });
            }

            // XML validation
            if (rule.ValidateXml)
            {
                var xmlValid = IsValidXml(entry.RawResponse);
                checks.Add(new ValidationCheck
                {
                    Name = "Valid XML",
                    Expected = "Valid",
                    Actual = xmlValid ? "Valid" : "Invalid",
                    Passed = xmlValid,
                    Severity = ValidationSeverity.High
                });
            }

            // Security headers validation
            if (rule.CheckSecurityHeaders)
            {
                var securityChecks = CheckSecurityHeaders(entry.RawResponse);
                checks.AddRange(securityChecks);
            }

            result.Checks = checks;
            result.TotalChecks = checks.Count;
            result.PassedChecks = checks.Count(c => c.Passed);
            result.FailedChecks = checks.Count(c => !c.Passed);
            result.OverallPassed = result.FailedChecks == 0;
            result.SuccessRate = result.TotalChecks > 0 ? (double)result.PassedChecks / result.TotalChecks * 100 : 0;

            return result;
        }

        public BatchValidationResult ValidateBatch(List<TrafficEntry> entries, ValidationRule rule)
        {
            var results = entries.Select(entry => ValidateResponse(entry, rule)).ToList();

            return new BatchValidationResult
            {
                RuleName = rule.Name,
                TotalEntries = entries.Count,
                Results = results,
                TotalPassed = results.Count(r => r.OverallPassed),
                TotalFailed = results.Count(r => !r.OverallPassed),
                OverallSuccessRate = entries.Count > 0 ? (double)results.Count(r => r.OverallPassed) / entries.Count * 100 : 0,
                Timestamp = DateTime.Now
            };
        }

        private bool IsValidJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            try
            {
                JsonDocument.Parse(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateJsonStructure(string json, string schemaPath)
        {
            // Basic JSON structure validation (checking for required keys)
            try
            {
                var doc = JsonDocument.Parse(json);
                var paths = schemaPath.Split(',');

                foreach (var path in paths)
                {
                    var keys = path.Trim().Split('.');
                    JsonElement current = doc.RootElement;

                    foreach (var key in keys)
                    {
                        if (current.ValueKind == JsonValueKind.Object)
                        {
                            if (!current.TryGetProperty(key, out current))
                                return false;
                        }
                        else if (current.ValueKind == JsonValueKind.Array)
                        {
                            if (!int.TryParse(key, out int index) || index >= current.GetArrayLength())
                                return false;
                            current = current[index];
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidXml(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            try
            {
                XDocument.Parse(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<ValidationCheck> CheckSecurityHeaders(string response)
        {
            var checks = new List<ValidationCheck>();

            var securityHeaders = new Dictionary<string, string>
            {
                ["Strict-Transport-Security"] = "HSTS header for HTTPS enforcement",
                ["Content-Security-Policy"] = "CSP header for XSS protection",
                ["X-Frame-Options"] = "Clickjacking protection",
                ["X-Content-Type-Options"] = "MIME type sniffing protection",
                ["Referrer-Policy"] = "Referrer information control",
                ["Permissions-Policy"] = "Feature policy control"
            };

            foreach (var header in securityHeaders)
            {
                var present = response?.Contains($"{header.Key}:", StringComparison.OrdinalIgnoreCase) == true;
                checks.Add(new ValidationCheck
                {
                    Name = $"Security Header: {header.Key}",
                    Expected = "Present",
                    Actual = present ? "Present" : "Missing",
                    Passed = present,
                    Severity = ValidationSeverity.Medium,
                    Description = header.Value
                });
            }

            return checks;
        }

        public List<ValidationRule> GetBuiltInRules()
        {
            return new List<ValidationRule>
            {
                new ValidationRule
                {
                    Name = "API Success Response",
                    Description = "Validates successful API response",
                    ExpectedStatusCodeRange = new[] { 200, 299 },
                    ExpectedContentType = "application/json",
                    ValidateJson = true,
                    MaxResponseTime = 2000
                },
                new ValidationRule
                {
                    Name = "Fast Response",
                    Description = "Ensures response time is under 500ms",
                    MaxResponseTime = 500
                },
                new ValidationRule
                {
                    Name = "Secure Response",
                    Description = "Checks for security headers",
                    CheckSecurityHeaders = true,
                    RequiredHeaders = new List<string> { "Strict-Transport-Security", "X-Frame-Options" }
                },
                new ValidationRule
                {
                    Name = "No Error Messages",
                    Description = "Ensures no error keywords in response",
                    BodyMustNotContain = new List<string> { "error", "exception", "stack trace", "failed" },
                    UseRegex = false
                },
                new ValidationRule
                {
                    Name = "Valid HTML",
                    Description = "Validates HTML response",
                    ExpectedContentType = "text/html",
                    BodyMustContain = new List<string> { "<html", "</html>" }
                },
                new ValidationRule
                {
                    Name = "JSON API",
                    Description = "Validates JSON API response structure",
                    ExpectedContentType = "application/json",
                    ValidateJson = true,
                    JsonSchemaPath = "status,data",
                    MaxResponseTime = 3000
                }
            };
        }
    }

    public class ValidationRule
    {
        public string Name { get; set; }
        public string Description { get; set; }

        // Status validations
        public int? ExpectedStatusCode { get; set; }
        public int[] ExpectedStatusCodeRange { get; set; }

        // Content validations
        public string ExpectedContentType { get; set; }
        public bool ValidateJson { get; set; }
        public bool ValidateXml { get; set; }
        public string JsonSchemaPath { get; set; }

        // Performance validations
        public int? MaxResponseTime { get; set; }
        public long? MinResponseSize { get; set; }
        public long? MaxResponseSize { get; set; }

        // Header validations
        public List<string> RequiredHeaders { get; set; }
        public List<string> ForbiddenHeaders { get; set; }
        public bool CheckSecurityHeaders { get; set; }

        // Body validations
        public List<string> BodyMustContain { get; set; }
        public List<string> BodyMustNotContain { get; set; }
        public bool UseRegex { get; set; }
    }

    public class ValidationResult
    {
        public string RuleName { get; set; }
        public int EntryId { get; set; }
        public string Url { get; set; }
        public List<ValidationCheck> Checks { get; set; }
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public bool OverallPassed { get; set; }
        public double SuccessRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ValidationCheck
    {
        public string Name { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public bool Passed { get; set; }
        public ValidationSeverity Severity { get; set; }
        public string Description { get; set; }
    }

    public class BatchValidationResult
    {
        public string RuleName { get; set; }
        public int TotalEntries { get; set; }
        public List<ValidationResult> Results { get; set; }
        public int TotalPassed { get; set; }
        public int TotalFailed { get; set; }
        public double OverallSuccessRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ValidationSeverity
    {
        Low,
        Medium,
        High
    }
}
