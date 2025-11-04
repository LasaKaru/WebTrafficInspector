using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Advanced comparison service for requests, responses, and sessions
    /// </summary>
    public class ComparisonService
    {
        /// <summary>
        /// Compare two traffic entries
        /// </summary>
        public EntryComparison CompareEntries(TrafficEntry entry1, TrafficEntry entry2)
        {
            var comparison = new EntryComparison
            {
                Entry1Id = entry1.Id,
                Entry2Id = entry2.Id,
                Entry1Url = $"{entry1.Host}{entry1.Path}",
                Entry2Url = $"{entry2.Host}{entry2.Path}"
            };

            // Compare basic properties
            comparison.MethodMatches = entry1.Method == entry2.Method;
            comparison.HostMatches = entry1.Host == entry2.Host;
            comparison.PathMatches = entry1.Path == entry2.Path;
            comparison.StatusMatches = entry1.Status == entry2.Status;

            // Compare requests
            comparison.RequestComparison = CompareText(entry1.RawRequest ?? "", entry2.RawRequest ?? "");

            // Compare responses
            comparison.ResponseComparison = CompareText(entry1.RawResponse ?? "", entry2.RawResponse ?? "");

            // Calculate similarity
            comparison.OverallSimilarity = CalculateSimilarity(entry1, entry2);

            return comparison;
        }

        /// <summary>
        /// Compare two text blocks and find differences
        /// </summary>
        public TextComparison CompareText(string text1, string text2)
        {
            var comparison = new TextComparison
            {
                Text1Length = text1.Length,
                Text2Length = text2.Length,
                LengthDifference = Math.Abs(text1.Length - text2.Length),
                AreIdentical = text1 == text2
            };

            if (comparison.AreIdentical)
            {
                comparison.SimilarityPercentage = 100;
                return comparison;
            }

            // Find line-by-line differences
            var lines1 = text1.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var lines2 = text2.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < Math.Max(lines1.Length, lines2.Length); i++)
            {
                var line1 = i < lines1.Length ? lines1[i] : "";
                var line2 = i < lines2.Length ? lines2[i] : "";

                if (line1 != line2)
                {
                    comparison.Differences.Add(new LineDifference
                    {
                        LineNumber = i + 1,
                        Text1 = line1,
                        Text2 = line2,
                        Type = DetermineChangeType(line1, line2)
                    });
                }
            }

            // Calculate similarity using Levenshtein-like metric
            comparison.SimilarityPercentage = CalculateTextSimilarity(text1, text2);

            return comparison;
        }

        /// <summary>
        /// Compare response times across multiple replays
        /// </summary>
        public TimeComparison CompareResponseTimes(List<ReplayResult> results)
        {
            if (results == null || results.Count == 0)
                return new TimeComparison();

            var durations = results.Where(r => r.Success).Select(r => r.Duration.TotalMilliseconds).ToList();

            return new TimeComparison
            {
                SampleCount = durations.Count,
                AverageMs = durations.Average(),
                MinMs = durations.Min(),
                MaxMs = durations.Max(),
                MedianMs = CalculateMedian(durations),
                StandardDeviation = CalculateStandardDeviation(durations)
            };
        }

        /// <summary>
        /// Compare two sessions
        /// </summary>
        public SessionComparison CompareSessions(List<TrafficEntry> session1, List<TrafficEntry> session2)
        {
            var comparison = new SessionComparison
            {
                Session1Count = session1.Count,
                Session2Count = session2.Count
            };

            // Find unique URLs in each session
            var urls1 = session1.Select(e => $"{e.Host}{e.Path}").ToHashSet();
            var urls2 = session2.Select(e => $"{e.Host}{e.Path}").ToHashSet();

            comparison.UniqueToSession1 = urls1.Except(urls2).ToList();
            comparison.UniqueToSession2 = urls2.Except(urls1).ToList();
            comparison.CommonUrls = urls1.Intersect(urls2).ToList();

            // Compare methods
            var methods1 = session1.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count());
            var methods2 = session2.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count());

            comparison.MethodDistribution1 = methods1;
            comparison.MethodDistribution2 = methods2;

            // Compare status codes
            var statuses1 = session1.GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count());
            var statuses2 = session2.GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count());

            comparison.StatusDistribution1 = statuses1;
            comparison.StatusDistribution2 = statuses2;

            // Time range comparison
            if (session1.Any())
            {
                comparison.Session1Start = session1.Min(e => e.Timestamp);
                comparison.Session1End = session1.Max(e => e.Timestamp);
                comparison.Session1Duration = comparison.Session1End - comparison.Session1Start;
            }

            if (session2.Any())
            {
                comparison.Session2Start = session2.Min(e => e.Timestamp);
                comparison.Session2End = session2.Max(e => e.Timestamp);
                comparison.Session2Duration = comparison.Session2End - comparison.Session2Start;
            }

            return comparison;
        }

        /// <summary>
        /// Generate a diff report
        /// </summary>
        public string GenerateDiffReport(TextComparison comparison)
        {
            var report = new StringBuilder();

            report.AppendLine("=== COMPARISON REPORT ===");
            report.AppendLine();
            report.AppendLine($"Similarity: {comparison.SimilarityPercentage:F2}%");
            report.AppendLine($"Text 1 Length: {comparison.Text1Length}");
            report.AppendLine($"Text 2 Length: {comparison.Text2Length}");
            report.AppendLine($"Length Difference: {comparison.LengthDifference}");
            report.AppendLine();

            if (comparison.AreIdentical)
            {
                report.AppendLine("✓ Texts are identical");
            }
            else
            {
                report.AppendLine($"Found {comparison.Differences.Count} differences:");
                report.AppendLine();

                foreach (var diff in comparison.Differences.Take(50)) // Limit output
                {
                    report.AppendLine($"Line {diff.LineNumber} ({diff.Type}):");
                    if (!string.IsNullOrEmpty(diff.Text1))
                    {
                        report.AppendLine($"  < {diff.Text1}");
                    }
                    if (!string.IsNullOrEmpty(diff.Text2))
                    {
                        report.AppendLine($"  > {diff.Text2}");
                    }
                    report.AppendLine();
                }

                if (comparison.Differences.Count > 50)
                {
                    report.AppendLine($"... and {comparison.Differences.Count - 50} more differences");
                }
            }

            return report.ToString();
        }

        /// <summary>
        /// Compare headers between two entries
        /// </summary>
        public HeaderComparison CompareHeaders(TrafficEntry entry1, TrafficEntry entry2)
        {
            var headers1 = ParseHeaders(entry1.RawRequest ?? "");
            var headers2 = ParseHeaders(entry2.RawRequest ?? "");

            var comparison = new HeaderComparison();

            var allHeaderNames = headers1.Keys.Union(headers2.Keys).ToList();

            foreach (var headerName in allHeaderNames)
            {
                var hasHeader1 = headers1.TryGetValue(headerName, out var value1);
                var hasHeader2 = headers2.TryGetValue(headerName, out var value2);

                if (hasHeader1 && hasHeader2)
                {
                    if (value1 == value2)
                    {
                        comparison.MatchingHeaders.Add(headerName, value1);
                    }
                    else
                    {
                        comparison.DifferentHeaders.Add(headerName, new[] { value1, value2 });
                    }
                }
                else if (hasHeader1)
                {
                    comparison.OnlyInFirst.Add(headerName, value1);
                }
                else
                {
                    comparison.OnlyInSecond.Add(headerName, value2);
                }
            }

            return comparison;
        }

        // Helper Methods
        private double CalculateSimilarity(TrafficEntry entry1, TrafficEntry entry2)
        {
            int score = 0;
            int maxScore = 0;

            // Method (10 points)
            maxScore += 10;
            if (entry1.Method == entry2.Method) score += 10;

            // Host (20 points)
            maxScore += 20;
            if (entry1.Host == entry2.Host) score += 20;

            // Path (20 points)
            maxScore += 20;
            if (entry1.Path == entry2.Path) score += 20;
            else if (PathsSimilar(entry1.Path, entry2.Path)) score += 10;

            // Status (10 points)
            maxScore += 10;
            if (entry1.Status == entry2.Status) score += 10;

            // Request similarity (20 points)
            maxScore += 20;
            var requestSimilarity = CalculateTextSimilarity(entry1.RawRequest ?? "", entry2.RawRequest ?? "");
            score += (int)(requestSimilarity * 20 / 100);

            // Response similarity (20 points)
            maxScore += 20;
            var responseSimilarity = CalculateTextSimilarity(entry1.RawResponse ?? "", entry2.RawResponse ?? "");
            score += (int)(responseSimilarity * 20 / 100);

            return (double)score / maxScore * 100;
        }

        private double CalculateTextSimilarity(string text1, string text2)
        {
            if (text1 == text2) return 100;
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2)) return 100;
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2)) return 0;

            var lines1 = text1.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var lines2 = text2.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            int matchingLines = 0;
            int totalLines = Math.Max(lines1.Length, lines2.Length);

            for (int i = 0; i < Math.Min(lines1.Length, lines2.Length); i++)
            {
                if (lines1[i] == lines2[i])
                    matchingLines++;
            }

            return (double)matchingLines / totalLines * 100;
        }

        private bool PathsSimilar(string path1, string path2)
        {
            // Remove query strings and compare
            var basePath1 = path1.Split('?')[0];
            var basePath2 = path2.Split('?')[0];

            var segments1 = basePath1.Split('/');
            var segments2 = basePath2.Split('/');

            if (segments1.Length != segments2.Length)
                return false;

            int matches = 0;
            for (int i = 0; i < segments1.Length; i++)
            {
                if (segments1[i] == segments2[i])
                    matches++;
            }

            return (double)matches / segments1.Length >= 0.7; // 70% similarity
        }

        private string DetermineChangeType(string line1, string line2)
        {
            if (string.IsNullOrEmpty(line1)) return "Added";
            if (string.IsNullOrEmpty(line2)) return "Removed";
            return "Modified";
        }

        private double CalculateMedian(List<double> values)
        {
            var sorted = values.OrderBy(v => v).ToList();
            int mid = sorted.Count / 2;

            if (sorted.Count % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2;
            else
                return sorted[mid];
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            var avg = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / values.Count);
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
    }

    public class EntryComparison
    {
        public int Entry1Id { get; set; }
        public int Entry2Id { get; set; }
        public string Entry1Url { get; set; }
        public string Entry2Url { get; set; }
        public bool MethodMatches { get; set; }
        public bool HostMatches { get; set; }
        public bool PathMatches { get; set; }
        public bool StatusMatches { get; set; }
        public TextComparison RequestComparison { get; set; }
        public TextComparison ResponseComparison { get; set; }
        public double OverallSimilarity { get; set; }
    }

    public class TextComparison
    {
        public int Text1Length { get; set; }
        public int Text2Length { get; set; }
        public int LengthDifference { get; set; }
        public bool AreIdentical { get; set; }
        public double SimilarityPercentage { get; set; }
        public List<LineDifference> Differences { get; set; } = new List<LineDifference>();
    }

    public class LineDifference
    {
        public int LineNumber { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Type { get; set; } // Added, Removed, Modified
    }

    public class TimeComparison
    {
        public int SampleCount { get; set; }
        public double AverageMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
        public double MedianMs { get; set; }
        public double StandardDeviation { get; set; }
    }

    public class SessionComparison
    {
        public int Session1Count { get; set; }
        public int Session2Count { get; set; }
        public List<string> UniqueToSession1 { get; set; }
        public List<string> UniqueToSession2 { get; set; }
        public List<string> CommonUrls { get; set; }
        public Dictionary<string, int> MethodDistribution1 { get; set; }
        public Dictionary<string, int> MethodDistribution2 { get; set; }
        public Dictionary<int, int> StatusDistribution1 { get; set; }
        public Dictionary<int, int> StatusDistribution2 { get; set; }
        public DateTime Session1Start { get; set; }
        public DateTime Session1End { get; set; }
        public TimeSpan Session1Duration { get; set; }
        public DateTime Session2Start { get; set; }
        public DateTime Session2End { get; set; }
        public TimeSpan Session2Duration { get; set; }
    }

    public class HeaderComparison
    {
        public Dictionary<string, string> MatchingHeaders { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string[]> DifferentHeaders { get; set; } = new Dictionary<string, string[]>();
        public Dictionary<string, string> OnlyInFirst { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> OnlyInSecond { get; set; } = new Dictionary<string, string>();
    }
}
