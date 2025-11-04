using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class AutoAttackModeService
    {
        private bool _isEnabled;
        private string _targetUrl;
        private string _targetDomain;
        private List<string> _excludedExtensions;
        private List<string> _excludedPatterns;

        private XSSScannerService _xssScanner;
        private SQLInjectionScannerService _sqlScanner;
        private IDORScannerService _idorScanner;

        private List<AttackResult> _allResults = new List<AttackResult>();
        private Dictionary<string, List<AttackResult>> _resultsByUrl = new Dictionary<string, List<AttackResult>>();

        public event EventHandler<AttackResultEventArgs> AttackStarted;
        public event EventHandler<AttackResultEventArgs> AttackCompleted;
        public event EventHandler<AttackProgressEventArgs> AttackProgress;
        public event EventHandler<AttackSummaryEventArgs> ScanCompleted;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!_isEnabled)
                {
                    // Clear when disabled
                    _targetUrl = null;
                    _targetDomain = null;
                }
            }
        }

        public string TargetUrl
        {
            get => _targetUrl;
            set
            {
                _targetUrl = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _targetDomain = ExtractDomain(value);
                }
            }
        }

        public AutoAttackOptions Options { get; set; }

        public AutoAttackModeService()
        {
            _xssScanner = new XSSScannerService();
            _sqlScanner = new SQLInjectionScannerService();
            _idorScanner = new IDORScannerService();

            // Default excluded extensions (static assets)
            _excludedExtensions = new List<string>
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico", ".svg",
                ".css", ".js", ".woff", ".woff2", ".ttf", ".eot",
                ".mp4", ".mp3", ".avi", ".mov", ".wmv",
                ".pdf", ".zip", ".rar", ".tar", ".gz",
                ".xml", ".json", ".txt", ".csv"
            };

            // Default excluded patterns
            _excludedPatterns = new List<string>
            {
                @"cdn\.",
                @"static\.",
                @"assets\.",
                @"fonts\.",
                @"images\.",
                @"google-analytics",
                @"googleapis",
                @"facebook\.net",
                @"twitter\.com",
                @"linkedin\.com"
            };

            Options = new AutoAttackOptions();
        }

        public void LoadXSSPayloadsFromFile(string filePath)
        {
            _xssScanner.LoadPayloadsFromFile(filePath);
        }

        public async Task<bool> ProcessTrafficEntry(TrafficEntry entry)
        {
            if (!_isEnabled) return false;
            if (entry == null) return false;

            // Check if URL should be attacked
            if (!ShouldAttackUrl(entry.Url))
            {
                return false;
            }

            // Start attack in background
            _ = Task.Run(async () =>
            {
                await AttackUrl(entry);
            });

            return true;
        }

        public async Task<AutoAttackReport> AttackUrl(TrafficEntry entry)
        {
            var report = new AutoAttackReport
            {
                TargetUrl = entry.Url,
                StartTime = DateTime.Now,
                AttackResults = new List<AttackResult>()
            };

            try
            {
                OnAttackStarted(new AttackResultEventArgs
                {
                    Url = entry.Url,
                    AttackType = "All",
                    Status = "Started"
                });

                // Run all attacks in parallel if enabled
                var tasks = new List<Task<AttackResult>>();

                if (Options.EnableXSSScanning)
                {
                    tasks.Add(RunXSSAttack(entry));
                }

                if (Options.EnableSQLInjectionScanning)
                {
                    tasks.Add(RunSQLInjectionAttack(entry));
                }

                if (Options.EnableIDORScanning)
                {
                    tasks.Add(RunIDORAttack(entry));
                }

                // Wait for all attacks to complete
                var results = await Task.WhenAll(tasks);

                report.AttackResults.AddRange(results);
                _allResults.AddRange(results);

                // Store results by URL
                if (!_resultsByUrl.ContainsKey(entry.Url))
                {
                    _resultsByUrl[entry.Url] = new List<AttackResult>();
                }
                _resultsByUrl[entry.Url].AddRange(results);

                report.EndTime = DateTime.Now;
                report.Duration = (report.EndTime - report.StartTime).TotalSeconds;
                report.TotalVulnerabilities = results.Sum(r => r.VulnerabilitiesFound);
                report.TotalTests = results.Sum(r => r.TotalTests);

                OnScanCompleted(new AttackSummaryEventArgs
                {
                    Url = entry.Url,
                    TotalAttacks = results.Length,
                    TotalVulnerabilities = report.TotalVulnerabilities,
                    Duration = report.Duration
                });
            }
            catch (Exception ex)
            {
                report.Error = ex.Message;
                report.EndTime = DateTime.Now;
            }

            return report;
        }

        private async Task<AttackResult> RunXSSAttack(TrafficEntry entry)
        {
            try
            {
                OnAttackProgress(new AttackProgressEventArgs
                {
                    Url = entry.Url,
                    AttackType = "XSS",
                    Status = "Scanning...",
                    Progress = 0
                });

                var scanOptions = new XSSScanOptions
                {
                    TestHeaders = Options.TestHeaders,
                    TestCookies = Options.TestCookies,
                    DelayBetweenRequests = Options.DelayBetweenRequests
                };

                var scanReport = await _xssScanner.ScanTrafficEntry(entry, scanOptions);

                var result = new AttackResult
                {
                    AttackType = "XSS",
                    TargetUrl = entry.Url,
                    Status = scanReport.Status,
                    VulnerabilitiesFound = scanReport.VulnerabilitiesFound,
                    TotalTests = scanReport.TotalTests,
                    Duration = scanReport.Duration,
                    Details = scanReport,
                    Timestamp = DateTime.Now
                };

                OnAttackCompleted(new AttackResultEventArgs
                {
                    Url = entry.Url,
                    AttackType = "XSS",
                    Status = result.Status,
                    VulnerabilitiesFound = result.VulnerabilitiesFound
                });

                return result;
            }
            catch (Exception ex)
            {
                return new AttackResult
                {
                    AttackType = "XSS",
                    TargetUrl = entry.Url,
                    Status = $"Error: {ex.Message}",
                    VulnerabilitiesFound = 0,
                    TotalTests = 0,
                    Timestamp = DateTime.Now
                };
            }
        }

        private async Task<AttackResult> RunSQLInjectionAttack(TrafficEntry entry)
        {
            try
            {
                OnAttackProgress(new AttackProgressEventArgs
                {
                    Url = entry.Url,
                    AttackType = "SQL Injection",
                    Status = "Scanning...",
                    Progress = 0
                });

                var scanOptions = new SQLIScanOptions
                {
                    TestAllTypes = true,
                    DelayBetweenRequests = Options.DelayBetweenRequests
                };

                var scanReport = await _sqlScanner.ScanTrafficEntry(entry, scanOptions);

                var result = new AttackResult
                {
                    AttackType = "SQL Injection",
                    TargetUrl = entry.Url,
                    Status = scanReport.Status,
                    VulnerabilitiesFound = scanReport.VulnerabilitiesFound,
                    TotalTests = scanReport.TotalTests,
                    Duration = scanReport.Duration,
                    Details = scanReport,
                    Timestamp = DateTime.Now
                };

                OnAttackCompleted(new AttackResultEventArgs
                {
                    Url = entry.Url,
                    AttackType = "SQL Injection",
                    Status = result.Status,
                    VulnerabilitiesFound = result.VulnerabilitiesFound
                });

                return result;
            }
            catch (Exception ex)
            {
                return new AttackResult
                {
                    AttackType = "SQL Injection",
                    TargetUrl = entry.Url,
                    Status = $"Error: {ex.Message}",
                    VulnerabilitiesFound = 0,
                    TotalTests = 0,
                    Timestamp = DateTime.Now
                };
            }
        }

        private async Task<AttackResult> RunIDORAttack(TrafficEntry entry)
        {
            try
            {
                OnAttackProgress(new AttackProgressEventArgs
                {
                    Url = entry.Url,
                    AttackType = "IDOR",
                    Status = "Scanning...",
                    Progress = 0
                });

                var scanOptions = new IDORScanOptions
                {
                    SequentialRange = 5,
                    TestNegativeIDs = true,
                    DelayBetweenRequests = Options.DelayBetweenRequests
                };

                var scanReport = await _idorScanner.ScanTrafficEntry(entry, scanOptions);

                var result = new AttackResult
                {
                    AttackType = "IDOR",
                    TargetUrl = entry.Url,
                    Status = scanReport.Status,
                    VulnerabilitiesFound = scanReport.VulnerabilitiesFound,
                    TotalTests = scanReport.TotalTests,
                    Duration = scanReport.Duration,
                    Details = scanReport,
                    Timestamp = DateTime.Now
                };

                OnAttackCompleted(new AttackResultEventArgs
                {
                    Url = entry.Url,
                    AttackType = "IDOR",
                    Status = result.Status,
                    VulnerabilitiesFound = result.VulnerabilitiesFound
                });

                return result;
            }
            catch (Exception ex)
            {
                return new AttackResult
                {
                    AttackType = "IDOR",
                    TargetUrl = entry.Url,
                    Status = $"Error: {ex.Message}",
                    VulnerabilitiesFound = 0,
                    TotalTests = 0,
                    Timestamp = DateTime.Now
                };
            }
        }

        public bool ShouldAttackUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            if (string.IsNullOrEmpty(_targetDomain)) return false;

            try
            {
                var uri = new Uri(url);

                // Check if same domain
                if (!IsSameDomain(uri.Host, _targetDomain))
                {
                    return false;
                }

                // Check if URL has excluded extension
                var path = uri.AbsolutePath.ToLower();
                if (_excludedExtensions.Any(ext => path.EndsWith(ext)))
                {
                    return false;
                }

                // Check excluded patterns
                if (_excludedPatterns.Any(pattern => Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase)))
                {
                    return false;
                }

                // Exclude common static asset paths
                if (path.Contains("/assets/") || path.Contains("/static/") ||
                    path.Contains("/images/") || path.Contains("/css/") ||
                    path.Contains("/js/") || path.Contains("/fonts/"))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsSameDomain(string host1, string host2)
        {
            // Normalize domains
            host1 = host1.ToLower().Replace("www.", "");
            host2 = host2.ToLower().Replace("www.", "");

            return host1 == host2 || host1.EndsWith("." + host2) || host2.EndsWith("." + host1);
        }

        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host.ToLower();

                // Remove www. prefix
                if (host.StartsWith("www."))
                {
                    host = host.Substring(4);
                }

                return host;
            }
            catch
            {
                return null;
            }
        }

        public List<AttackResult> GetAllResults()
        {
            return new List<AttackResult>(_allResults);
        }

        public List<AttackResult> GetResultsForUrl(string url)
        {
            return _resultsByUrl.TryGetValue(url, out var results)
                ? new List<AttackResult>(results)
                : new List<AttackResult>();
        }

        public AttackStatistics GetStatistics()
        {
            var stats = new AttackStatistics
            {
                TotalAttacks = _allResults.Count,
                TotalVulnerabilities = _allResults.Sum(r => r.VulnerabilitiesFound),
                TotalTests = _allResults.Sum(r => r.TotalTests),
                AverageDuration = _allResults.Any() ? _allResults.Average(r => r.Duration) : 0,

                AttackTypeDistribution = _allResults
                    .GroupBy(r => r.AttackType)
                    .ToDictionary(g => g.Key, g => g.Count()),

                VulnerabilitiesByType = _allResults
                    .Where(r => r.VulnerabilitiesFound > 0)
                    .GroupBy(r => r.AttackType)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.VulnerabilitiesFound)),

                UniqueUrlsTested = _resultsByUrl.Keys.Count,

                MostVulnerableUrls = _resultsByUrl
                    .Select(kvp => new UrlVulnerabilityInfo
                    {
                        Url = kvp.Key,
                        TotalVulnerabilities = kvp.Value.Sum(r => r.VulnerabilitiesFound),
                        AttackTypes = kvp.Value.Select(r => r.AttackType).Distinct().ToList()
                    })
                    .OrderByDescending(u => u.TotalVulnerabilities)
                    .Take(10)
                    .ToList()
            };

            return stats;
        }

        public string GenerateComprehensiveReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("          AUTO ATTACK MODE - COMPREHENSIVE REPORT");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"Target Domain: {_targetDomain}");
            sb.AppendLine($"Report Generated: {DateTime.Now}");
            sb.AppendLine($"Auto Attack Mode: {(_isEnabled ? "ENABLED" : "DISABLED")}");
            sb.AppendLine();

            var stats = GetStatistics();

            sb.AppendLine("SUMMARY:");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            sb.AppendLine($"Total Attacks Run: {stats.TotalAttacks}");
            sb.AppendLine($"Total Vulnerabilities Found: {stats.TotalVulnerabilities}");
            sb.AppendLine($"Total Tests Performed: {stats.TotalTests}");
            sb.AppendLine($"Unique URLs Tested: {stats.UniqueUrlsTested}");
            sb.AppendLine($"Average Attack Duration: {stats.AverageDuration:F2} seconds");
            sb.AppendLine();

            if (stats.AttackTypeDistribution.Any())
            {
                sb.AppendLine("ATTACK TYPE DISTRIBUTION:");
                sb.AppendLine("─────────────────────────────────────────────────────────");
                foreach (var kvp in stats.AttackTypeDistribution.OrderByDescending(x => x.Value))
                {
                    var vulnCount = stats.VulnerabilitiesByType.ContainsKey(kvp.Key)
                        ? stats.VulnerabilitiesByType[kvp.Key]
                        : 0;
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value} attacks, {vulnCount} vulnerabilities");
                }
                sb.AppendLine();
            }

            if (stats.MostVulnerableUrls.Any())
            {
                sb.AppendLine("MOST VULNERABLE URLs:");
                sb.AppendLine("─────────────────────────────────────────────────────────");
                foreach (var urlInfo in stats.MostVulnerableUrls)
                {
                    sb.AppendLine($"  URL: {urlInfo.Url}");
                    sb.AppendLine($"  Vulnerabilities: {urlInfo.TotalVulnerabilities}");
                    sb.AppendLine($"  Attack Types: {string.Join(", ", urlInfo.AttackTypes)}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("DETAILED RESULTS:");
            sb.AppendLine("═══════════════════════════════════════════════════════════");

            var groupedResults = _allResults
                .Where(r => r.VulnerabilitiesFound > 0)
                .GroupBy(r => r.AttackType);

            foreach (var group in groupedResults)
            {
                sb.AppendLine();
                sb.AppendLine($"[{group.Key}] - {group.Count()} vulnerable URL(s)");
                sb.AppendLine(new string('─', 60));

                foreach (var result in group)
                {
                    sb.AppendLine($"  URL: {result.TargetUrl}");
                    sb.AppendLine($"  Status: {result.Status}");
                    sb.AppendLine($"  Vulnerabilities: {result.VulnerabilitiesFound}");
                    sb.AppendLine($"  Tests Run: {result.TotalTests}");
                    sb.AppendLine($"  Duration: {result.Duration:F2}s");
                    sb.AppendLine($"  Timestamp: {result.Timestamp}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }

        public void ClearResults()
        {
            _allResults.Clear();
            _resultsByUrl.Clear();
        }

        protected virtual void OnAttackStarted(AttackResultEventArgs e)
        {
            AttackStarted?.Invoke(this, e);
        }

        protected virtual void OnAttackCompleted(AttackResultEventArgs e)
        {
            AttackCompleted?.Invoke(this, e);
        }

        protected virtual void OnAttackProgress(AttackProgressEventArgs e)
        {
            AttackProgress?.Invoke(this, e);
        }

        protected virtual void OnScanCompleted(AttackSummaryEventArgs e)
        {
            ScanCompleted?.Invoke(this, e);
        }
    }

    public class AutoAttackOptions
    {
        public bool EnableXSSScanning { get; set; } = true;
        public bool EnableSQLInjectionScanning { get; set; } = true;
        public bool EnableIDORScanning { get; set; } = true;
        public bool TestHeaders { get; set; } = true;
        public bool TestCookies { get; set; } = true;
        public int DelayBetweenRequests { get; set; } = 100;
    }

    public class AutoAttackReport
    {
        public string TargetUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public int TotalVulnerabilities { get; set; }
        public int TotalTests { get; set; }
        public List<AttackResult> AttackResults { get; set; }
        public string Error { get; set; }
    }

    public class AttackResult
    {
        public string AttackType { get; set; }
        public string TargetUrl { get; set; }
        public string Status { get; set; }
        public int VulnerabilitiesFound { get; set; }
        public int TotalTests { get; set; }
        public double Duration { get; set; }
        public object Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AttackStatistics
    {
        public int TotalAttacks { get; set; }
        public int TotalVulnerabilities { get; set; }
        public int TotalTests { get; set; }
        public double AverageDuration { get; set; }
        public int UniqueUrlsTested { get; set; }
        public Dictionary<string, int> AttackTypeDistribution { get; set; }
        public Dictionary<string, int> VulnerabilitiesByType { get; set; }
        public List<UrlVulnerabilityInfo> MostVulnerableUrls { get; set; }
    }

    public class UrlVulnerabilityInfo
    {
        public string Url { get; set; }
        public int TotalVulnerabilities { get; set; }
        public List<string> AttackTypes { get; set; }
    }

    public class AttackResultEventArgs : EventArgs
    {
        public string Url { get; set; }
        public string AttackType { get; set; }
        public string Status { get; set; }
        public int VulnerabilitiesFound { get; set; }
    }

    public class AttackProgressEventArgs : EventArgs
    {
        public string Url { get; set; }
        public string AttackType { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
    }

    public class AttackSummaryEventArgs : EventArgs
    {
        public string Url { get; set; }
        public int TotalAttacks { get; set; }
        public int TotalVulnerabilities { get; set; }
        public double Duration { get; set; }
    }
}
