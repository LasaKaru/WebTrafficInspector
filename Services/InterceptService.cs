using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class InterceptService
    {
        private List<InterceptRule> _rules = new List<InterceptRule>();
        private bool _isInterceptEnabled = false;
        private InterceptMode _mode = InterceptMode.Off;

        public bool IsInterceptEnabled => _isInterceptEnabled;
        public InterceptMode Mode => _mode;

        public void EnableIntercept(InterceptMode mode = InterceptMode.RequestAndResponse)
        {
            _isInterceptEnabled = true;
            _mode = mode;
        }

        public void DisableIntercept()
        {
            _isInterceptEnabled = false;
            _mode = InterceptMode.Off;
        }

        public void AddRule(InterceptRule rule)
        {
            rule.Id = Guid.NewGuid().ToString();
            rule.CreatedAt = DateTime.Now;
            _rules.Add(rule);
        }

        public void RemoveRule(string ruleId)
        {
            _rules.RemoveAll(r => r.Id == ruleId);
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

        public List<InterceptRule> GetAllRules() => _rules.ToList();

        public InterceptResult ProcessRequest(TrafficEntry entry)
        {
            if (!_isInterceptEnabled || _mode == InterceptMode.ResponseOnly)
                return new InterceptResult { ShouldIntercept = false, OriginalEntry = entry };

            var matchingRules = _rules
                .Where(r => r.IsEnabled && r.Target == InterceptTarget.Request && MatchesRule(entry, r))
                .OrderBy(r => r.Priority)
                .ToList();

            if (!matchingRules.Any())
                return new InterceptResult { ShouldIntercept = false, OriginalEntry = entry };

            var modified = CloneEntry(entry);
            var appliedActions = new List<string>();

            foreach (var rule in matchingRules)
            {
                ApplyRuleActions(modified, rule);
                appliedActions.Add(rule.Name);
            }

            return new InterceptResult
            {
                ShouldIntercept = true,
                OriginalEntry = entry,
                ModifiedEntry = modified,
                AppliedRules = appliedActions,
                RequiresUserAction = matchingRules.Any(r => r.Action == InterceptAction.Pause)
            };
        }

        public InterceptResult ProcessResponse(TrafficEntry entry)
        {
            if (!_isInterceptEnabled || _mode == InterceptMode.RequestOnly)
                return new InterceptResult { ShouldIntercept = false, OriginalEntry = entry };

            var matchingRules = _rules
                .Where(r => r.IsEnabled && r.Target == InterceptTarget.Response && MatchesRule(entry, r))
                .OrderBy(r => r.Priority)
                .ToList();

            if (!matchingRules.Any())
                return new InterceptResult { ShouldIntercept = false, OriginalEntry = entry };

            var modified = CloneEntry(entry);
            var appliedActions = new List<string>();

            foreach (var rule in matchingRules)
            {
                ApplyRuleActions(modified, rule);
                appliedActions.Add(rule.Name);
            }

            return new InterceptResult
            {
                ShouldIntercept = true,
                OriginalEntry = entry,
                ModifiedEntry = modified,
                AppliedRules = appliedActions,
                RequiresUserAction = matchingRules.Any(r => r.Action == InterceptAction.Pause)
            };
        }

        private bool MatchesRule(TrafficEntry entry, InterceptRule rule)
        {
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
            if (!string.IsNullOrEmpty(rule.Method) && !entry.Method.Equals(rule.Method, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check host
            if (!string.IsNullOrEmpty(rule.Host) && !entry.Host.Contains(rule.Host, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check status code
            if (rule.StatusCode.HasValue && entry.StatusCode != rule.StatusCode.Value)
                return false;

            // Check content type
            if (!string.IsNullOrEmpty(rule.ContentType) &&
                (entry.ContentType == null || !entry.ContentType.Contains(rule.ContentType, StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        }

        private void ApplyRuleActions(TrafficEntry entry, InterceptRule rule)
        {
            switch (rule.Action)
            {
                case InterceptAction.ModifyHeaders:
                    if (!string.IsNullOrEmpty(rule.HeaderModifications))
                    {
                        if (rule.Target == InterceptTarget.Request)
                            entry.RawRequest = ApplyHeaderModifications(entry.RawRequest, rule.HeaderModifications);
                        else
                            entry.RawResponse = ApplyHeaderModifications(entry.RawResponse, rule.HeaderModifications);
                    }
                    break;

                case InterceptAction.ModifyBody:
                    if (!string.IsNullOrEmpty(rule.BodyModifications))
                    {
                        if (rule.Target == InterceptTarget.Request)
                            entry.RawRequest = ApplyBodyModifications(entry.RawRequest, rule.BodyModifications);
                        else
                            entry.RawResponse = ApplyBodyModifications(entry.RawResponse, rule.BodyModifications);
                    }
                    break;

                case InterceptAction.ReplaceResponse:
                    if (!string.IsNullOrEmpty(rule.ReplacementResponse))
                    {
                        entry.RawResponse = rule.ReplacementResponse;
                        entry.Status = rule.ReplacementStatusCode ?? entry.Status;
                    }
                    break;

                case InterceptAction.Drop:
                    entry.Tags = (entry.Tags ?? "") + ",DROPPED";
                    break;

                case InterceptAction.Delay:
                    if (rule.DelayMs.HasValue)
                    {
                        System.Threading.Thread.Sleep(rule.DelayMs.Value);
                    }
                    break;

                case InterceptAction.MatchReplace:
                    if (!string.IsNullOrEmpty(rule.FindPattern) && rule.ReplaceWith != null)
                    {
                        if (rule.Target == InterceptTarget.Request && entry.RawRequest != null)
                        {
                            if (rule.UseRegex)
                                entry.RawRequest = Regex.Replace(entry.RawRequest, rule.FindPattern, rule.ReplaceWith);
                            else
                                entry.RawRequest = entry.RawRequest.Replace(rule.FindPattern, rule.ReplaceWith);
                        }
                        else if (rule.Target == InterceptTarget.Response && entry.RawResponse != null)
                        {
                            if (rule.UseRegex)
                                entry.RawResponse = Regex.Replace(entry.RawResponse, rule.FindPattern, rule.ReplaceWith);
                            else
                                entry.RawResponse = entry.RawResponse.Replace(rule.FindPattern, rule.ReplaceWith);
                        }
                    }
                    break;
            }

            // Add rule tag
            if (rule.AddTag)
            {
                entry.Tags = string.IsNullOrEmpty(entry.Tags) ? rule.Name : $"{entry.Tags},{rule.Name}";
            }
        }

        private string ApplyHeaderModifications(string raw, string modifications)
        {
            if (string.IsNullOrEmpty(raw)) return raw;

            var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            var modLines = modifications.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var modLine in modLines)
            {
                if (modLine.StartsWith("-"))
                {
                    // Remove header
                    var headerName = modLine.Substring(1).Trim();
                    lines.RemoveAll(l => l.StartsWith(headerName + ":", StringComparison.OrdinalIgnoreCase));
                }
                else if (modLine.Contains(":"))
                {
                    // Add or replace header
                    var parts = modLine.Split(new[] { ':' }, 2);
                    var headerName = parts[0].Trim();
                    var headerValue = parts[1].Trim();

                    // Remove existing
                    lines.RemoveAll(l => l.StartsWith(headerName + ":", StringComparison.OrdinalIgnoreCase));

                    // Add new (after first line which is the request/status line)
                    if (lines.Count > 0)
                        lines.Insert(1, $"{headerName}: {headerValue}");
                }
            }

            return string.Join("\r\n", lines);
        }

        private string ApplyBodyModifications(string raw, string modifications)
        {
            if (string.IsNullOrEmpty(raw)) return raw;

            // Find body (after double line break)
            var headerEndIndex = raw.IndexOf("\r\n\r\n");
            if (headerEndIndex == -1)
                headerEndIndex = raw.IndexOf("\n\n");

            if (headerEndIndex == -1)
                return raw + "\r\n\r\n" + modifications;

            var headers = raw.Substring(0, headerEndIndex);
            return headers + "\r\n\r\n" + modifications;
        }

        private TrafficEntry CloneEntry(TrafficEntry entry)
        {
            return new TrafficEntry
            {
                Id = entry.Id,
                Timestamp = entry.Timestamp,
                Method = entry.Method,
                Host = entry.Host,
                Path = entry.Path,
                Status = entry.Status,
                ContentType = entry.ContentType,
                Length = entry.Length,
                Duration = entry.Duration,
                RawRequest = entry.RawRequest,
                RawResponse = entry.RawResponse,
                IsPinned = entry.IsPinned,
                Tags = entry.Tags,
                Notes = entry.Notes,
                Color = entry.Color
            };
        }

        public List<InterceptRule> GetBuiltInRules()
        {
            return new List<InterceptRule>
            {
                new InterceptRule
                {
                    Name = "Remove CORS Headers",
                    Description = "Remove CORS restriction headers",
                    Target = InterceptTarget.Response,
                    Action = InterceptAction.ModifyHeaders,
                    HeaderModifications = "-Access-Control-Allow-Origin\n-Access-Control-Allow-Methods\n-Access-Control-Allow-Headers",
                    Priority = 10
                },
                new InterceptRule
                {
                    Name = "Mock API Response",
                    Description = "Replace API response with mock data",
                    Target = InterceptTarget.Response,
                    UrlPattern = "/api/",
                    Action = InterceptAction.ReplaceResponse,
                    ReplacementResponse = "{\"status\":\"success\",\"data\":{\"message\":\"Mocked response\"}}",
                    ReplacementStatusCode = 200,
                    Priority = 20
                },
                new InterceptRule
                {
                    Name = "Simulate Slow Network",
                    Description = "Add 2 second delay to all requests",
                    Target = InterceptTarget.Request,
                    Action = InterceptAction.Delay,
                    DelayMs = 2000,
                    Priority = 5
                },
                new InterceptRule
                {
                    Name = "Remove Cache Headers",
                    Description = "Remove cache control headers",
                    Target = InterceptTarget.Response,
                    Action = InterceptAction.ModifyHeaders,
                    HeaderModifications = "-Cache-Control\n-ETag\n-Last-Modified\n-Expires",
                    Priority = 10
                },
                new InterceptRule
                {
                    Name = "Add Custom User-Agent",
                    Description = "Add custom User-Agent header",
                    Target = InterceptTarget.Request,
                    Action = InterceptAction.ModifyHeaders,
                    HeaderModifications = "User-Agent: WebTrafficInspector/1.0",
                    Priority = 10
                }
            };
        }
    }

    public class InterceptRule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public InterceptTarget Target { get; set; }
        public InterceptAction Action { get; set; }
        public int Priority { get; set; } = 10;
        public DateTime CreatedAt { get; set; }

        // Matching criteria
        public string UrlPattern { get; set; }
        public string Host { get; set; }
        public string Method { get; set; }
        public int? StatusCode { get; set; }
        public string ContentType { get; set; }
        public bool UseRegex { get; set; }

        // Action parameters
        public string HeaderModifications { get; set; }
        public string BodyModifications { get; set; }
        public string ReplacementResponse { get; set; }
        public int? ReplacementStatusCode { get; set; }
        public int? DelayMs { get; set; }
        public string FindPattern { get; set; }
        public string ReplaceWith { get; set; }
        public bool AddTag { get; set; }
    }

    public class InterceptResult
    {
        public bool ShouldIntercept { get; set; }
        public TrafficEntry OriginalEntry { get; set; }
        public TrafficEntry ModifiedEntry { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
        public bool RequiresUserAction { get; set; }
    }

    public enum InterceptMode
    {
        Off,
        RequestOnly,
        ResponseOnly,
        RequestAndResponse
    }

    public enum InterceptTarget
    {
        Request,
        Response,
        Both
    }

    public enum InterceptAction
    {
        Pause,
        ModifyHeaders,
        ModifyBody,
        ReplaceResponse,
        Drop,
        Delay,
        MatchReplace
    }
}
