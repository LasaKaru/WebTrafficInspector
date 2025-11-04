using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced pattern matching and data extraction service
    /// </summary>
    public class PatternMatcherService
    {
        private readonly Dictionary<string, string> _commonPatterns;

        public PatternMatcherService()
        {
            InitializeCommonPatterns();
        }

        /// <summary>
        /// Search for a pattern across all traffic entries
        /// </summary>
        public List<PatternMatch> SearchPattern(List<TrafficEntry> entries, string pattern, bool isRegex = false, bool caseSensitive = false)
        {
            var matches = new List<PatternMatch>();
            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            foreach (var entry in entries)
            {
                // Search in URL
                if (!string.IsNullOrEmpty(entry.Path))
                {
                    if (MatchesPattern(entry.Path, pattern, isRegex, options))
                    {
                        matches.Add(new PatternMatch
                        {
                            EntryId = entry.Id,
                            Location = "URL",
                            MatchedText = entry.Path,
                            Context = $"{entry.Method} {entry.Host}{entry.Path}"
                        });
                    }
                }

                // Search in Request
                if (!string.IsNullOrEmpty(entry.RawRequest))
                {
                    var requestMatches = FindMatches(entry.RawRequest, pattern, isRegex, options);
                    foreach (var match in requestMatches)
                    {
                        matches.Add(new PatternMatch
                        {
                            EntryId = entry.Id,
                            Location = "Request",
                            MatchedText = match,
                            Context = GetContext(entry.RawRequest, match, 50)
                        });
                    }
                }

                // Search in Response
                if (!string.IsNullOrEmpty(entry.RawResponse))
                {
                    var responseMatches = FindMatches(entry.RawResponse, pattern, isRegex, options);
                    foreach (var match in responseMatches)
                    {
                        matches.Add(new PatternMatch
                        {
                            EntryId = entry.Id,
                            Location = "Response",
                            MatchedText = match,
                            Context = GetContext(entry.RawResponse, match, 50)
                        });
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Extract all instances of a common pattern type (emails, IPs, URLs, etc.)
        /// </summary>
        public List<ExtractedData> ExtractCommonPattern(List<TrafficEntry> entries, string patternType)
        {
            if (!_commonPatterns.ContainsKey(patternType))
                return new List<ExtractedData>();

            var pattern = _commonPatterns[patternType];
            var results = new List<ExtractedData>();

            foreach (var entry in entries)
            {
                var text = $"{entry.RawRequest}\n{entry.RawResponse}";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    results.Add(new ExtractedData
                    {
                        Type = patternType,
                        Value = match.Value,
                        EntryId = entry.Id,
                        Url = $"{entry.Host}{entry.Path}"
                    });
                }
            }

            return results.DistinctBy(e => e.Value).ToList();
        }

        /// <summary>
        /// Extract parameters from all requests
        /// </summary>
        public List<ParameterInfo> ExtractParameters(List<TrafficEntry> entries)
        {
            var parameters = new List<ParameterInfo>();

            foreach (var entry in entries)
            {
                // Extract from URL query string
                if (entry.Path.Contains("?"))
                {
                    var queryString = entry.Path.Substring(entry.Path.IndexOf("?") + 1);
                    var pairs = queryString.Split('&');

                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split('=');
                        if (parts.Length >= 1)
                        {
                            parameters.Add(new ParameterInfo
                            {
                                Name = parts[0],
                                Value = parts.Length > 1 ? parts[1] : "",
                                Location = "Query String",
                                EntryId = entry.Id,
                                Url = $"{entry.Host}{entry.Path}"
                            });
                        }
                    }
                }

                // Extract from request body (POST parameters)
                if (entry.Method == "POST" && !string.IsNullOrEmpty(entry.RawRequest))
                {
                    var bodyParams = ExtractPostParameters(entry.RawRequest);
                    foreach (var param in bodyParams)
                    {
                        param.EntryId = entry.Id;
                        param.Url = $"{entry.Host}{entry.Path}";
                        parameters.Add(param);
                    }
                }

                // Extract from JSON bodies
                if (!string.IsNullOrEmpty(entry.RawRequest) && entry.RawRequest.Contains("{"))
                {
                    var jsonParams = ExtractJsonParameters(entry.RawRequest);
                    foreach (var param in jsonParams)
                    {
                        param.EntryId = entry.Id;
                        param.Url = $"{entry.Host}{entry.Path}";
                        param.Location = "JSON Body";
                        parameters.Add(param);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Find all unique endpoints/paths
        /// </summary>
        public List<EndpointInfo> ExtractEndpoints(List<TrafficEntry> entries)
        {
            var endpoints = entries
                .GroupBy(e => new { e.Method, e.Path })
                .Select(g => new EndpointInfo
                {
                    Method = g.Key.Method,
                    Path = g.Key.Path,
                    RequestCount = g.Count(),
                    UniqueStatuses = g.Select(e => e.Status).Distinct().ToList(),
                    FirstSeen = g.Min(e => e.Timestamp),
                    LastSeen = g.Max(e => e.Timestamp),
                    Hosts = g.Select(e => e.Host).Distinct().ToList()
                })
                .OrderBy(e => e.Path)
                .ToList();

            return endpoints;
        }

        /// <summary>
        /// Extract headers and their values
        /// </summary>
        public List<HeaderInfo> ExtractHeaders(List<TrafficEntry> entries, string headerType = "both")
        {
            var headers = new List<HeaderInfo>();

            foreach (var entry in entries)
            {
                if (headerType is "request" or "both" && !string.IsNullOrEmpty(entry.RawRequest))
                {
                    var requestHeaders = ParseHeaders(entry.RawRequest);
                    foreach (var header in requestHeaders)
                    {
                        headers.Add(new HeaderInfo
                        {
                            Name = header.Key,
                            Value = header.Value,
                            Type = "Request",
                            EntryId = entry.Id
                        });
                    }
                }

                if (headerType is "response" or "both" && !string.IsNullOrEmpty(entry.RawResponse))
                {
                    var responseHeaders = ParseHeaders(entry.RawResponse);
                    foreach (var header in responseHeaders)
                    {
                        headers.Add(new HeaderInfo
                        {
                            Name = header.Key,
                            Value = header.Value,
                            Type = "Response",
                            EntryId = entry.Id
                        });
                    }
                }
            }

            return headers;
        }

        /// <summary>
        /// Find all cookies
        /// </summary>
        public List<CookieInfo> ExtractCookies(List<TrafficEntry> entries)
        {
            var cookies = new List<CookieInfo>();

            foreach (var entry in entries)
            {
                // Extract from request headers
                if (!string.IsNullOrEmpty(entry.RawRequest))
                {
                    var cookieHeaders = Regex.Matches(entry.RawRequest, @"Cookie:\s*(.+)", RegexOptions.IgnoreCase);
                    foreach (Match match in cookieHeaders)
                    {
                        var cookieString = match.Groups[1].Value;
                        var cookiePairs = cookieString.Split(';');

                        foreach (var pair in cookiePairs)
                        {
                            var parts = pair.Trim().Split('=');
                            if (parts.Length >= 2)
                            {
                                cookies.Add(new CookieInfo
                                {
                                    Name = parts[0],
                                    Value = parts[1],
                                    Source = "Request",
                                    EntryId = entry.Id
                                });
                            }
                        }
                    }
                }

                // Extract from response headers
                if (!string.IsNullOrEmpty(entry.RawResponse))
                {
                    var setCookieHeaders = Regex.Matches(entry.RawResponse, @"Set-Cookie:\s*(.+)", RegexOptions.IgnoreCase);
                    foreach (Match match in setCookieHeaders)
                    {
                        var cookieString = match.Groups[1].Value;
                        var parts = cookieString.Split(';')[0].Split('=');

                        if (parts.Length >= 2)
                        {
                            var cookieInfo = new CookieInfo
                            {
                                Name = parts[0],
                                Value = parts[1],
                                Source = "Response (Set-Cookie)",
                                EntryId = entry.Id
                            };

                            // Parse attributes
                            if (cookieString.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase))
                                cookieInfo.Flags.Add("HttpOnly");
                            if (cookieString.Contains("Secure", StringComparison.OrdinalIgnoreCase))
                                cookieInfo.Flags.Add("Secure");
                            if (cookieString.Contains("SameSite", StringComparison.OrdinalIgnoreCase))
                                cookieInfo.Flags.Add("SameSite");

                            cookies.Add(cookieInfo);
                        }
                    }
                }
            }

            return cookies.DistinctBy(c => c.Name).ToList();
        }

        /// <summary>
        /// Create a parameter fuzzing wordlist
        /// </summary>
        public List<string> GenerateFuzzingWordlist(List<TrafficEntry> entries)
        {
            var wordlist = new HashSet<string>();

            // Extract all unique parameter names
            var parameters = ExtractParameters(entries);
            foreach (var param in parameters)
            {
                wordlist.Add(param.Name);
            }

            // Extract all unique path segments
            foreach (var entry in entries)
            {
                var segments = entry.Path.Split(new[] { '/', '?', '&', '=' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var segment in segments)
                {
                    if (!string.IsNullOrWhiteSpace(segment) && segment.Length > 2)
                    {
                        wordlist.Add(segment);
                    }
                }
            }

            return wordlist.OrderBy(w => w).ToList();
        }

        // Helper Methods
        private bool MatchesPattern(string text, string pattern, bool isRegex, RegexOptions options)
        {
            try
            {
                if (isRegex)
                {
                    return Regex.IsMatch(text, pattern, options);
                }
                else
                {
                    return text.IndexOf(pattern, options.HasFlag(RegexOptions.IgnoreCase) ?
                        StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<string> FindMatches(string text, string pattern, bool isRegex, RegexOptions options)
        {
            var results = new List<string>();

            try
            {
                if (isRegex)
                {
                    var matches = Regex.Matches(text, pattern, options);
                    results.AddRange(matches.Select(m => m.Value));
                }
                else
                {
                    var comparison = options.HasFlag(RegexOptions.IgnoreCase) ?
                        StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    int index = 0;
                    while ((index = text.IndexOf(pattern, index, comparison)) >= 0)
                    {
                        results.Add(pattern);
                        index += pattern.Length;
                    }
                }
            }
            catch { }

            return results;
        }

        private string GetContext(string text, string match, int contextLength)
        {
            var index = text.IndexOf(match, StringComparison.OrdinalIgnoreCase);
            if (index < 0) return match;

            var start = Math.Max(0, index - contextLength);
            var end = Math.Min(text.Length, index + match.Length + contextLength);
            var context = text.Substring(start, end - start);

            if (start > 0) context = "..." + context;
            if (end < text.Length) context = context + "...";

            return context;
        }

        private List<ParameterInfo> ExtractPostParameters(string rawRequest)
        {
            var parameters = new List<ParameterInfo>();
            var lines = rawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var bodyStart = Array.FindIndex(lines, string.IsNullOrWhiteSpace);

            if (bodyStart >= 0 && bodyStart < lines.Length - 1)
            {
                var body = string.Join("", lines.Skip(bodyStart + 1));
                var pairs = body.Split('&');

                foreach (var pair in pairs)
                {
                    var parts = pair.Split('=');
                    if (parts.Length >= 1)
                    {
                        parameters.Add(new ParameterInfo
                        {
                            Name = parts[0],
                            Value = parts.Length > 1 ? parts[1] : "",
                            Location = "POST Body"
                        });
                    }
                }
            }

            return parameters;
        }

        private List<ParameterInfo> ExtractJsonParameters(string text)
        {
            var parameters = new List<ParameterInfo>();

            // Simple JSON key extraction (not full parser)
            var jsonPattern = @"""(\w+)""\s*:\s*""?([^"",}\]]+)""?";
            var matches = Regex.Matches(text, jsonPattern);

            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    parameters.Add(new ParameterInfo
                    {
                        Name = match.Groups[1].Value,
                        Value = match.Groups[2].Value,
                        Location = "JSON"
                    });
                }
            }

            return parameters;
        }

        private Dictionary<string, string> ParseHeaders(string raw)
        {
            var headers = new Dictionary<string, string>();
            var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) break;

                var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    headers[parts[0]] = parts[1];
                }
            }

            return headers;
        }

        private void InitializeCommonPatterns()
        {
            _commonPatterns = new Dictionary<string, string>
            {
                ["Email"] = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
                ["IP Address"] = @"\b(?:\d{1,3}\.){3}\d{1,3}\b",
                ["URL"] = @"https?://[^\s<>""{}|\\^`\[\]]+",
                ["Phone"] = @"\b(?:\+?\d{1,3}[-.]?)?\(?\d{3}\)?[-.]?\d{3}[-.]?\d{4}\b",
                ["Credit Card"] = @"\b(?:\d{4}[-\s]?){3}\d{4}\b",
                ["SSN"] = @"\b\d{3}-\d{2}-\d{4}\b",
                ["API Key"] = @"\b[A-Za-z0-9_-]{32,}\b",
                ["UUID"] = @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
                ["JWT"] = @"\beyJ[A-Za-z0-9_-]*\.eyJ[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*\b",
                ["Hash (MD5)"] = @"\b[a-fA-F0-9]{32}\b",
                ["Hash (SHA1)"] = @"\b[a-fA-F0-9]{40}\b",
                ["Hash (SHA256)"] = @"\b[a-fA-F0-9]{64}\b"
            };
        }
    }

    public class PatternMatch
    {
        public int EntryId { get; set; }
        public string Location { get; set; }
        public string MatchedText { get; set; }
        public string Context { get; set; }
    }

    public class ExtractedData
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public int EntryId { get; set; }
        public string Url { get; set; }
    }

    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Location { get; set; }
        public int EntryId { get; set; }
        public string Url { get; set; }
    }

    public class EndpointInfo
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public int RequestCount { get; set; }
        public List<int> UniqueStatuses { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public List<string> Hosts { get; set; }
    }

    public class HeaderInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public int EntryId { get; set; }
    }

    public class CookieInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
        public int EntryId { get; set; }
        public List<string> Flags { get; set; } = new List<string>();
    }
}
