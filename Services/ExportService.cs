using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Comprehensive export service for multiple formats
    /// </summary>
    public class ExportService
    {
        /// <summary>
        /// Export to HAR (HTTP Archive) format - compatible with most tools
        /// </summary>
        public string ExportToHAR(List<TrafficEntry> entries, string sessionName = "WebTrafficInspector")
        {
            var har = new
            {
                log = new
                {
                    version = "1.2",
                    creator = new
                    {
                        name = "WebTrafficInspector",
                        version = "1.0"
                    },
                    pages = new[]
                    {
                        new
                        {
                            startedDateTime = entries.FirstOrDefault()?.Timestamp.ToString("o") ?? DateTime.Now.ToString("o"),
                            id = "page_1",
                            title = sessionName,
                            pageTimings = new
                            {
                                onContentLoad = -1,
                                onLoad = -1
                            }
                        }
                    },
                    entries = entries.Select((entry, index) => new
                    {
                        startedDateTime = entry.Timestamp.ToString("o"),
                        time = 0, // We don't track timing
                        request = new
                        {
                            method = entry.Method,
                            url = $"http://{entry.Host}{entry.Path}",
                            httpVersion = "HTTP/1.1",
                            headers = ParseHeaders(entry.RawRequest, "request"),
                            queryString = ParseQueryString(entry.Path),
                            cookies = new object[] { },
                            headersSize = -1,
                            bodySize = entry.RawRequest?.Length ?? 0,
                            postData = ExtractPostData(entry.RawRequest)
                        },
                        response = new
                        {
                            status = entry.Status,
                            statusText = GetStatusText(entry.Status),
                            httpVersion = "HTTP/1.1",
                            headers = ParseHeaders(entry.RawResponse, "response"),
                            cookies = new object[] { },
                            content = new
                            {
                                size = entry.Length,
                                mimeType = ExtractContentType(entry.RawResponse),
                                text = ExtractResponseBody(entry.RawResponse)
                            },
                            redirectURL = "",
                            headersSize = -1,
                            bodySize = entry.Length
                        },
                        cache = new { },
                        timings = new
                        {
                            send = 0,
                            wait = 0,
                            receive = 0
                        },
                        pageref = "page_1"
                    }).ToArray()
                }
            };

            return JsonSerializer.Serialize(har, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Export to detailed JSON format
        /// </summary>
        public string ExportToJSON(List<TrafficEntry> entries)
        {
            var export = new
            {
                exportDate = DateTime.Now.ToString("o"),
                tool = "WebTrafficInspector",
                version = "1.0",
                totalEntries = entries.Count,
                entries = entries.Select(e => new
                {
                    id = e.Id,
                    timestamp = e.Timestamp.ToString("o"),
                    method = e.Method,
                    host = e.Host,
                    path = e.Path,
                    url = $"http://{e.Host}{e.Path}",
                    status = e.Status,
                    statusText = GetStatusText(e.Status),
                    length = e.Length,
                    rawRequest = e.RawRequest,
                    rawResponse = e.RawResponse,
                    isPinned = e.IsPinned,
                    tags = e.Tags,
                    notes = e.Notes,
                    color = e.Color
                }).ToArray()
            };

            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Export to CSV format
        /// </summary>
        public string ExportToCSV(List<TrafficEntry> entries)
        {
            var csv = new StringBuilder();
            csv.AppendLine("ID,Timestamp,Method,Host,Path,URL,Status,Length,Tags,Notes");

            foreach (var entry in entries)
            {
                csv.AppendLine($"{entry.Id}," +
                              $"\"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\"," +
                              $"{entry.Method}," +
                              $"\"{entry.Host}\"," +
                              $"\"{EscapeCsv(entry.Path)}\"," +
                              $"\"{entry.Host}{entry.Path}\"," +
                              $"{entry.Status}," +
                              $"{entry.Length}," +
                              $"\"{EscapeCsv(entry.Tags ?? "")}\"," +
                              $"\"{EscapeCsv(entry.Notes ?? "")}\"");
            }

            return csv.ToString();
        }

        /// <summary>
        /// Export to XML format
        /// </summary>
        public string ExportToXML(List<TrafficEntry> entries)
        {
            var root = new XElement("TrafficSession",
                new XAttribute("ExportDate", DateTime.Now.ToString("o")),
                new XAttribute("TotalEntries", entries.Count),
                entries.Select(entry => new XElement("Entry",
                    new XElement("Id", entry.Id),
                    new XElement("Timestamp", entry.Timestamp.ToString("o")),
                    new XElement("Method", entry.Method),
                    new XElement("Host", entry.Host),
                    new XElement("Path", entry.Path),
                    new XElement("URL", $"http://{entry.Host}{entry.Path}"),
                    new XElement("Status", entry.Status),
                    new XElement("Length", entry.Length),
                    new XElement("RawRequest", new XCData(entry.RawRequest ?? "")),
                    new XElement("RawResponse", new XCData(entry.RawResponse ?? "")),
                    new XElement("IsPinned", entry.IsPinned),
                    new XElement("Tags", entry.Tags ?? ""),
                    new XElement("Notes", entry.Notes ?? ""),
                    new XElement("Color", entry.Color ?? "")
                ))
            );

            return root.ToString();
        }

        /// <summary>
        /// Export to Markdown format (for documentation/reports)
        /// </summary>
        public string ExportToMarkdown(List<TrafficEntry> entries, string title = "Traffic Analysis Report")
        {
            var md = new StringBuilder();

            md.AppendLine($"# {title}");
            md.AppendLine();
            md.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            md.AppendLine($"**Total Entries:** {entries.Count}");
            md.AppendLine();

            // Summary statistics
            md.AppendLine("## Summary");
            md.AppendLine();
            md.AppendLine("| Metric | Value |");
            md.AppendLine("|--------|-------|");
            md.AppendLine($"| Total Requests | {entries.Count} |");
            md.AppendLine($"| Unique Hosts | {entries.Select(e => e.Host).Distinct().Count()} |");
            md.AppendLine($"| HTTP Methods | {string.Join(", ", entries.GroupBy(e => e.Method).Select(g => $"{g.Key} ({g.Count()})"))} |");
            md.AppendLine($"| Status Codes | {string.Join(", ", entries.GroupBy(e => e.Status).Select(g => $"{g.Key} ({g.Count()})"))} |");
            md.AppendLine();

            // Detailed entries
            md.AppendLine("## Requests");
            md.AppendLine();
            md.AppendLine("| # | Time | Method | Host | Path | Status |");
            md.AppendLine("|---|------|--------|------|------|--------|");

            foreach (var entry in entries)
            {
                md.AppendLine($"| {entry.Id} | {entry.Timestamp:HH:mm:ss} | {entry.Method} | {entry.Host} | `{entry.Path}` | {entry.Status} |");
            }

            md.AppendLine();

            // Tagged entries
            var taggedEntries = entries.Where(e => !string.IsNullOrEmpty(e.Tags)).ToList();
            if (taggedEntries.Any())
            {
                md.AppendLine("## Tagged Entries");
                md.AppendLine();
                foreach (var entry in taggedEntries)
                {
                    md.AppendLine($"### Entry #{entry.Id} - {entry.Method} {entry.Host}{entry.Path}");
                    md.AppendLine($"**Tags:** {entry.Tags}");
                    if (!string.IsNullOrEmpty(entry.Notes))
                    {
                        md.AppendLine($"**Notes:** {entry.Notes}");
                    }
                    md.AppendLine();
                }
            }

            return md.ToString();
        }

        /// <summary>
        /// Export to Burp Suite format (basic compatibility)
        /// </summary>
        public string ExportToBurp(List<TrafficEntry> entries)
        {
            var burpItems = entries.Select(entry => new
            {
                host = entry.Host,
                port = ExtractPort(entry.Host),
                protocol = "http",
                method = entry.Method,
                path = entry.Path,
                request = Convert.ToBase64String(Encoding.UTF8.GetBytes(entry.RawRequest ?? "")),
                response = Convert.ToBase64String(Encoding.UTF8.GetBytes(entry.RawResponse ?? "")),
                status = entry.Status,
                responseLength = entry.Length,
                time = entry.Timestamp.ToString("o"),
                comment = entry.Notes ?? "",
                highlight = entry.Color ?? ""
            }).ToArray();

            return JsonSerializer.Serialize(new { items = burpItems }, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Export security findings to HTML report
        /// </summary>
        public string ExportSecurityReport(List<SecurityReport> reports, string sessionName)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<title>Security Analysis Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
            html.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            html.AppendLine("h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #555; margin-top: 30px; }");
            html.AppendLine(".summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin: 20px 0; }");
            html.AppendLine(".stat-card { padding: 15px; border-radius: 5px; text-align: center; }");
            html.AppendLine(".high-risk { background: #ffebee; color: #c62828; }");
            html.AppendLine(".medium-risk { background: #fff3e0; color: #ef6c00; }");
            html.AppendLine(".low-risk { background: #e8f5e9; color: #2e7d32; }");
            html.AppendLine(".info { background: #e3f2fd; color: #1565c0; }");
            html.AppendLine(".issue { margin: 10px 0; padding: 10px; border-left: 4px solid #ddd; background: #f9f9f9; }");
            html.AppendLine(".issue.critical { border-left-color: #d32f2f; }");
            html.AppendLine(".issue.high { border-left-color: #f57c00; }");
            html.AppendLine(".issue.medium { border-left-color: #fbc02d; }");
            html.AppendLine(".issue.low { border-left-color: #388e3c; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }");
            html.AppendLine("th { background: #4CAF50; color: white; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine("<div class='container'>");

            // Header
            html.AppendLine($"<h1>Security Analysis Report: {sessionName}</h1>");
            html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            // Summary
            var highRisk = reports.Count(r => r.RiskLevel == "High");
            var mediumRisk = reports.Count(r => r.RiskLevel == "Medium");
            var lowRisk = reports.Count(r => r.RiskLevel == "Low");
            var totalIssues = reports.Sum(r => r.TotalIssues);

            html.AppendLine("<div class='summary'>");
            html.AppendLine($"<div class='stat-card high-risk'><h3>{highRisk}</h3><p>High Risk</p></div>");
            html.AppendLine($"<div class='stat-card medium-risk'><h3>{mediumRisk}</h3><p>Medium Risk</p></div>");
            html.AppendLine($"<div class='stat-card low-risk'><h3>{lowRisk}</h3><p>Low Risk</p></div>");
            html.AppendLine($"<div class='stat-card info'><h3>{totalIssues}</h3><p>Total Issues</p></div>");
            html.AppendLine("</div>");

            // Detailed findings
            html.AppendLine("<h2>Detailed Findings</h2>");
            foreach (var report in reports.Where(r => r.TotalIssues > 0).OrderByDescending(r => r.RiskScore))
            {
                html.AppendLine($"<h3>Entry #{report.EntryId}: {report.Url}</h3>");
                html.AppendLine($"<p><strong>Risk Level:</strong> {report.RiskLevel} | <strong>Risk Score:</strong> {report.RiskScore}/100</p>");

                foreach (var issue in report.RequestIssues.Concat(report.ResponseIssues).Concat(report.InjectionAttempts))
                {
                    html.AppendLine($"<div class='issue {issue.Severity.ToLower()}'>");
                    html.AppendLine($"<strong>{issue.Type}</strong> [{issue.Severity}]<br>");
                    html.AppendLine($"{issue.Description}<br>");
                    html.AppendLine($"<em>Recommendation: {issue.Recommendation}</em>");
                    html.AppendLine("</div>");
                }
            }

            html.AppendLine("</div></body></html>");
            return html.ToString();
        }

        // Helper methods
        private List<object> ParseHeaders(string raw, string type)
        {
            var headers = new List<object>();
            if (string.IsNullOrEmpty(raw)) return headers;

            var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) break;

                var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    headers.Add(new { name = parts[0], value = parts[1] });
                }
            }

            return headers;
        }

        private List<object> ParseQueryString(string path)
        {
            var queryParams = new List<object>();
            if (!path.Contains("?")) return queryParams;

            var queryString = path.Substring(path.IndexOf("?") + 1);
            var pairs = queryString.Split('&');

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                queryParams.Add(new
                {
                    name = parts[0],
                    value = parts.Length > 1 ? parts[1] : ""
                });
            }

            return queryParams;
        }

        private object ExtractPostData(string rawRequest)
        {
            if (string.IsNullOrEmpty(rawRequest)) return null;

            var lines = rawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var bodyStart = Array.FindIndex(lines, string.IsNullOrWhiteSpace);

            if (bodyStart >= 0 && bodyStart < lines.Length - 1)
            {
                var body = string.Join("\n", lines.Skip(bodyStart + 1));
                if (!string.IsNullOrWhiteSpace(body))
                {
                    return new
                    {
                        mimeType = "application/x-www-form-urlencoded",
                        text = body
                    };
                }
            }

            return null;
        }

        private string ExtractContentType(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse)) return "text/plain";

            var lines = rawResponse.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var contentTypeLine = lines.FirstOrDefault(l => l.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase));

            if (contentTypeLine != null)
            {
                return contentTypeLine.Split(new char[] { ':', ' ' }, 2)[1].Split(';')[0].Trim();
            }

            return "text/plain";
        }

        private string ExtractResponseBody(string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse)) return "";

            var lines = rawResponse.Split(new[] { "\r\n\r\n", "\n\n" }, 2, StringSplitOptions.None);
            return lines.Length > 1 ? lines[1] : "";
        }

        private string GetStatusText(int status)
        {
            return status switch
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
                _ => "Unknown"
            };
        }

        private int ExtractPort(string host)
        {
            if (host.Contains(":"))
            {
                var parts = host.Split(':');
                if (int.TryParse(parts[1], out int port))
                    return port;
            }
            return 80;
        }

        private string EscapeCsv(string value)
        {
            if (value == null) return "";
            return value.Replace("\"", "\"\"");
        }

        /// <summary>
        /// Import from JSON format
        /// </summary>
        public List<TrafficEntry> ImportFromJson(string json)
        {
            try
            {
                var export = JsonSerializer.Deserialize<JsonElement>(json);
                var entries = new List<TrafficEntry>();

                if (export.TryGetProperty("entries", out var entriesElement) && entriesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entryElement in entriesElement.EnumerateArray())
                    {
                        var entry = new TrafficEntry
                        {
                            Id = entryElement.TryGetProperty("id", out var id) ? id.GetInt32() : entries.Count + 1,
                            Timestamp = entryElement.TryGetProperty("timestamp", out var timestamp) ?
                                DateTime.Parse(timestamp.GetString()) : DateTime.Now,
                            Method = entryElement.TryGetProperty("method", out var method) ? method.GetString() : "GET",
                            Host = entryElement.TryGetProperty("host", out var host) ? host.GetString() : "",
                            Path = entryElement.TryGetProperty("path", out var path) ? path.GetString() : "/",
                            Status = entryElement.TryGetProperty("status", out var status) ? status.GetInt32() : 200,
                            Length = entryElement.TryGetProperty("length", out var length) ? length.GetInt64() : 0,
                            RawRequest = entryElement.TryGetProperty("rawRequest", out var rawRequest) ? rawRequest.GetString() : "",
                            RawResponse = entryElement.TryGetProperty("rawResponse", out var rawResponse) ? rawResponse.GetString() : "",
                            IsPinned = entryElement.TryGetProperty("isPinned", out var isPinned) && isPinned.GetBoolean(),
                            Tags = entryElement.TryGetProperty("tags", out var tags) ? tags.GetString() : "",
                            Notes = entryElement.TryGetProperty("notes", out var notes) ? notes.GetString() : "",
                            Color = entryElement.TryGetProperty("color", out var color) ? color.GetString() : ""
                        };
                        entries.Add(entry);
                    }
                }

                return entries;
            }
            catch
            {
                // Return empty list if parsing fails
                return new List<TrafficEntry>();
            }
        }

        /// <summary>
        /// Import from Burp Suite XML format
        /// </summary>
        public List<TrafficEntry> ImportFromBurp(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var entries = new List<TrafficEntry>();
                var id = 1;

                foreach (var item in doc.Descendants("item"))
                {
                    var entry = new TrafficEntry
                    {
                        Id = id++,
                        Timestamp = DateTime.Now,
                        Method = item.Element("method")?.Value ?? "GET",
                        Host = item.Element("host")?.Value ?? "",
                        Path = item.Element("url")?.Value ?? "/",
                        Status = int.TryParse(item.Element("status")?.Value, out var status) ? status : 0,
                        Length = long.TryParse(item.Element("responselength")?.Value, out var length) ? length : 0,
                        RawRequest = DecodeBase64(item.Element("request")?.Value ?? ""),
                        RawResponse = DecodeBase64(item.Element("response")?.Value ?? ""),
                        IsPinned = false,
                        Tags = "",
                        Notes = "",
                        Color = ""
                    };
                    entries.Add(entry);
                }

                return entries;
            }
            catch
            {
                // Return empty list if parsing fails
                return new List<TrafficEntry>();
            }
        }

        private string DecodeBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return "";

            try
            {
                var bytes = Convert.FromBase64String(base64String);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return base64String; // Return as-is if not valid base64
            }
        }
    }
}
