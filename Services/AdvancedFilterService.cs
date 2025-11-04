using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class AdvancedFilterService
    {
        public List<TrafficEntry> ApplyFilter(List<TrafficEntry> entries, FilterCriteria criteria)
        {
            var filtered = entries.AsEnumerable();

            // URL/Path filtering
            if (!string.IsNullOrEmpty(criteria.UrlPattern))
            {
                if (criteria.UseRegex)
                {
                    var regex = new Regex(criteria.UrlPattern, RegexOptions.IgnoreCase);
                    filtered = filtered.Where(e => regex.IsMatch($"{e.Host}{e.Path}"));
                }
                else
                {
                    filtered = filtered.Where(e => $"{e.Host}{e.Path}".Contains(criteria.UrlPattern, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Method filtering
            if (criteria.Methods != null && criteria.Methods.Any())
            {
                filtered = filtered.Where(e => criteria.Methods.Contains(e.Method, StringComparer.OrdinalIgnoreCase));
            }

            // Status code filtering
            if (criteria.StatusCodes != null && criteria.StatusCodes.Any())
            {
                filtered = filtered.Where(e => criteria.StatusCodes.Contains(e.StatusCode));
            }

            // Status code range filtering
            if (criteria.MinStatusCode.HasValue)
            {
                filtered = filtered.Where(e => e.StatusCode >= criteria.MinStatusCode.Value);
            }
            if (criteria.MaxStatusCode.HasValue)
            {
                filtered = filtered.Where(e => e.StatusCode <= criteria.MaxStatusCode.Value);
            }

            // Content type filtering
            if (!string.IsNullOrEmpty(criteria.ContentType))
            {
                filtered = filtered.Where(e => e.ContentType?.Contains(criteria.ContentType, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Request body filtering
            if (!string.IsNullOrEmpty(criteria.RequestBodyPattern))
            {
                if (criteria.UseRegex)
                {
                    var regex = new Regex(criteria.RequestBodyPattern, RegexOptions.IgnoreCase);
                    filtered = filtered.Where(e => !string.IsNullOrEmpty(e.RawRequest) && regex.IsMatch(e.RawRequest));
                }
                else
                {
                    filtered = filtered.Where(e => e.RawRequest?.Contains(criteria.RequestBodyPattern, StringComparison.OrdinalIgnoreCase) == true);
                }
            }

            // Response body filtering
            if (!string.IsNullOrEmpty(criteria.ResponseBodyPattern))
            {
                if (criteria.UseRegex)
                {
                    var regex = new Regex(criteria.ResponseBodyPattern, RegexOptions.IgnoreCase);
                    filtered = filtered.Where(e => !string.IsNullOrEmpty(e.RawResponse) && regex.IsMatch(e.RawResponse));
                }
                else
                {
                    filtered = filtered.Where(e => e.RawResponse?.Contains(criteria.ResponseBodyPattern, StringComparison.OrdinalIgnoreCase) == true);
                }
            }

            // Header filtering
            if (!string.IsNullOrEmpty(criteria.HeaderPattern))
            {
                if (criteria.UseRegex)
                {
                    var regex = new Regex(criteria.HeaderPattern, RegexOptions.IgnoreCase);
                    filtered = filtered.Where(e =>
                        (!string.IsNullOrEmpty(e.RawRequest) && regex.IsMatch(e.RawRequest)) ||
                        (!string.IsNullOrEmpty(e.RawResponse) && regex.IsMatch(e.RawResponse)));
                }
                else
                {
                    filtered = filtered.Where(e =>
                        e.RawRequest?.Contains(criteria.HeaderPattern, StringComparison.OrdinalIgnoreCase) == true ||
                        e.RawResponse?.Contains(criteria.HeaderPattern, StringComparison.OrdinalIgnoreCase) == true);
                }
            }

            // Size filtering
            if (criteria.MinSize.HasValue)
            {
                filtered = filtered.Where(e => e.Size >= criteria.MinSize.Value);
            }
            if (criteria.MaxSize.HasValue)
            {
                filtered = filtered.Where(e => e.Size <= criteria.MaxSize.Value);
            }

            // Time filtering
            if (criteria.StartTime.HasValue)
            {
                filtered = filtered.Where(e => e.Timestamp >= criteria.StartTime.Value);
            }
            if (criteria.EndTime.HasValue)
            {
                filtered = filtered.Where(e => e.Timestamp <= criteria.EndTime.Value);
            }

            // Duration filtering
            if (criteria.MinDuration.HasValue)
            {
                filtered = filtered.Where(e => e.Duration >= criteria.MinDuration.Value);
            }
            if (criteria.MaxDuration.HasValue)
            {
                filtered = filtered.Where(e => e.Duration <= criteria.MaxDuration.Value);
            }

            // Host filtering
            if (!string.IsNullOrEmpty(criteria.Host))
            {
                if (criteria.UseRegex)
                {
                    var regex = new Regex(criteria.Host, RegexOptions.IgnoreCase);
                    filtered = filtered.Where(e => regex.IsMatch(e.Host));
                }
                else
                {
                    filtered = filtered.Where(e => e.Host.Contains(criteria.Host, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Tags filtering
            if (!string.IsNullOrEmpty(criteria.Tags))
            {
                filtered = filtered.Where(e => e.Tags?.Contains(criteria.Tags, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Color filtering
            if (!string.IsNullOrEmpty(criteria.Color))
            {
                filtered = filtered.Where(e => e.Color?.Equals(criteria.Color, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Pinned filtering
            if (criteria.ShowOnlyPinned)
            {
                filtered = filtered.Where(e => e.IsPinned);
            }

            // SSL/TLS filtering
            if (criteria.SslOnly.HasValue)
            {
                filtered = filtered.Where(e => e.Url.StartsWith("https://") == criteria.SslOnly.Value);
            }

            // Error filtering
            if (criteria.ErrorsOnly)
            {
                filtered = filtered.Where(e => e.StatusCode >= 400);
            }

            // Success filtering
            if (criteria.SuccessOnly)
            {
                filtered = filtered.Where(e => e.StatusCode >= 200 && e.StatusCode < 300);
            }

            // Negation support
            if (criteria.InvertFilter)
            {
                var filteredSet = new HashSet<TrafficEntry>(filtered);
                filtered = entries.Where(e => !filteredSet.Contains(e));
            }

            return filtered.ToList();
        }

        public List<FilterPreset> GetBuiltInPresets()
        {
            return new List<FilterPreset>
            {
                new FilterPreset
                {
                    Name = "Errors Only",
                    Description = "Show only 4xx and 5xx status codes",
                    Criteria = new FilterCriteria { ErrorsOnly = true }
                },
                new FilterPreset
                {
                    Name = "Success Only",
                    Description = "Show only 2xx status codes",
                    Criteria = new FilterCriteria { SuccessOnly = true }
                },
                new FilterPreset
                {
                    Name = "HTTPS Only",
                    Description = "Show only SSL/TLS traffic",
                    Criteria = new FilterCriteria { SslOnly = true }
                },
                new FilterPreset
                {
                    Name = "JSON Responses",
                    Description = "Show only JSON responses",
                    Criteria = new FilterCriteria { ContentType = "application/json" }
                },
                new FilterPreset
                {
                    Name = "HTML Pages",
                    Description = "Show only HTML responses",
                    Criteria = new FilterCriteria { ContentType = "text/html" }
                },
                new FilterPreset
                {
                    Name = "API Endpoints",
                    Description = "Show only /api/* endpoints",
                    Criteria = new FilterCriteria { UrlPattern = "/api/", UseRegex = false }
                },
                new FilterPreset
                {
                    Name = "Large Responses",
                    Description = "Show responses larger than 100KB",
                    Criteria = new FilterCriteria { MinSize = 100 * 1024 }
                },
                new FilterPreset
                {
                    Name = "Slow Requests",
                    Description = "Show requests taking more than 1 second",
                    Criteria = new FilterCriteria { MinDuration = 1000 }
                },
                new FilterPreset
                {
                    Name = "Pinned Items",
                    Description = "Show only pinned entries",
                    Criteria = new FilterCriteria { ShowOnlyPinned = true }
                },
                new FilterPreset
                {
                    Name = "POST Requests",
                    Description = "Show only POST requests",
                    Criteria = new FilterCriteria { Methods = new List<string> { "POST" } }
                }
            };
        }

        public FilterStatistics GetFilterStatistics(List<TrafficEntry> original, List<TrafficEntry> filtered)
        {
            return new FilterStatistics
            {
                OriginalCount = original.Count,
                FilteredCount = filtered.Count,
                RemovedCount = original.Count - filtered.Count,
                FilterEfficiency = original.Count > 0 ? (double)filtered.Count / original.Count * 100 : 0,
                TotalSizeOriginal = original.Sum(e => e.Size),
                TotalSizeFiltered = filtered.Sum(e => e.Size),
                MethodDistribution = filtered.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count()),
                StatusDistribution = filtered.GroupBy(e => e.StatusCode).ToDictionary(g => g.Key, g => g.Count()),
                HostDistribution = filtered.GroupBy(e => e.Host).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public List<TrafficEntry> QuickFilter(List<TrafficEntry> entries, string quickFilterType)
        {
            return quickFilterType switch
            {
                "GET" => entries.Where(e => e.Method == "GET").ToList(),
                "POST" => entries.Where(e => e.Method == "POST").ToList(),
                "PUT" => entries.Where(e => e.Method == "PUT").ToList(),
                "DELETE" => entries.Where(e => e.Method == "DELETE").ToList(),
                "2xx" => entries.Where(e => e.StatusCode >= 200 && e.StatusCode < 300).ToList(),
                "3xx" => entries.Where(e => e.StatusCode >= 300 && e.StatusCode < 400).ToList(),
                "4xx" => entries.Where(e => e.StatusCode >= 400 && e.StatusCode < 500).ToList(),
                "5xx" => entries.Where(e => e.StatusCode >= 500 && e.StatusCode < 600).ToList(),
                "JSON" => entries.Where(e => e.ContentType?.Contains("json") == true).ToList(),
                "HTML" => entries.Where(e => e.ContentType?.Contains("html") == true).ToList(),
                "XML" => entries.Where(e => e.ContentType?.Contains("xml") == true).ToList(),
                "Images" => entries.Where(e => e.ContentType?.StartsWith("image/") == true).ToList(),
                "HTTPS" => entries.Where(e => e.Url.StartsWith("https://")).ToList(),
                "HTTP" => entries.Where(e => e.Url.StartsWith("http://") && !e.Url.StartsWith("https://")).ToList(),
                _ => entries
            };
        }
    }

    public class FilterCriteria
    {
        public string UrlPattern { get; set; }
        public string Host { get; set; }
        public List<string> Methods { get; set; }
        public List<int> StatusCodes { get; set; }
        public int? MinStatusCode { get; set; }
        public int? MaxStatusCode { get; set; }
        public string ContentType { get; set; }
        public string RequestBodyPattern { get; set; }
        public string ResponseBodyPattern { get; set; }
        public string HeaderPattern { get; set; }
        public long? MinSize { get; set; }
        public long? MaxSize { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public string Tags { get; set; }
        public string Color { get; set; }
        public bool ShowOnlyPinned { get; set; }
        public bool? SslOnly { get; set; }
        public bool ErrorsOnly { get; set; }
        public bool SuccessOnly { get; set; }
        public bool UseRegex { get; set; }
        public bool InvertFilter { get; set; }
    }

    public class FilterPreset
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public FilterCriteria Criteria { get; set; }
    }

    public class FilterStatistics
    {
        public int OriginalCount { get; set; }
        public int FilteredCount { get; set; }
        public int RemovedCount { get; set; }
        public double FilterEfficiency { get; set; }
        public long TotalSizeOriginal { get; set; }
        public long TotalSizeFiltered { get; set; }
        public Dictionary<string, int> MethodDistribution { get; set; }
        public Dictionary<int, int> StatusDistribution { get; set; }
        public Dictionary<string, int> HostDistribution { get; set; }
    }
}
