using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class AttackSurfaceMapperService
    {
        public AttackSurfaceMap MapAttackSurface(List<TrafficEntry> entries)
        {
            var map = new AttackSurfaceMap
            {
                GeneratedAt = DateTime.Now,
                TotalRequests = entries.Count
            };

            // Extract endpoints
            map.Endpoints = ExtractEndpoints(entries);
            map.TotalEndpoints = map.Endpoints.Count;

            // Extract parameters
            map.InputPoints = ExtractInputPoints(entries);
            map.TotalInputPoints = map.InputPoints.Count;

            // Extract technologies
            map.Technologies = DetectTechnologies(entries);

            // Extract authentication endpoints
            map.AuthenticationEndpoints = FindAuthenticationEndpoints(entries);

            // Extract file upload endpoints
            map.FileUploadEndpoints = FindFileUploadEndpoints(entries);

            // Extract admin/sensitive paths
            map.SensitivePaths = FindSensitivePaths(entries);

            // Extract subdomains
            map.Subdomains = entries.Select(e => e.Host).Distinct().ToList();
            map.TotalSubdomains = map.Subdomains.Count;

            // Extract API versions
            map.ApiVersions = ExtractApiVersions(entries);

            // Identify entry points by type
            map.EntryPointsByType = ClassifyEntryPoints(map.InputPoints);

            // Calculate risk score
            map.RiskScore = CalculateRiskScore(map);
            map.RiskLevel = GetRiskLevel(map.RiskScore);

            return map;
        }

        private List<EndpointEntry> ExtractEndpoints(List<TrafficEntry> entries)
        {
            return entries
                .GroupBy(e => new { e.Method, e.Path })
                .Select(g => new EndpointEntry
                {
                    Method = g.Key.Method,
                    Path = g.Key.Path,
                    FullUrl = g.First().Url,
                    RequestCount = g.Count(),
                    Parameters = ExtractParametersFromPath(g.Key.Path),
                    HasAuthentication = g.Any(e => e.RawRequest?.Contains("Authorization:") == true ||
                                                   e.RawRequest?.Contains("Cookie:") == true),
                    AverageResponseTime = g.Average(e => e.Duration),
                    StatusCodes = g.Select(e => e.StatusCode).Distinct().ToList(),
                    ContentTypes = g.Select(e => e.ContentType).Where(ct => ct != null).Distinct().ToList()
                })
                .OrderBy(e => e.Path)
                .ToList();
        }

        private List<InputPoint> ExtractInputPoints(List<TrafficEntry> entries)
        {
            var inputPoints = new List<InputPoint>();

            foreach (var entry in entries)
            {
                // Query parameters
                if (entry.Path.Contains('?'))
                {
                    var queryString = entry.Path.Split('?')[1];
                    var parameters = queryString.Split('&');

                    foreach (var param in parameters)
                    {
                        var parts = param.Split('=');
                        if (parts.Length >= 1)
                        {
                            inputPoints.Add(new InputPoint
                            {
                                Name = parts[0],
                                Type = InputPointType.QueryParameter,
                                Endpoint = entry.Path.Split('?')[0],
                                Method = entry.Method,
                                SampleValue = parts.Length > 1 ? parts[1] : ""
                            });
                        }
                    }
                }

                // POST body parameters
                if (entry.Method == "POST" && !string.IsNullOrEmpty(entry.RawRequest))
                {
                    var bodyParams = ExtractPostParameters(entry.RawRequest);
                    foreach (var param in bodyParams)
                    {
                        inputPoints.Add(new InputPoint
                        {
                            Name = param.Key,
                            Type = InputPointType.BodyParameter,
                            Endpoint = entry.Path,
                            Method = entry.Method,
                            SampleValue = param.Value
                        });
                    }
                }

                // Headers
                if (!string.IsNullOrEmpty(entry.RawRequest))
                {
                    var customHeaders = ExtractCustomHeaders(entry.RawRequest);
                    foreach (var header in customHeaders)
                    {
                        inputPoints.Add(new InputPoint
                        {
                            Name = header.Key,
                            Type = InputPointType.Header,
                            Endpoint = entry.Path,
                            Method = entry.Method,
                            SampleValue = header.Value
                        });
                    }
                }

                // Cookies
                if (!string.IsNullOrEmpty(entry.RawRequest) && entry.RawRequest.Contains("Cookie:"))
                {
                    var cookies = ExtractCookieNames(entry.RawRequest);
                    foreach (var cookie in cookies)
                    {
                        inputPoints.Add(new InputPoint
                        {
                            Name = cookie,
                            Type = InputPointType.Cookie,
                            Endpoint = entry.Path,
                            Method = entry.Method
                        });
                    }
                }

                // Path parameters (e.g., /users/{id})
                var pathParams = ExtractPathParameters(entry.Path);
                foreach (var param in pathParams)
                {
                    inputPoints.Add(new InputPoint
                    {
                        Name = param,
                        Type = InputPointType.PathParameter,
                        Endpoint = entry.Path,
                        Method = entry.Method
                    });
                }
            }

            // Remove duplicates
            return inputPoints
                .GroupBy(ip => new { ip.Name, ip.Type, ip.Endpoint, ip.Method })
                .Select(g => g.First())
                .ToList();
        }

        private List<Technology> DetectTechnologies(List<TrafficEntry> entries)
        {
            var technologies = new List<Technology>();

            foreach (var entry in entries)
            {
                var response = entry.RawResponse ?? "";
                var headers = entry.RawRequest ?? "";

                // Server header
                var serverMatch = Regex.Match(response, @"Server:\s*([^\r\n]+)", RegexOptions.IgnoreCase);
                if (serverMatch.Success)
                {
                    technologies.Add(new Technology
                    {
                        Name = "Server",
                        Version = serverMatch.Groups[1].Value.Trim(),
                        Confidence = 100,
                        Category = "Server"
                    });
                }

                // X-Powered-By header
                var poweredByMatch = Regex.Match(response, @"X-Powered-By:\s*([^\r\n]+)", RegexOptions.IgnoreCase);
                if (poweredByMatch.Success)
                {
                    technologies.Add(new Technology
                    {
                        Name = "Framework",
                        Version = poweredByMatch.Groups[1].Value.Trim(),
                        Confidence = 100,
                        Category = "Framework"
                    });
                }

                // Detect frameworks from response patterns
                if (response.Contains("wp-content") || response.Contains("wp-includes"))
                {
                    technologies.Add(new Technology { Name = "WordPress", Confidence = 90, Category = "CMS" });
                }
                if (response.Contains("Drupal") || response.Contains("/sites/default/"))
                {
                    technologies.Add(new Technology { Name = "Drupal", Confidence = 85, Category = "CMS" });
                }
                if (response.Contains("__VIEWSTATE") || response.Contains("ASP.NET"))
                {
                    technologies.Add(new Technology { Name = "ASP.NET", Confidence = 95, Category = "Framework" });
                }
                if (response.Contains("ng-app") || response.Contains("angular"))
                {
                    technologies.Add(new Technology { Name = "Angular", Confidence = 80, Category = "Frontend" });
                }
                if (response.Contains("react") || response.Contains("_reactRoot"))
                {
                    technologies.Add(new Technology { Name = "React", Confidence = 80, Category = "Frontend" });
                }
                if (response.Contains("vue"))
                {
                    technologies.Add(new Technology { Name = "Vue.js", Confidence = 75, Category = "Frontend" });
                }
                if (response.Contains("jQuery"))
                {
                    technologies.Add(new Technology { Name = "jQuery", Confidence = 90, Category = "Frontend" });
                }
                if (response.Contains("bootstrap"))
                {
                    technologies.Add(new Technology { Name = "Bootstrap", Confidence = 85, Category = "Frontend" });
                }
            }

            // Remove duplicates and return unique technologies
            return technologies
                .GroupBy(t => t.Name)
                .Select(g => g.OrderByDescending(t => t.Confidence).First())
                .OrderByDescending(t => t.Confidence)
                .ToList();
        }

        private List<string> FindAuthenticationEndpoints(List<TrafficEntry> entries)
        {
            var authPatterns = new[]
            {
                "login", "signin", "auth", "authenticate", "oauth", "token",
                "register", "signup", "logout", "signout", "password", "reset"
            };

            return entries
                .Where(e => authPatterns.Any(pattern =>
                    e.Path.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                .Select(e => $"{e.Method} {e.Path}")
                .Distinct()
                .ToList();
        }

        private List<string> FindFileUploadEndpoints(List<TrafficEntry> entries)
        {
            return entries
                .Where(e => e.Method == "POST" &&
                           (e.ContentType?.Contains("multipart/form-data") == true ||
                            e.Path.Contains("upload", StringComparison.OrdinalIgnoreCase) ||
                            e.Path.Contains("file", StringComparison.OrdinalIgnoreCase)))
                .Select(e => e.Path)
                .Distinct()
                .ToList();
        }

        private List<SensitivePath> FindSensitivePaths(List<TrafficEntry> entries)
        {
            var sensitivePaths = new List<SensitivePath>();
            var sensitivePatterns = new Dictionary<string, string>
            {
                ["admin"] = "Administrative interface",
                ["api"] = "API endpoint",
                ["backup"] = "Backup files",
                ["config"] = "Configuration files",
                ["debug"] = "Debug information",
                ["test"] = "Test endpoints",
                [".git"] = "Version control",
                [".env"] = "Environment configuration",
                ["swagger"] = "API documentation",
                ["graphql"] = "GraphQL endpoint",
                ["phpinfo"] = "PHP configuration info",
                ["server-status"] = "Server status",
                ["wp-admin"] = "WordPress admin"
            };

            foreach (var entry in entries)
            {
                foreach (var pattern in sensitivePatterns)
                {
                    if (entry.Path.Contains(pattern.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        sensitivePaths.Add(new SensitivePath
                        {
                            Path = entry.Path,
                            Pattern = pattern.Key,
                            Description = pattern.Value,
                            StatusCode = entry.StatusCode,
                            IsAccessible = entry.StatusCode >= 200 && entry.StatusCode < 300
                        });
                    }
                }
            }

            return sensitivePaths.Distinct().ToList();
        }

        private List<string> ExtractApiVersions(List<TrafficEntry> entries)
        {
            var versions = new HashSet<string>();
            var versionPattern = new Regex(@"/(?:api/)?v(\d+)(?:\.\d+)?", RegexOptions.IgnoreCase);

            foreach (var entry in entries)
            {
                var match = versionPattern.Match(entry.Path);
                if (match.Success)
                {
                    versions.Add("v" + match.Groups[1].Value);
                }
            }

            return versions.OrderBy(v => v).ToList();
        }

        private Dictionary<string, int> ClassifyEntryPoints(List<InputPoint> inputPoints)
        {
            return inputPoints
                .GroupBy(ip => ip.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private List<string> ExtractParametersFromPath(string path)
        {
            var parameters = new List<string>();
            if (path.Contains('?'))
            {
                var queryString = path.Split('?')[1];
                parameters = queryString.Split('&').Select(p => p.Split('=')[0]).ToList();
            }
            return parameters;
        }

        private Dictionary<string, string> ExtractPostParameters(string rawRequest)
        {
            var parameters = new Dictionary<string, string>();
            var bodyStartIndex = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStartIndex == -1) return parameters;

            var body = rawRequest.Substring(bodyStartIndex + 4);

            // Try to parse as JSON
            try
            {
                var jsonDoc = System.Text.Json.JsonDocument.Parse(body);
                ExtractJsonProperties(jsonDoc.RootElement, "", parameters);
            }
            catch
            {
                // Try to parse as form data
                var parts = body.Split('&');
                foreach (var part in parts)
                {
                    var keyValue = part.Split('=');
                    if (keyValue.Length >= 1)
                    {
                        parameters[keyValue[0]] = keyValue.Length > 1 ? keyValue[1] : "";
                    }
                }
            }

            return parameters;
        }

        private void ExtractJsonProperties(System.Text.Json.JsonElement element, string prefix, Dictionary<string, string> parameters)
        {
            if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Object ||
                        property.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        ExtractJsonProperties(property.Value, key, parameters);
                    }
                    else
                    {
                        parameters[key] = property.Value.ToString();
                    }
                }
            }
        }

        private Dictionary<string, string> ExtractCustomHeaders(string rawRequest)
        {
            var headers = new Dictionary<string, string>();
            var standardHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Host", "User-Agent", "Accept", "Accept-Language", "Accept-Encoding",
                "Connection", "Content-Type", "Content-Length", "Referer", "Cookie"
            };

            var lines = rawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines.Skip(1)) // Skip first line (request line)
            {
                if (string.IsNullOrWhiteSpace(line)) break;

                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var headerName = line.Substring(0, colonIndex).Trim();
                    var headerValue = line.Substring(colonIndex + 1).Trim();

                    if (!standardHeaders.Contains(headerName))
                    {
                        headers[headerName] = headerValue;
                    }
                }
            }

            return headers;
        }

        private List<string> ExtractCookieNames(string rawRequest)
        {
            var cookies = new List<string>();
            var cookieMatch = Regex.Match(rawRequest, @"Cookie:\s*([^\r\n]+)", RegexOptions.IgnoreCase);

            if (cookieMatch.Success)
            {
                var cookieString = cookieMatch.Groups[1].Value;
                var parts = cookieString.Split(';');

                foreach (var part in parts)
                {
                    var nameValue = part.Trim().Split('=');
                    if (nameValue.Length > 0)
                    {
                        cookies.Add(nameValue[0]);
                    }
                }
            }

            return cookies;
        }

        private List<string> ExtractPathParameters(string path)
        {
            // Simple numeric ID detection
            var parameters = new List<string>();
            var matches = Regex.Matches(path, @"/(\d+)(?:/|$)");

            foreach (Match match in matches)
            {
                parameters.Add("id_" + match.Groups[1].Value);
            }

            return parameters;
        }

        private int CalculateRiskScore(AttackSurfaceMap map)
        {
            int score = 0;

            // More endpoints = higher attack surface
            score += Math.Min(map.TotalEndpoints, 50);

            // More input points = more attack vectors
            score += Math.Min(map.TotalInputPoints / 2, 30);

            // Accessible sensitive paths increase risk
            score += map.SensitivePaths.Count(sp => sp.IsAccessible) * 5;

            // File upload endpoints are risky
            score += map.FileUploadEndpoints.Count * 3;

            // Unauthenticated endpoints
            score += map.Endpoints.Count(e => !e.HasAuthentication) / 2;

            return Math.Min(score, 100);
        }

        private string GetRiskLevel(int score)
        {
            if (score < 30) return "Low";
            if (score < 60) return "Medium";
            if (score < 80) return "High";
            return "Critical";
        }

        public string GenerateReport(AttackSurfaceMap map)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("          ATTACK SURFACE MAPPING REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Generated: {map.GeneratedAt}");
            sb.AppendLine($"Risk Level: {map.RiskLevel} ({map.RiskScore}/100)");
            sb.AppendLine();

            sb.AppendLine("OVERVIEW:");
            sb.AppendLine($"  Total Endpoints:       {map.TotalEndpoints}");
            sb.AppendLine($"  Total Input Points:    {map.TotalInputPoints}");
            sb.AppendLine($"  Subdomains:            {map.TotalSubdomains}");
            sb.AppendLine($"  Technologies:          {map.Technologies.Count}");
            sb.AppendLine();

            if (map.Technologies.Any())
            {
                sb.AppendLine("DETECTED TECHNOLOGIES:");
                foreach (var tech in map.Technologies.Take(10))
                {
                    sb.AppendLine($"  {tech.Name,-20} {tech.Category,-15} ({tech.Confidence}% confidence)");
                }
                sb.AppendLine();
            }

            if (map.EntryPointsByType.Any())
            {
                sb.AppendLine("INPUT POINTS BY TYPE:");
                foreach (var type in map.EntryPointsByType.OrderByDescending(x => x.Value))
                {
                    sb.AppendLine($"  {type.Key,-20} {type.Value,5}");
                }
                sb.AppendLine();
            }

            if (map.AuthenticationEndpoints.Any())
            {
                sb.AppendLine($"AUTHENTICATION ENDPOINTS ({map.AuthenticationEndpoints.Count}):");
                foreach (var endpoint in map.AuthenticationEndpoints.Take(10))
                {
                    sb.AppendLine($"  {endpoint}");
                }
                sb.AppendLine();
            }

            if (map.SensitivePaths.Any())
            {
                sb.AppendLine($"SENSITIVE PATHS ({map.SensitivePaths.Count}):");
                foreach (var path in map.SensitivePaths.Take(15))
                {
                    var status = path.IsAccessible ? "ACCESSIBLE" : "PROTECTED";
                    sb.AppendLine($"  [{status}] {path.Path} - {path.Description}");
                }
                sb.AppendLine();
            }

            if (map.FileUploadEndpoints.Any())
            {
                sb.AppendLine($"FILE UPLOAD ENDPOINTS ({map.FileUploadEndpoints.Count}):");
                foreach (var endpoint in map.FileUploadEndpoints)
                {
                    sb.AppendLine($"  {endpoint}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    public class AttackSurfaceMap
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalRequests { get; set; }
        public int TotalEndpoints { get; set; }
        public int TotalInputPoints { get; set; }
        public int TotalSubdomains { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }

        public List<EndpointEntry> Endpoints { get; set; } = new List<EndpointEntry>();
        public List<InputPoint> InputPoints { get; set; } = new List<InputPoint>();
        public List<Technology> Technologies { get; set; } = new List<Technology>();
        public List<string> AuthenticationEndpoints { get; set; } = new List<string>();
        public List<string> FileUploadEndpoints { get; set; } = new List<string>();
        public List<SensitivePath> SensitivePaths { get; set; } = new List<SensitivePath>();
        public List<string> Subdomains { get; set; } = new List<string>();
        public List<string> ApiVersions { get; set; } = new List<string>();
        public Dictionary<string, int> EntryPointsByType { get; set; } = new Dictionary<string, int>();
    }

    public class EndpointEntry
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string FullUrl { get; set; }
        public int RequestCount { get; set; }
        public List<string> Parameters { get; set; }
        public bool HasAuthentication { get; set; }
        public double AverageResponseTime { get; set; }
        public List<int> StatusCodes { get; set; }
        public List<string> ContentTypes { get; set; }
    }

    public class InputPoint
    {
        public string Name { get; set; }
        public InputPointType Type { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string SampleValue { get; set; }
    }

    public class Technology
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public int Confidence { get; set; }
        public string Category { get; set; }
    }

    public class SensitivePath
    {
        public string Path { get; set; }
        public string Pattern { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }
        public bool IsAccessible { get; set; }
    }

    public enum InputPointType
    {
        QueryParameter,
        BodyParameter,
        PathParameter,
        Header,
        Cookie
    }
}
