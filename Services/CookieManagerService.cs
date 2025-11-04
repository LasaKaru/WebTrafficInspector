using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class CookieManagerService
    {
        private Dictionary<string, List<Cookie>> _cookieJar = new Dictionary<string, List<Cookie>>();

        public void ExtractCookiesFromTraffic(List<TrafficEntry> entries)
        {
            _cookieJar.Clear();

            foreach (var entry in entries)
            {
                // Extract from requests
                if (!string.IsNullOrEmpty(entry.RawRequest))
                {
                    ExtractCookiesFromRequest(entry.RawRequest, entry.Host);
                }

                // Extract from responses (Set-Cookie headers)
                if (!string.IsNullOrEmpty(entry.RawResponse))
                {
                    ExtractCookiesFromResponse(entry.RawResponse, entry.Host);
                }
            }
        }

        private void ExtractCookiesFromRequest(string rawRequest, string host)
        {
            var lines = rawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.StartsWith("Cookie:", StringComparison.OrdinalIgnoreCase))
                {
                    var cookieValue = line.Substring(7).Trim();
                    var cookies = cookieValue.Split(';');

                    foreach (var cookie in cookies)
                    {
                        var parts = cookie.Trim().Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            AddCookie(host, new Cookie
                            {
                                Name = parts[0].Trim(),
                                Value = parts[1].Trim(),
                                Domain = host,
                                Source = "Request"
                            });
                        }
                    }
                }
            }
        }

        private void ExtractCookiesFromResponse(string rawResponse, string host)
        {
            var lines = rawResponse.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.StartsWith("Set-Cookie:", StringComparison.OrdinalIgnoreCase))
                {
                    var cookieValue = line.Substring(11).Trim();
                    var cookie = ParseSetCookie(cookieValue, host);
                    if (cookie != null)
                    {
                        AddCookie(host, cookie);
                    }
                }
            }
        }

        private Cookie ParseSetCookie(string setCookieValue, string defaultDomain)
        {
            var parts = setCookieValue.Split(';');
            if (parts.Length == 0) return null;

            var nameValue = parts[0].Split(new[] { '=' }, 2);
            if (nameValue.Length != 2) return null;

            var cookie = new Cookie
            {
                Name = nameValue[0].Trim(),
                Value = nameValue[1].Trim(),
                Domain = defaultDomain,
                Source = "Response (Set-Cookie)"
            };

            // Parse attributes
            for (int i = 1; i < parts.Length; i++)
            {
                var attr = parts[i].Trim();
                var attrParts = attr.Split(new[] { '=' }, 2);
                var attrName = attrParts[0].Trim().ToLower();

                switch (attrName)
                {
                    case "domain":
                        cookie.Domain = attrParts.Length > 1 ? attrParts[1].Trim() : defaultDomain;
                        break;
                    case "path":
                        cookie.Path = attrParts.Length > 1 ? attrParts[1].Trim() : "/";
                        break;
                    case "expires":
                        if (attrParts.Length > 1 && DateTime.TryParse(attrParts[1], out DateTime expires))
                            cookie.Expires = expires;
                        break;
                    case "max-age":
                        if (attrParts.Length > 1 && int.TryParse(attrParts[1], out int maxAge))
                            cookie.MaxAge = maxAge;
                        break;
                    case "secure":
                        cookie.Secure = true;
                        break;
                    case "httponly":
                        cookie.HttpOnly = true;
                        break;
                    case "samesite":
                        cookie.SameSite = attrParts.Length > 1 ? attrParts[1].Trim() : "Lax";
                        break;
                }
            }

            return cookie;
        }

        private void AddCookie(string host, Cookie cookie)
        {
            if (!_cookieJar.ContainsKey(host))
            {
                _cookieJar[host] = new List<Cookie>();
            }

            // Remove existing cookie with same name
            _cookieJar[host].RemoveAll(c => c.Name == cookie.Name);
            _cookieJar[host].Add(cookie);
        }

        public List<Cookie> GetAllCookies()
        {
            return _cookieJar.Values.SelectMany(cookies => cookies).ToList();
        }

        public List<Cookie> GetCookiesForHost(string host)
        {
            return _cookieJar.TryGetValue(host, out var cookies) ? cookies : new List<Cookie>();
        }

        public Dictionary<string, List<Cookie>> GetCookiesByHost()
        {
            return new Dictionary<string, List<Cookie>>(_cookieJar);
        }

        public void DeleteCookie(string host, string cookieName)
        {
            if (_cookieJar.TryGetValue(host, out var cookies))
            {
                cookies.RemoveAll(c => c.Name == cookieName);
            }
        }

        public void DeleteAllCookiesForHost(string host)
        {
            _cookieJar.Remove(host);
        }

        public void ClearAllCookies()
        {
            _cookieJar.Clear();
        }

        public void AddOrUpdateCookie(string host, Cookie cookie)
        {
            AddCookie(host, cookie);
        }

        public CookieAnalysis AnalyzeCookies()
        {
            var allCookies = GetAllCookies();

            return new CookieAnalysis
            {
                TotalCookies = allCookies.Count,
                UniqueCookieNames = allCookies.Select(c => c.Name).Distinct().Count(),
                UniqueHosts = _cookieJar.Keys.Count,
                SecureCookies = allCookies.Count(c => c.Secure),
                HttpOnlyCookies = allCookies.Count(c => c.HttpOnly),
                SessionCookies = allCookies.Count(c => !c.Expires.HasValue && !c.MaxAge.HasValue),
                PersistentCookies = allCookies.Count(c => c.Expires.HasValue || c.MaxAge.HasValue),
                ExpiredCookies = allCookies.Count(c => c.Expires.HasValue && c.Expires.Value < DateTime.Now),

                CookiesByHost = _cookieJar.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),

                SameSiteDistribution = allCookies
                    .Where(c => !string.IsNullOrEmpty(c.SameSite))
                    .GroupBy(c => c.SameSite)
                    .ToDictionary(g => g.Key, g => g.Count()),

                LargestCookies = allCookies
                    .OrderByDescending(c => c.Value.Length)
                    .Take(10)
                    .Select(c => new CookieInfoData { Name = c.Name, Domain = c.Domain, Size = c.Value.Length })
                    .ToList(),

                SecurityIssues = FindSecurityIssues(allCookies)
            };
        }

        private List<string> FindSecurityIssues(List<Cookie> cookies)
        {
            var issues = new List<string>();

            var nonSecureCookies = cookies.Count(c => !c.Secure);
            if (nonSecureCookies > 0)
            {
                issues.Add($"{nonSecureCookies} cookie(s) without Secure flag (vulnerable to MITM)");
            }

            var nonHttpOnlyCookies = cookies.Count(c => !c.HttpOnly);
            if (nonHttpOnlyCookies > 0)
            {
                issues.Add($"{nonHttpOnlyCookies} cookie(s) without HttpOnly flag (vulnerable to XSS)");
            }

            var noSameSiteCookies = cookies.Count(c => string.IsNullOrEmpty(c.SameSite));
            if (noSameSiteCookies > 0)
            {
                issues.Add($"{noSameSiteCookies} cookie(s) without SameSite attribute (vulnerable to CSRF)");
            }

            var sessionTokens = cookies.Where(c =>
                c.Name.Contains("session", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains("auth", StringComparison.OrdinalIgnoreCase));

            foreach (var token in sessionTokens)
            {
                if (!token.Secure)
                    issues.Add($"Session cookie '{token.Name}' is not secure");
                if (!token.HttpOnly)
                    issues.Add($"Session cookie '{token.Name}' is not HttpOnly");
            }

            return issues;
        }

        public string ExportCookies(CookieExportFormat format)
        {
            var allCookies = GetAllCookies();

            return format switch
            {
                CookieExportFormat.Json => ExportAsJson(allCookies),
                CookieExportFormat.Netscape => ExportAsNetscape(allCookies),
                CookieExportFormat.Text => ExportAsText(allCookies),
                _ => ""
            };
        }

        private string ExportAsJson(List<Cookie> cookies)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(cookies, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            return json;
        }

        private string ExportAsNetscape(List<Cookie> cookies)
        {
            var lines = new List<string>();
            lines.Add("# Netscape HTTP Cookie File");
            lines.Add("# This file was generated by WebTrafficInspector");
            lines.Add("");

            foreach (var cookie in cookies)
            {
                var domain = cookie.Domain.StartsWith(".") ? cookie.Domain : "." + cookie.Domain;
                var includeSubdomains = "TRUE";
                var path = cookie.Path ?? "/";
                var secure = cookie.Secure ? "TRUE" : "FALSE";
                var expiration = cookie.Expires?.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("F0") ?? "0";
                var name = cookie.Name;
                var value = cookie.Value;

                lines.Add($"{domain}\t{includeSubdomains}\t{path}\t{secure}\t{expiration}\t{name}\t{value}");
            }

            return string.Join("\n", lines);
        }

        private string ExportAsText(List<Cookie> cookies)
        {
            var lines = new List<string>();
            lines.Add("Cookie Export");
            lines.Add("=============");
            lines.Add("");

            foreach (var group in _cookieJar)
            {
                lines.Add($"Host: {group.Key}");
                lines.Add(new string('-', 50));

                foreach (var cookie in group.Value)
                {
                    lines.Add($"  Name:     {cookie.Name}");
                    lines.Add($"  Value:    {cookie.Value}");
                    if (!string.IsNullOrEmpty(cookie.Path))
                        lines.Add($"  Path:     {cookie.Path}");
                    if (cookie.Expires.HasValue)
                        lines.Add($"  Expires:  {cookie.Expires.Value}");
                    if (cookie.MaxAge.HasValue)
                        lines.Add($"  Max-Age:  {cookie.MaxAge.Value}");
                    if (cookie.Secure)
                        lines.Add($"  Secure:   Yes");
                    if (cookie.HttpOnly)
                        lines.Add($"  HttpOnly: Yes");
                    if (!string.IsNullOrEmpty(cookie.SameSite))
                        lines.Add($"  SameSite: {cookie.SameSite}");
                    lines.Add($"  Source:   {cookie.Source}");
                    lines.Add("");
                }
                lines.Add("");
            }

            return string.Join("\n", lines);
        }
    }

    public class Cookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime? Expires { get; set; }
        public int? MaxAge { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public string SameSite { get; set; }
        public string Source { get; set; }
    }

    public class CookieAnalysis
    {
        public int TotalCookies { get; set; }
        public int UniqueCookieNames { get; set; }
        public int UniqueHosts { get; set; }
        public int SecureCookies { get; set; }
        public int HttpOnlyCookies { get; set; }
        public int SessionCookies { get; set; }
        public int PersistentCookies { get; set; }
        public int ExpiredCookies { get; set; }
        public Dictionary<string, int> CookiesByHost { get; set; }
        public Dictionary<string, int> SameSiteDistribution { get; set; }
        public List<CookieInfoData> LargestCookies { get; set; }
        public List<string> SecurityIssues { get; set; }
    }

    public class CookieInfoData
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public int Size { get; set; }
    }

    public enum CookieExportFormat
    {
        Json,
        Netscape,
        Text
    }
}
