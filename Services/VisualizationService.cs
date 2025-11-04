using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class VisualizationService
    {
        public TrafficStatistics GenerateStatistics(List<TrafficEntry> entries)
        {
            if (!entries.Any())
                return new TrafficStatistics();

            return new TrafficStatistics
            {
                TotalRequests = entries.Count,
                TotalSize = entries.Sum(e => e.Size),
                AverageSize = entries.Average(e => e.Size),
                AverageDuration = entries.Average(e => e.Duration),
                TotalDuration = entries.Sum(e => e.Duration),
                MinDuration = entries.Min(e => e.Duration),
                MaxDuration = entries.Max(e => e.Duration),
                StartTime = entries.Min(e => e.Timestamp),
                EndTime = entries.Max(e => e.Timestamp),
                TimeSpan = entries.Max(e => e.Timestamp) - entries.Min(e => e.Timestamp),

                UniqueHosts = entries.Select(e => e.Host).Distinct().Count(),
                UniqueUrls = entries.Select(e => e.Url).Distinct().Count(),
                UniquePaths = entries.Select(e => e.Path).Distinct().Count(),

                MethodDistribution = entries.GroupBy(e => e.Method)
                    .ToDictionary(g => g.Key, g => g.Count()),

                StatusCodeDistribution = entries.GroupBy(e => e.StatusCode)
                    .ToDictionary(g => g.Key, g => g.Count()),

                HostDistribution = entries.GroupBy(e => e.Host)
                    .OrderByDescending(g => g.Count())
                    .Take(20)
                    .ToDictionary(g => g.Key, g => g.Count()),

                ContentTypeDistribution = entries
                    .Where(e => !string.IsNullOrEmpty(e.ContentType))
                    .GroupBy(e => e.ContentType.Split(';')[0].Trim())
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),

                SuccessCount = entries.Count(e => e.StatusCode >= 200 && e.StatusCode < 300),
                RedirectCount = entries.Count(e => e.StatusCode >= 300 && e.StatusCode < 400),
                ClientErrorCount = entries.Count(e => e.StatusCode >= 400 && e.StatusCode < 500),
                ServerErrorCount = entries.Count(e => e.StatusCode >= 500),

                HttpsCount = entries.Count(e => e.Url.StartsWith("https://")),
                HttpCount = entries.Count(e => e.Url.StartsWith("http://") && !e.Url.StartsWith("https://"))
            };
        }

        public TimelineData GenerateTimeline(List<TrafficEntry> entries, TimeSpan interval)
        {
            if (!entries.Any())
                return new TimelineData();

            var minTime = entries.Min(e => e.Timestamp);
            var maxTime = entries.Max(e => e.Timestamp);
            var buckets = new Dictionary<DateTime, TimelineBucket>();

            // Create buckets
            for (var time = minTime; time <= maxTime; time += interval)
            {
                buckets[time] = new TimelineBucket
                {
                    Timestamp = time,
                    RequestCount = 0,
                    TotalSize = 0,
                    TotalDuration = 0,
                    ErrorCount = 0
                };
            }

            // Fill buckets
            foreach (var entry in entries)
            {
                var bucketTime = new DateTime(
                    ((entry.Timestamp.Ticks / interval.Ticks) * interval.Ticks),
                    entry.Timestamp.Kind);

                if (buckets.TryGetValue(bucketTime, out var bucket))
                {
                    bucket.RequestCount++;
                    bucket.TotalSize += entry.Size;
                    bucket.TotalDuration += entry.Duration;
                    if (entry.StatusCode >= 400)
                        bucket.ErrorCount++;
                }
            }

            return new TimelineData
            {
                Buckets = buckets.Values.OrderBy(b => b.Timestamp).ToList(),
                Interval = interval,
                StartTime = minTime,
                EndTime = maxTime
            };
        }

        public SizeDistributionData GenerateSizeDistribution(List<TrafficEntry> entries)
        {
            var ranges = new Dictionary<string, int>
            {
                ["0-1KB"] = 0,
                ["1-10KB"] = 0,
                ["10-100KB"] = 0,
                ["100KB-1MB"] = 0,
                ["1MB-10MB"] = 0,
                ["10MB+"] = 0
            };

            foreach (var entry in entries)
            {
                if (entry.Size < 1024)
                    ranges["0-1KB"]++;
                else if (entry.Size < 10 * 1024)
                    ranges["1-10KB"]++;
                else if (entry.Size < 100 * 1024)
                    ranges["10-100KB"]++;
                else if (entry.Size < 1024 * 1024)
                    ranges["100KB-1MB"]++;
                else if (entry.Size < 10 * 1024 * 1024)
                    ranges["1MB-10MB"]++;
                else
                    ranges["10MB+"]++;
            }

            return new SizeDistributionData
            {
                Ranges = ranges,
                TotalSize = entries.Sum(e => e.Size),
                AverageSize = entries.Any() ? entries.Average(e => e.Size) : 0,
                MedianSize = CalculateMedian(entries.Select(e => (double)e.Size).ToList())
            };
        }

        public DurationDistributionData GenerateDurationDistribution(List<TrafficEntry> entries)
        {
            var ranges = new Dictionary<string, int>
            {
                ["0-100ms"] = 0,
                ["100-500ms"] = 0,
                ["500ms-1s"] = 0,
                ["1-5s"] = 0,
                ["5-10s"] = 0,
                ["10s+"] = 0
            };

            foreach (var entry in entries)
            {
                if (entry.Duration < 100)
                    ranges["0-100ms"]++;
                else if (entry.Duration < 500)
                    ranges["100-500ms"]++;
                else if (entry.Duration < 1000)
                    ranges["500ms-1s"]++;
                else if (entry.Duration < 5000)
                    ranges["1-5s"]++;
                else if (entry.Duration < 10000)
                    ranges["5-10s"]++;
                else
                    ranges["10s+"]++;
            }

            return new DurationDistributionData
            {
                Ranges = ranges,
                AverageDuration = entries.Any() ? entries.Average(e => e.Duration) : 0,
                MedianDuration = CalculateMedian(entries.Select(e => (double)e.Duration).ToList()),
                MinDuration = entries.Any() ? entries.Min(e => e.Duration) : 0,
                MaxDuration = entries.Any() ? entries.Max(e => e.Duration) : 0
            };
        }

        public string GenerateAsciiChart(Dictionary<string, int> data, int maxWidth = 50)
        {
            if (!data.Any())
                return "No data available";

            var sb = new StringBuilder();
            var maxValue = data.Values.Max();
            var maxLabelLength = data.Keys.Max(k => k.Length);

            foreach (var kvp in data.OrderByDescending(x => x.Value))
            {
                var barLength = (int)((double)kvp.Value / maxValue * maxWidth);
                var bar = new string('█', barLength);
                var label = kvp.Key.PadRight(maxLabelLength);
                sb.AppendLine($"{label} │{bar} {kvp.Value}");
            }

            return sb.ToString();
        }

        public string GenerateTextReport(TrafficStatistics stats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("           TRAFFIC ANALYSIS REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine("OVERVIEW:");
            sb.AppendLine($"  Total Requests:        {stats.TotalRequests:N0}");
            sb.AppendLine($"  Unique Hosts:          {stats.UniqueHosts:N0}");
            sb.AppendLine($"  Unique URLs:           {stats.UniqueUrls:N0}");
            sb.AppendLine($"  Time Span:             {stats.TimeSpan.TotalMinutes:F1} minutes");
            sb.AppendLine();

            sb.AppendLine("SIZE STATISTICS:");
            sb.AppendLine($"  Total Size:            {FormatSize(stats.TotalSize)}");
            sb.AppendLine($"  Average Size:          {FormatSize((long)stats.AverageSize)}");
            sb.AppendLine();

            sb.AppendLine("PERFORMANCE:");
            sb.AppendLine($"  Average Duration:      {stats.AverageDuration:F0}ms");
            sb.AppendLine($"  Min Duration:          {stats.MinDuration:F0}ms");
            sb.AppendLine($"  Max Duration:          {stats.MaxDuration:F0}ms");
            sb.AppendLine($"  Total Duration:        {stats.TotalDuration / 1000:F1}s");
            sb.AppendLine();

            sb.AppendLine("STATUS CODES:");
            sb.AppendLine($"  Success (2xx):         {stats.SuccessCount} ({GetPercentage(stats.SuccessCount, stats.TotalRequests)}%)");
            sb.AppendLine($"  Redirects (3xx):       {stats.RedirectCount} ({GetPercentage(stats.RedirectCount, stats.TotalRequests)}%)");
            sb.AppendLine($"  Client Errors (4xx):   {stats.ClientErrorCount} ({GetPercentage(stats.ClientErrorCount, stats.TotalRequests)}%)");
            sb.AppendLine($"  Server Errors (5xx):   {stats.ServerErrorCount} ({GetPercentage(stats.ServerErrorCount, stats.TotalRequests)}%)");
            sb.AppendLine();

            sb.AppendLine("PROTOCOL:");
            sb.AppendLine($"  HTTPS:                 {stats.HttpsCount} ({GetPercentage(stats.HttpsCount, stats.TotalRequests)}%)");
            sb.AppendLine($"  HTTP:                  {stats.HttpCount} ({GetPercentage(stats.HttpCount, stats.TotalRequests)}%)");
            sb.AppendLine();

            sb.AppendLine("METHOD DISTRIBUTION:");
            foreach (var method in stats.MethodDistribution.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {method.Key.PadRight(8)} {method.Value,6:N0} ({GetPercentage(method.Value, stats.TotalRequests)}%)");
            }
            sb.AppendLine();

            sb.AppendLine("TOP HOSTS:");
            foreach (var host in stats.HostDistribution.Take(10))
            {
                sb.AppendLine($"  {host.Value,6:N0}  {host.Key}");
            }
            sb.AppendLine();

            sb.AppendLine("TOP CONTENT TYPES:");
            foreach (var ct in stats.ContentTypeDistribution.Take(5))
            {
                sb.AppendLine($"  {ct.Value,6:N0}  {ct.Key}");
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");

            return sb.ToString();
        }

        public EndpointAnalysis AnalyzeEndpoints(List<TrafficEntry> entries)
        {
            var endpoints = entries
                .GroupBy(e => new { e.Method, e.Path })
                .Select(g => new EndpointInfo
                {
                    Method = g.Key.Method,
                    Path = g.Key.Path,
                    RequestCount = g.Count(),
                    AverageDuration = g.Average(e => e.Duration),
                    MinDuration = g.Min(e => e.Duration),
                    MaxDuration = g.Max(e => e.Duration),
                    AverageSize = g.Average(e => e.Size),
                    ErrorRate = (double)g.Count(e => e.StatusCode >= 400) / g.Count() * 100,
                    StatusCodes = g.GroupBy(e => e.StatusCode).ToDictionary(sg => sg.Key, sg => sg.Count()),
                    FirstSeen = g.Min(e => e.Timestamp),
                    LastSeen = g.Max(e => e.Timestamp)
                })
                .OrderByDescending(e => e.RequestCount)
                .ToList();

            return new EndpointAnalysis
            {
                Endpoints = endpoints,
                TotalEndpoints = endpoints.Count,
                HighTrafficEndpoints = endpoints.Where(e => e.RequestCount > 10).ToList(),
                SlowEndpoints = endpoints.Where(e => e.AverageDuration > 1000).ToList(),
                ErrorProneEndpoints = endpoints.Where(e => e.ErrorRate > 10).ToList()
            };
        }

        private double CalculateMedian(List<double> values)
        {
            if (!values.Any()) return 0;

            var sorted = values.OrderBy(x => x).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            else
                return sorted[mid];
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:F2} {sizes[order]}";
        }

        private double GetPercentage(int value, int total)
        {
            return total > 0 ? (double)value / total * 100 : 0;
        }
    }

    public class TrafficStatistics
    {
        public int TotalRequests { get; set; }
        public long TotalSize { get; set; }
        public double AverageSize { get; set; }
        public double AverageDuration { get; set; }
        public double TotalDuration { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeSpan { get; set; }

        public int UniqueHosts { get; set; }
        public int UniqueUrls { get; set; }
        public int UniquePaths { get; set; }

        public Dictionary<string, int> MethodDistribution { get; set; }
        public Dictionary<int, int> StatusCodeDistribution { get; set; }
        public Dictionary<string, int> HostDistribution { get; set; }
        public Dictionary<string, int> ContentTypeDistribution { get; set; }

        public int SuccessCount { get; set; }
        public int RedirectCount { get; set; }
        public int ClientErrorCount { get; set; }
        public int ServerErrorCount { get; set; }

        public int HttpsCount { get; set; }
        public int HttpCount { get; set; }
    }

    public class TimelineData
    {
        public List<TimelineBucket> Buckets { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class TimelineBucket
    {
        public DateTime Timestamp { get; set; }
        public int RequestCount { get; set; }
        public long TotalSize { get; set; }
        public double TotalDuration { get; set; }
        public int ErrorCount { get; set; }
    }

    public class SizeDistributionData
    {
        public Dictionary<string, int> Ranges { get; set; }
        public long TotalSize { get; set; }
        public double AverageSize { get; set; }
        public double MedianSize { get; set; }
    }

    public class DurationDistributionData
    {
        public Dictionary<string, int> Ranges { get; set; }
        public double AverageDuration { get; set; }
        public double MedianDuration { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
    }

    public class EndpointInfo
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public int RequestCount { get; set; }
        public double AverageDuration { get; set; }
        public double MinDuration { get; set; }
        public double MaxDuration { get; set; }
        public double AverageSize { get; set; }
        public double ErrorRate { get; set; }
        public Dictionary<int, int> StatusCodes { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class EndpointAnalysis
    {
        public List<EndpointInfo> Endpoints { get; set; }
        public int TotalEndpoints { get; set; }
        public List<EndpointInfo> HighTrafficEndpoints { get; set; }
        public List<EndpointInfo> SlowEndpoints { get; set; }
        public List<EndpointInfo> ErrorProneEndpoints { get; set; }
    }
}
