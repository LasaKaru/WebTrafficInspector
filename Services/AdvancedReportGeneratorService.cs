using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced report generation service with HTML/PDF output capabilities
    /// </summary>
    public class AdvancedReportGeneratorService
    {
        public AdvancedReportGeneratorService()
        {
        }

        /// <summary>
        /// Generate comprehensive HTML security report
        /// </summary>
        public string GenerateSecurityReport(List<TrafficEntry> entries, ReportOptions options = null)
        {
            options ??= new ReportOptions();

            var sb = new StringBuilder();

            // HTML Header
            AppendHTMLHeader(sb, "Comprehensive Security Report");

            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");

            // Title Section
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>Web Traffic Inspector - Security Analysis Report</h1>");
            sb.AppendLine($"<p class='subtitle'>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine("</div>");

            // Executive Summary
            AppendExecutiveSummary(sb, entries);

            // Traffic Overview
            AppendTrafficOverview(sb, entries);

            // Security Findings
            AppendSecurityFindings(sb, entries);

            // Vulnerability Details
            if (options.IncludeVulnerabilityDetails)
            {
                AppendVulnerabilityDetails(sb, entries);
            }

            // Traffic Analysis
            if (options.IncludeTrafficAnalysis)
            {
                AppendTrafficAnalysis(sb, entries);
            }

            // Detailed Traffic Log
            if (options.IncludeDetailedLog)
            {
                AppendDetailedTrafficLog(sb, entries);
            }

            // Recommendations
            AppendRecommendations(sb, entries);

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private void AppendHTMLHeader(StringBuilder sb, string title)
        {
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine($"<title>{title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    background: #f5f5f5;
                }
                .container {
                    max-width: 1200px;
                    margin: 20px auto;
                    background: white;
                    padding: 40px;
                    box-shadow: 0 0 20px rgba(0,0,0,0.1);
                }
                .header {
                    text-align: center;
                    padding-bottom: 30px;
                    border-bottom: 3px solid #2c3e50;
                    margin-bottom: 30px;
                }
                h1 {
                    color: #2c3e50;
                    font-size: 2.5em;
                    margin-bottom: 10px;
                }
                h2 {
                    color: #34495e;
                    font-size: 1.8em;
                    margin: 30px 0 15px 0;
                    padding-bottom: 10px;
                    border-bottom: 2px solid #ecf0f1;
                }
                h3 {
                    color: #555;
                    font-size: 1.3em;
                    margin: 20px 0 10px 0;
                }
                .subtitle {
                    color: #7f8c8d;
                    font-size: 1.1em;
                }
                .summary-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                    gap: 20px;
                    margin: 20px 0;
                }
                .summary-card {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 25px;
                    border-radius: 10px;
                    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
                }
                .summary-card.success {
                    background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
                }
                .summary-card.warning {
                    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
                }
                .summary-card.danger {
                    background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
                }
                .summary-card h3 {
                    color: white;
                    font-size: 1.1em;
                    margin-bottom: 10px;
                }
                .summary-card .value {
                    font-size: 2.5em;
                    font-weight: bold;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 20px 0;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }
                th {
                    background: #34495e;
                    color: white;
                    padding: 12px;
                    text-align: left;
                    font-weight: 600;
                }
                td {
                    padding: 10px 12px;
                    border-bottom: 1px solid #ecf0f1;
                }
                tr:hover {
                    background: #f8f9fa;
                }
                .severity-critical {
                    background: #e74c3c;
                    color: white;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: bold;
                }
                .severity-high {
                    background: #e67e22;
                    color: white;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: bold;
                }
                .severity-medium {
                    background: #f39c12;
                    color: white;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: bold;
                }
                .severity-low {
                    background: #3498db;
                    color: white;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: bold;
                }
                .code-block {
                    background: #2c3e50;
                    color: #ecf0f1;
                    padding: 15px;
                    border-radius: 5px;
                    font-family: 'Courier New', monospace;
                    font-size: 0.9em;
                    overflow-x: auto;
                    margin: 10px 0;
                }
                .alert {
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 5px;
                    border-left: 4px solid;
                }
                .alert-critical {
                    background: #fde8e8;
                    border-color: #e74c3c;
                    color: #721c24;
                }
                .alert-warning {
                    background: #fff3cd;
                    border-color: #f39c12;
                    color: #856404;
                }
                .alert-info {
                    background: #d1ecf1;
                    border-color: #3498db;
                    color: #0c5460;
                }
                .chart-bar {
                    background: #ecf0f1;
                    height: 30px;
                    border-radius: 5px;
                    overflow: hidden;
                    margin: 5px 0;
                }
                .chart-bar-fill {
                    background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
                    height: 100%;
                    display: flex;
                    align-items: center;
                    padding-left: 10px;
                    color: white;
                    font-weight: bold;
                }
                @media print {
                    body { background: white; }
                    .container { box-shadow: none; }
                }
            ");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
        }

        private void AppendExecutiveSummary(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Executive Summary</h2>");
            sb.AppendLine("<div class='summary-grid'>");

            // Total Requests
            sb.AppendLine("<div class='summary-card'>");
            sb.AppendLine("<h3>Total Requests</h3>");
            sb.AppendLine($"<div class='value'>{entries.Count}</div>");
            sb.AppendLine("</div>");

            // Unique Hosts
            var uniqueHosts = entries.Select(e => e.Host).Distinct().Count();
            sb.AppendLine("<div class='summary-card success'>");
            sb.AppendLine("<h3>Unique Hosts</h3>");
            sb.AppendLine($"<div class='value'>{uniqueHosts}</div>");
            sb.AppendLine("</div>");

            // HTTP Errors
            var errors = entries.Count(e => e.Status >= 400);
            sb.AppendLine("<div class='summary-card warning'>");
            sb.AppendLine("<h3>HTTP Errors</h3>");
            sb.AppendLine($"<div class='value'>{errors}</div>");
            sb.AppendLine("</div>");

            // Total Data Transferred
            var totalBytes = entries.Sum(e => e.Length);
            sb.AppendLine("<div class='summary-card danger'>");
            sb.AppendLine("<h3>Data Transferred</h3>");
            sb.AppendLine($"<div class='value'>{FormatBytes(totalBytes)}</div>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>");
        }

        private void AppendTrafficOverview(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Traffic Overview</h2>");

            // HTTP Methods Distribution
            sb.AppendLine("<h3>HTTP Methods</h3>");
            var methods = entries.GroupBy(e => e.Method).OrderByDescending(g => g.Count());
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Method</th><th>Count</th><th>Percentage</th></tr>");
            foreach (var method in methods)
            {
                var percentage = (method.Count() * 100.0 / entries.Count);
                sb.AppendLine($"<tr><td><strong>{method.Key}</strong></td><td>{method.Count()}</td><td>{percentage:F1}%</td></tr>");
            }
            sb.AppendLine("</table>");

            // Status Codes Distribution
            sb.AppendLine("<h3>Status Codes</h3>");
            var statuses = entries.GroupBy(e => e.Status).OrderByDescending(g => g.Count());
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Status Code</th><th>Description</th><th>Count</th></tr>");
            foreach (var status in statuses)
            {
                var description = GetStatusCodeDescription(status.Key);
                sb.AppendLine($"<tr><td><strong>{status.Key}</strong></td><td>{description}</td><td>{status.Count()}</td></tr>");
            }
            sb.AppendLine("</table>");

            // Top Hosts
            sb.AppendLine("<h3>Top 10 Hosts</h3>");
            var topHosts = entries.GroupBy(e => e.Host).OrderByDescending(g => g.Count()).Take(10);
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Host</th><th>Requests</th><th>Distribution</th></tr>");
            foreach (var host in topHosts)
            {
                var percentage = (host.Count() * 100.0 / entries.Count);
                sb.AppendLine($"<tr><td>{host.Key}</td><td>{host.Count()}</td><td>");
                sb.AppendLine($"<div class='chart-bar'><div class='chart-bar-fill' style='width: {percentage}%'>{percentage:F1}%</div></div>");
                sb.AppendLine("</td></tr>");
            }
            sb.AppendLine("</table>");
        }

        private void AppendSecurityFindings(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Security Findings</h2>");

            var findings = new List<SecurityFinding>();

            // Check for unencrypted traffic
            var httpCount = entries.Count(e => e.Url != null && e.Url.StartsWith("http://"));
            if (httpCount > 0)
            {
                findings.Add(new SecurityFinding
                {
                    Severity = "High",
                    Title = "Unencrypted HTTP Traffic Detected",
                    Description = $"{httpCount} requests were sent over unencrypted HTTP connections",
                    Recommendation = "Use HTTPS for all traffic to ensure confidentiality and integrity"
                });
            }

            // Check for sensitive data in URLs
            var sensitivePatterns = new[] { "password", "pwd", "secret", "token", "api_key", "apikey" };
            foreach (var entry in entries)
            {
                if (entry.Path != null && sensitivePatterns.Any(p => entry.Path.ToLower().Contains(p)))
                {
                    findings.Add(new SecurityFinding
                    {
                        Severity = "Critical",
                        Title = "Sensitive Data in URL",
                        Description = $"URL contains sensitive parameter: {entry.Url}",
                        Recommendation = "Never include sensitive data in URLs; use POST body or secure headers"
                    });
                }
            }

            // Check for missing security headers
            var entriesWithoutHSTS = entries.Count(e => !(ExtractHeaders(e.RawResponse)?.ToLower().Contains("strict-transport-security") ?? false));
            if (entriesWithoutHSTS > entries.Count / 2)
            {
                findings.Add(new SecurityFinding
                {
                    Severity = "Medium",
                    Title = "Missing HSTS Header",
                    Description = $"{entriesWithoutHSTS} responses lack HTTP Strict Transport Security header",
                    Recommendation = "Implement HSTS to prevent protocol downgrade attacks"
                });
            }

            // Check for SQL injection patterns
            var sqlPatterns = new[] { "' OR '1'='1", "UNION SELECT", "'; DROP TABLE", "1=1--" };
            foreach (var entry in entries)
            {
                if (entry.Path != null && sqlPatterns.Any(p => entry.Path.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    findings.Add(new SecurityFinding
                    {
                        Severity = "Critical",
                        Title = "Potential SQL Injection Attempt",
                        Description = $"SQL injection pattern detected in: {entry.Url}",
                        Recommendation = "Verify proper input validation and parameterized queries"
                    });
                }
            }

            // Display findings
            if (findings.Any())
            {
                var critical = findings.Where(f => f.Severity == "Critical").ToList();
                var high = findings.Where(f => f.Severity == "High").ToList();
                var medium = findings.Where(f => f.Severity == "Medium").ToList();

                if (critical.Any())
                {
                    sb.AppendLine("<h3>Critical Findings</h3>");
                    foreach (var finding in critical)
                    {
                        sb.AppendLine("<div class='alert alert-critical'>");
                        sb.AppendLine($"<strong><span class='severity-critical'>CRITICAL</span> {finding.Title}</strong>");
                        sb.AppendLine($"<p>{finding.Description}</p>");
                        sb.AppendLine($"<p><em>Recommendation: {finding.Recommendation}</em></p>");
                        sb.AppendLine("</div>");
                    }
                }

                if (high.Any())
                {
                    sb.AppendLine("<h3>High Severity Findings</h3>");
                    foreach (var finding in high)
                    {
                        sb.AppendLine("<div class='alert alert-warning'>");
                        sb.AppendLine($"<strong><span class='severity-high'>HIGH</span> {finding.Title}</strong>");
                        sb.AppendLine($"<p>{finding.Description}</p>");
                        sb.AppendLine($"<p><em>Recommendation: {finding.Recommendation}</em></p>");
                        sb.AppendLine("</div>");
                    }
                }

                if (medium.Any())
                {
                    sb.AppendLine("<h3>Medium Severity Findings</h3>");
                    foreach (var finding in medium)
                    {
                        sb.AppendLine("<div class='alert alert-info'>");
                        sb.AppendLine($"<strong><span class='severity-medium'>MEDIUM</span> {finding.Title}</strong>");
                        sb.AppendLine($"<p>{finding.Description}</p>");
                        sb.AppendLine($"<p><em>Recommendation: {finding.Recommendation}</em></p>");
                        sb.AppendLine("</div>");
                    }
                }
            }
            else
            {
                sb.AppendLine("<div class='alert alert-info'>");
                sb.AppendLine("<strong>No major security findings detected in this session.</strong>");
                sb.AppendLine("</div>");
            }
        }

        private void AppendVulnerabilityDetails(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Vulnerability Assessment</h2>");
            sb.AppendLine("<p>Detailed vulnerability information would be populated by security scanners.</p>");
        }

        private void AppendTrafficAnalysis(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Traffic Pattern Analysis</h2>");

            // Content Type Analysis
            sb.AppendLine("<h3>Content Types</h3>");
            var contentTypes = new Dictionary<string, int>();
            foreach (var entry in entries)
            {
                var contentType = entry.ContentType ?? "Unknown";
                if (contentType.Contains(";"))
                    contentType = contentType.Split(';')[0].Trim();

                if (!contentTypes.ContainsKey(contentType))
                    contentTypes[contentType] = 0;
                contentTypes[contentType]++;
            }

            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Content Type</th><th>Count</th></tr>");
            foreach (var ct in contentTypes.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"<tr><td>{ct.Key}</td><td>{ct.Value}</td></tr>");
            }
            sb.AppendLine("</table>");
        }

        private void AppendDetailedTrafficLog(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Detailed Traffic Log</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>#</th><th>Method</th><th>Host</th><th>Path</th><th>Status</th><th>Length</th></tr>");

            int index = 1;
            foreach (var entry in entries.Take(100)) // Limit to first 100
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{index++}</td>");
                sb.AppendLine($"<td><strong>{entry.Method}</strong></td>");
                sb.AppendLine($"<td>{entry.Host}</td>");
                sb.AppendLine($"<td>{TruncateString(entry.Path, 50)}</td>");
                sb.AppendLine($"<td>{entry.Status}</td>");
                sb.AppendLine($"<td>{FormatBytes(entry.Length)}</td>");
                sb.AppendLine("</tr>");
            }

            if (entries.Count > 100)
            {
                sb.AppendLine($"<tr><td colspan='6'><em>... and {entries.Count - 100} more entries</em></td></tr>");
            }

            sb.AppendLine("</table>");
        }

        private void AppendRecommendations(StringBuilder sb, List<TrafficEntry> entries)
        {
            sb.AppendLine("<h2>Security Recommendations</h2>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Ensure all traffic uses HTTPS encryption</li>");
            sb.AppendLine("<li>Implement proper input validation for all user inputs</li>");
            sb.AppendLine("<li>Add security headers (HSTS, CSP, X-Frame-Options, etc.)</li>");
            sb.AppendLine("<li>Regular security audits and penetration testing</li>");
            sb.AppendLine("<li>Keep all software and dependencies up to date</li>");
            sb.AppendLine("<li>Implement rate limiting to prevent abuse</li>");
            sb.AppendLine("<li>Use parameterized queries to prevent SQL injection</li>");
            sb.AppendLine("<li>Implement proper session management</li>");
            sb.AppendLine("</ul>");
        }

        private string GetStatusCodeDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                301 => "Moved Permanently",
                302 => "Found",
                304 => "Not Modified",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "Other"
            };
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string ExtractHeaders(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse))
                return string.Empty;

            var lines = rawResponse.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var headerLines = new List<string>();

            // Skip the first line (HTTP status line) and collect headers until we hit an empty line
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    break;
                headerLines.Add(lines[i]);
            }

            return string.Join("\r\n", headerLines);
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
                return str ?? "";
            return str.Substring(0, maxLength) + "...";
        }

        public void SaveReportToFile(string html, string filePath)
        {
            File.WriteAllText(filePath, html, Encoding.UTF8);
        }
    }

    #region Models

    public class ReportOptions
    {
        public bool IncludeVulnerabilityDetails { get; set; } = true;
        public bool IncludeTrafficAnalysis { get; set; } = true;
        public bool IncludeDetailedLog { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
    }

    public class SecurityFinding
    {
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }

    #endregion
}
