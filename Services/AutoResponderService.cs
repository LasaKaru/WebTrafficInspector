using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class AutoResponderService
    {
        private List<AutoResponderRule> _rules = new List<AutoResponderRule>();
        private bool _isEnabled = false;
        private Dictionary<string, string> _responseTemplates = new Dictionary<string, string>();

        public bool IsEnabled => _isEnabled;

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        public void AddRule(AutoResponderRule rule)
        {
            rule.Id = Guid.NewGuid().ToString();
            rule.CreatedAt = DateTime.Now;
            rule.HitCount = 0;
            _rules.Add(rule);
        }

        public void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.Id == ruleId);
        }

        public void UpdateRule(AutoResponderRule rule)
        {
            var existingRule = _rules.FirstOrDefault(r => r.Id == rule.Id);
            if (existingRule != null)
            {
                int index = _rules.IndexOf(existingRule);
                _rules[index] = rule;
            }
        }

        public void EnableRule(string ruleId)
        {
            var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null) rule.IsEnabled = true;
        }

        public void DisableRule(string ruleId)
        {
            var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null) rule.IsEnabled = false;
        }

        public void ReorderRules(List<string> ruleIds)
        {
            var reordered = new List<AutoResponderRule>();
            foreach (var id in ruleIds)
            {
                var rule = _rules.FirstOrDefault(r => r.Id == id);
                if (rule != null) reordered.Add(rule);
            }
            _rules = reordered;
        }

        public List<AutoResponderRule> GetAllRules() => _rules.ToList();

        public AutoResponderResult ProcessRequest(TrafficEntry entry)
        {
            if (!_isEnabled)
                return new AutoResponderResult { ShouldRespond = false };

            var matchingRule = _rules
                .Where(r => r.IsEnabled && MatchesRule(entry, r))
                .FirstOrDefault();

            if (matchingRule == null)
                return new AutoResponderResult { ShouldRespond = false };

            matchingRule.HitCount++;
            matchingRule.LastHit = DateTime.Now;

            var response = GenerateResponse(entry, matchingRule);

            return new AutoResponderResult
            {
                ShouldRespond = true,
                Rule = matchingRule,
                Response = response
            };
        }

        private bool MatchesRule(TrafficEntry entry, AutoResponderRule rule)
        {
            // Check if rule has hit limit
            if (rule.MaxHits.HasValue && rule.HitCount >= rule.MaxHits.Value)
                return false;

            // Check URL pattern
            if (!string.IsNullOrEmpty(rule.UrlPattern))
            {
                if (rule.UseRegex)
                {
                    if (!Regex.IsMatch(entry.Url, rule.UrlPattern, RegexOptions.IgnoreCase))
                        return false;
                }
                else
                {
                    if (!entry.Url.Contains(rule.UrlPattern, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            // Check method
            if (!string.IsNullOrEmpty(rule.Method) &&
                !entry.Method.Equals(rule.Method, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check host
            if (!string.IsNullOrEmpty(rule.Host) &&
                !entry.Host.Contains(rule.Host, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check path
            if (!string.IsNullOrEmpty(rule.Path))
            {
                if (rule.UseRegex)
                {
                    if (!Regex.IsMatch(entry.Path, rule.Path, RegexOptions.IgnoreCase))
                        return false;
                }
                else
                {
                    if (!entry.Path.Contains(rule.Path, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            // Check request contains
            if (!string.IsNullOrEmpty(rule.RequestContains) &&
                (string.IsNullOrEmpty(entry.RawRequest) ||
                 !entry.RawRequest.Contains(rule.RequestContains, StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        }

        private string GenerateResponse(TrafficEntry entry, AutoResponderRule rule)
        {
            string responseBody = "";

            switch (rule.ResponseType)
            {
                case ResponseType.Static:
                    responseBody = rule.ResponseBody ?? "";
                    break;

                case ResponseType.File:
                    if (!string.IsNullOrEmpty(rule.ResponseFilePath) && File.Exists(rule.ResponseFilePath))
                    {
                        responseBody = File.ReadAllText(rule.ResponseFilePath);
                    }
                    break;

                case ResponseType.Template:
                    if (_responseTemplates.ContainsKey(rule.TemplateName))
                    {
                        responseBody = ApplyTemplate(_responseTemplates[rule.TemplateName], entry);
                    }
                    break;

                case ResponseType.Mirror:
                    responseBody = entry.RawRequest ?? "";
                    break;

                case ResponseType.Empty:
                    responseBody = "";
                    break;

                case ResponseType.Redirect:
                    // Will be handled in headers
                    break;

                case ResponseType.Delay:
                    if (rule.DelayMs.HasValue)
                    {
                        System.Threading.Thread.Sleep(rule.DelayMs.Value);
                    }
                    responseBody = rule.ResponseBody ?? "";
                    break;
            }

            // Apply variable substitution
            if (rule.EnableVariableSubstitution)
            {
                responseBody = SubstituteVariables(responseBody, entry);
            }

            // Build full HTTP response
            var statusLine = $"HTTP/1.1 {rule.StatusCode} {GetStatusText(rule.StatusCode)}";
            var headers = BuildResponseHeaders(rule, responseBody.Length);

            return $"{statusLine}\r\n{headers}\r\n\r\n{responseBody}";
        }

        private string ApplyTemplate(string template, TrafficEntry entry)
        {
            return SubstituteVariables(template, entry);
        }

        private string SubstituteVariables(string text, TrafficEntry entry)
        {
            // Replace variables with actual values
            text = text.Replace("{{url}}", entry.Url);
            text = text.Replace("{{host}}", entry.Host);
            text = text.Replace("{{path}}", entry.Path);
            text = text.Replace("{{method}}", entry.Method);
            text = text.Replace("{{timestamp}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            text = text.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
            text = text.Replace("{{time}}", DateTime.Now.ToString("HH:mm:ss"));
            text = text.Replace("{{guid}}", Guid.NewGuid().ToString());
            text = text.Replace("{{random}}", new Random().Next(1000, 9999).ToString());

            // Extract query parameters and make them available
            if (entry.Path.Contains('?'))
            {
                var queryString = entry.Path.Split('?')[1];
                var parameters = queryString.Split('&');
                foreach (var param in parameters)
                {
                    var parts = param.Split('=');
                    if (parts.Length == 2)
                    {
                        text = text.Replace($"{{{{param.{parts[0]}}}}}", parts[1]);
                    }
                }
            }

            return text;
        }

        private string BuildResponseHeaders(AutoResponderRule rule, int contentLength)
        {
            var headers = new List<string>();

            // Content-Type
            if (!string.IsNullOrEmpty(rule.ContentType))
            {
                headers.Add($"Content-Type: {rule.ContentType}");
            }

            // Content-Length
            headers.Add($"Content-Length: {contentLength}");

            // CORS headers if enabled
            if (rule.EnableCORS)
            {
                headers.Add("Access-Control-Allow-Origin: *");
                headers.Add("Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS");
                headers.Add("Access-Control-Allow-Headers: Content-Type, Authorization");
            }

            // Custom headers
            if (!string.IsNullOrEmpty(rule.CustomHeaders))
            {
                var customHeaderLines = rule.CustomHeaders.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                headers.AddRange(customHeaderLines);
            }

            // Redirect location
            if (rule.ResponseType == ResponseType.Redirect && !string.IsNullOrEmpty(rule.RedirectUrl))
            {
                headers.Add($"Location: {rule.RedirectUrl}");
            }

            // Server header
            headers.Add("Server: WebTrafficInspector");
            headers.Add($"Date: {DateTime.UtcNow:R}");

            return string.Join("\r\n", headers);
        }

        private string GetStatusText(int statusCode)
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
                405 => "Method Not Allowed",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => "Unknown"
            };
        }

        public void AddTemplate(string name, string template)
        {
            _responseTemplates[name] = template;
        }

        public void RemoveTemplate(string name)
        {
            _responseTemplates.Remove(name);
        }

        public Dictionary<string, string> GetTemplates() => new Dictionary<string, string>(_responseTemplates);

        public void LoadBuiltInTemplates()
        {
            _responseTemplates["JSON Success"] = @"{
    ""status"": ""success"",
    ""message"": ""{{method}} request to {{path}} processed successfully"",
    ""timestamp"": ""{{timestamp}}"",
    ""data"": {
        ""id"": ""{{guid}}"",
        ""url"": ""{{url}}""
    }
}";

            _responseTemplates["JSON Error"] = @"{
    ""status"": ""error"",
    ""message"": ""An error occurred processing your request"",
    ""code"": ""ERR_{{random}}"",
    ""timestamp"": ""{{timestamp}}""
}";

            _responseTemplates["XML Success"] = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<response>
    <status>success</status>
    <timestamp>{{timestamp}}</timestamp>
    <data>
        <id>{{guid}}</id>
        <url>{{url}}</url>
    </data>
</response>";

            _responseTemplates["HTML Page"] = @"<!DOCTYPE html>
<html>
<head>
    <title>Mocked Response</title>
</head>
<body>
    <h1>Auto Responder</h1>
    <p>This is a mocked response for: {{url}}</p>
    <p>Generated at: {{timestamp}}</p>
</body>
</html>";

            _responseTemplates["Empty JSON"] = "{}";
            _responseTemplates["Empty Array"] = "[]";
        }

        public List<AutoResponderRule> GetBuiltInRules()
        {
            LoadBuiltInTemplates();

            return new List<AutoResponderRule>
            {
                new AutoResponderRule
                {
                    Name = "Mock API Success",
                    Description = "Return success response for API calls",
                    UrlPattern = "/api/",
                    ResponseType = ResponseType.Template,
                    TemplateName = "JSON Success",
                    StatusCode = 200,
                    ContentType = "application/json",
                    EnableVariableSubstitution = true
                },
                new AutoResponderRule
                {
                    Name = "Mock 404 Not Found",
                    Description = "Return 404 for specific paths",
                    Path = "/notfound",
                    ResponseType = ResponseType.Static,
                    ResponseBody = "Not Found",
                    StatusCode = 404,
                    ContentType = "text/plain"
                },
                new AutoResponderRule
                {
                    Name = "Enable CORS",
                    Description = "Add CORS headers to responses",
                    ResponseType = ResponseType.Static,
                    StatusCode = 200,
                    EnableCORS = true
                },
                new AutoResponderRule
                {
                    Name = "Redirect to HTTPS",
                    Description = "Redirect HTTP to HTTPS",
                    UrlPattern = "^http://",
                    UseRegex = true,
                    ResponseType = ResponseType.Redirect,
                    StatusCode = 301,
                    RedirectUrl = "https://{{host}}{{path}}"
                },
                new AutoResponderRule
                {
                    Name = "Slow Response (5s delay)",
                    Description = "Simulate slow API",
                    UrlPattern = "/slow",
                    ResponseType = ResponseType.Delay,
                    DelayMs = 5000,
                    ResponseBody = "{\"message\":\"Delayed response\"}",
                    StatusCode = 200,
                    ContentType = "application/json"
                }
            };
        }

        public AutoResponderStats GetStatistics()
        {
            return new AutoResponderStats
            {
                TotalRules = _rules.Count,
                EnabledRules = _rules.Count(r => r.IsEnabled),
                TotalHits = _rules.Sum(r => r.HitCount),
                RulesByType = _rules.GroupBy(r => r.ResponseType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                MostHitRule = _rules.OrderByDescending(r => r.HitCount).FirstOrDefault()?.Name,
                RecentlyHitRules = _rules
                    .Where(r => r.LastHit.HasValue)
                    .OrderByDescending(r => r.LastHit)
                    .Take(5)
                    .Select(r => r.Name)
                    .ToList()
            };
        }
    }

    public class AutoResponderRule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastHit { get; set; }
        public int HitCount { get; set; }
        public int? MaxHits { get; set; }

        // Matching criteria
        public string UrlPattern { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string RequestContains { get; set; }
        public bool UseRegex { get; set; }

        // Response configuration
        public ResponseType ResponseType { get; set; }
        public int StatusCode { get; set; } = 200;
        public string ContentType { get; set; } = "application/json";
        public string ResponseBody { get; set; }
        public string ResponseFilePath { get; set; }
        public string TemplateName { get; set; }
        public string CustomHeaders { get; set; }
        public bool EnableCORS { get; set; }
        public bool EnableVariableSubstitution { get; set; } = true;
        public int? DelayMs { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class AutoResponderResult
    {
        public bool ShouldRespond { get; set; }
        public AutoResponderRule Rule { get; set; }
        public string Response { get; set; }
    }

    public class AutoResponderStats
    {
        public int TotalRules { get; set; }
        public int EnabledRules { get; set; }
        public int TotalHits { get; set; }
        public Dictionary<string, int> RulesByType { get; set; }
        public string MostHitRule { get; set; }
        public List<string> RecentlyHitRules { get; set; }
    }

    public enum ResponseType
    {
        Static,
        File,
        Template,
        Mirror,
        Empty,
        Redirect,
        Delay
    }
}
