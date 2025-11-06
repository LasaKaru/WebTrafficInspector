using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using WebTrafficInspector.Services;
using Microsoft.Win32;

namespace WebTrafficInspector.Windows
{
    public partial class AttackResultsWindow : Window
    {
        private AutoAttackModeService _autoAttackService;
        private ObservableCollection<AttackResult> _results;
        private ObservableCollection<AttackResult> _filteredResults;

        public AttackResultsWindow(AutoAttackModeService autoAttackService)
        {
            InitializeComponent();

            _autoAttackService = autoAttackService ?? throw new ArgumentNullException(nameof(autoAttackService));
            _results = new ObservableCollection<AttackResult>();
            _filteredResults = new ObservableCollection<AttackResult>();

            dgResults.ItemsSource = _filteredResults;

            // Subscribe to events
            _autoAttackService.AttackStarted += OnAttackStarted;
            _autoAttackService.AttackCompleted += OnAttackCompleted;
            _autoAttackService.AttackProgress += OnAttackProgress;
            _autoAttackService.ScanCompleted += OnScanCompleted;

            // Load existing results
            LoadExistingResults();
            UpdateStatistics();
            UpdateTargetUrl();
        }

        private void LoadExistingResults()
        {
            var existingResults = _autoAttackService.GetAllResults();
            foreach (var result in existingResults)
            {
                _results.Add(result);
            }
            ApplyFilter();
        }

        private void UpdateTargetUrl()
        {
            Dispatcher.Invoke(() =>
            {
                txtTargetUrl.Text = !string.IsNullOrEmpty(_autoAttackService.TargetUrl)
                    ? $"Target: {_autoAttackService.TargetUrl}"
                    : "Target: N/A";
            });
        }

        private void OnAttackStarted(object sender, AttackResultEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = "Running";
                LogActivity($"[{DateTime.Now:HH:mm:ss}] Started {e.AttackType} attack on {e.Url}");
            });
        }

        private void OnAttackCompleted(object sender, AttackResultEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Find and add the result
                var results = _autoAttackService.GetAllResults();
                var newResult = results.LastOrDefault(r =>
                    r.AttackType == e.AttackType &&
                    r.TargetUrl == e.Url);

                if (newResult != null && !_results.Contains(newResult))
                {
                    _results.Add(newResult);
                    ApplyFilter();
                }

                UpdateStatistics();

                string vulnStatus = e.VulnerabilitiesFound > 0
                    ? $"VULNERABLE ({e.VulnerabilitiesFound} found)"
                    : "Not Vulnerable";

                LogActivity($"[{DateTime.Now:HH:mm:ss}] Completed {e.AttackType} attack - {vulnStatus}");
            });
        }

        private void OnAttackProgress(object sender, AttackProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = $"{e.AttackType} - {e.Status}";
            });
        }

        private void OnScanCompleted(object sender, AttackSummaryEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtStatus.Text = "Idle";
                UpdateStatistics();

                LogActivity($"[{DateTime.Now:HH:mm:ss}] Scan completed for {e.Url}");
                LogActivity($"    Total attacks: {e.TotalAttacks}, Vulnerabilities: {e.TotalVulnerabilities}, Duration: {e.Duration:F2}s");
            });
        }

        private void UpdateStatistics()
        {
            var stats = _autoAttackService.GetStatistics();

            txtTotalAttacks.Text = stats.TotalAttacks.ToString();
            txtVulnerabilities.Text = stats.TotalVulnerabilities.ToString();
            txtTotalTests.Text = stats.TotalTests.ToString();
            txtUrlsTested.Text = stats.UniqueUrlsTested.ToString();
            txtAvgDuration.Text = $"{stats.AverageDuration:F2}s";

            // Update attack type distribution
            if (stats.AttackTypeDistribution != null)
            {
                dgAttackTypeStats.ItemsSource = stats.AttackTypeDistribution;
            }

            // Update most vulnerable URLs
            if (stats.MostVulnerableUrls != null)
            {
                var urlInfos = stats.MostVulnerableUrls.Select(u => new VulnerableUrlDisplay
                {
                    Url = u.Url,
                    TotalVulnerabilities = u.TotalVulnerabilities,
                    AttackTypesString = string.Join(", ", u.AttackTypes)
                }).ToList();

                dgVulnerableUrls.ItemsSource = urlInfos;
            }
        }

        private void LogActivity(string message)
        {
            txtActivityLog.AppendText(message + Environment.NewLine);
            txtActivityLog.ScrollToEnd();
        }

        private void ApplyFilter()
        {
            _filteredResults.Clear();

            var selectedType = ((ComboBoxItem)cmbFilterType.SelectedItem)?.Content?.ToString();
            bool onlyVulnerable = chkOnlyVulnerable.IsChecked == true;

            var filtered = _results.AsEnumerable();

            // Filter by attack type
            if (selectedType != "All")
            {
                filtered = filtered.Where(r => r.AttackType == selectedType);
            }

            // Filter by vulnerability status
            if (onlyVulnerable)
            {
                filtered = filtered.Where(r => r.VulnerabilitiesFound > 0);
            }

            foreach (var result in filtered.OrderByDescending(r => r.Timestamp))
            {
                _filteredResults.Add(result);
            }
        }

        private void FilterType_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void dgResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgResults.SelectedItem is AttackResult result)
            {
                ShowResultDetails(result);
            }
        }

        private void ShowResultDetails(AttackResult result)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Attack Type: {result.AttackType}");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine($"Target URL: {result.TargetUrl}");
            sb.AppendLine($"Status: {result.Status}");
            sb.AppendLine($"Timestamp: {result.Timestamp}");
            sb.AppendLine($"Duration: {result.Duration:F2} seconds");
            sb.AppendLine($"Vulnerabilities Found: {result.VulnerabilitiesFound}");
            sb.AppendLine($"Total Tests: {result.TotalTests}");
            sb.AppendLine();

            if (result.Details != null)
            {
                sb.AppendLine("DETAILED SCAN REPORT:");
                sb.AppendLine("───────────────────────────────────────────────────");

                if (result.AttackType == "XSS" && result.Details is XSSScanReport xssReport)
                {
                    sb.AppendLine(FormatXSSReport(xssReport));
                }
                else if (result.AttackType == "SQL Injection" && result.Details is SQLIScanReport sqlReport)
                {
                    sb.AppendLine(FormatSQLReport(sqlReport));
                }
                else if (result.AttackType == "IDOR" && result.Details is IDORScanReport idorReport)
                {
                    sb.AppendLine(FormatIDORReport(idorReport));
                }
                else
                {
                    sb.AppendLine(result.Details.ToString());
                }
            }

            txtDetails.Text = sb.ToString();
        }

        private string FormatXSSReport(XSSScanReport report)
        {
            var sb = new System.Text.StringBuilder();

            if (report.Results != null && report.Results.Any())
            {
                foreach (var vuln in report.Results)
                {
                    sb.AppendLine($"Parameter: {vuln.InjectionPoint}");
                    sb.AppendLine($"Location: {vuln.InjectionPoint}");
                    sb.AppendLine($"Payload: {vuln.Payload}");
                    sb.AppendLine($"Type: XSS");
                    sb.AppendLine($"Severity: {vuln.Severity}");
                    sb.AppendLine($"Context: {vuln.InjectionPoint}");
                    if (!string.IsNullOrEmpty(vuln.Evidence))
                    {
                        sb.AppendLine($"Evidence: {vuln.Evidence.Substring(0, Math.Min(150, vuln.Evidence.Length))}...");
                    }
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No XSS vulnerabilities detected.");
            }

            return sb.ToString();
        }

        private string FormatSQLReport(SQLIScanReport report)
        {
            var sb = new System.Text.StringBuilder();

            if (report.Results != null && report.Results.Any())
            {
                var grouped = report.Results.GroupBy(v => v.Type);
                foreach (var group in grouped)
                {
                    sb.AppendLine($"Injection Type: {group.Key}");
                    sb.AppendLine(new string('-', 40));

                    foreach (var vuln in group.Take(5))
                    {
                        sb.AppendLine($"  Parameter: {vuln.InjectionPoint}");
                        sb.AppendLine($"  Payload: {vuln.Payload}");
                        sb.AppendLine($"  Confidence: {vuln.Confidence}");
                        sb.AppendLine($"  Severity: {vuln.Severity}");
                        if (!string.IsNullOrEmpty(vuln.Evidence))
                        {
                            sb.AppendLine($"  Evidence: {vuln.Evidence.Substring(0, Math.Min(100, vuln.Evidence.Length))}...");
                        }
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No SQL Injection vulnerabilities detected.");
            }

            return sb.ToString();
        }

        private string FormatIDORReport(IDORScanReport report)
        {
            var sb = new System.Text.StringBuilder();

            if (report.Vulnerabilities != null && report.Vulnerabilities.Any())
            {
                var grouped = report.Vulnerabilities.GroupBy(v => v.Type);
                foreach (var group in grouped)
                {
                    sb.AppendLine($"Type: {group.Key}");
                    sb.AppendLine(new string('-', 40));

                    foreach (var vuln in group.Take(5))
                    {
                        sb.AppendLine($"  Parameter: {vuln.Parameter}");
                        sb.AppendLine($"  Original Value: {vuln.OriginalValue}");
                        sb.AppendLine($"  Tested Value: {vuln.TestedValue}");
                        sb.AppendLine($"  Severity: {vuln.Severity}");
                        sb.AppendLine($"  Status Code: {vuln.StatusCode}");
                        if (!string.IsNullOrEmpty(vuln.Evidence))
                        {
                            sb.AppendLine($"  Evidence: {vuln.Evidence.Substring(0, Math.Min(100, vuln.Evidence.Length))}...");
                        }
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                sb.AppendLine("No IDOR vulnerabilities detected.");
            }

            return sb.ToString();
        }

        private void btnExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"AutoAttack_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    DefaultExt = ".txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var report = _autoAttackService.GenerateComprehensiveReport();
                    File.WriteAllText(saveDialog.FileName, report);

                    LogActivity($"[{DateTime.Now:HH:mm:ss}] Report exported to: {saveDialog.FileName}");
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}",
                        "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}",
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearResults_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all attack results?",
                "Confirm Clear",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _results.Clear();
                _filteredResults.Clear();
                _autoAttackService.ClearResults();
                txtActivityLog.Clear();
                UpdateStatistics();

                LogActivity($"[{DateTime.Now:HH:mm:ss}] All results cleared");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadExistingResults();
            UpdateStatistics();
            LogActivity($"[{DateTime.Now:HH:mm:ss}] Results refreshed");
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Unsubscribe from events
            _autoAttackService.AttackStarted -= OnAttackStarted;
            _autoAttackService.AttackCompleted -= OnAttackCompleted;
            _autoAttackService.AttackProgress -= OnAttackProgress;
            _autoAttackService.ScanCompleted -= OnScanCompleted;

            base.OnClosing(e);
        }

        private class VulnerableUrlDisplay
        {
            public string Url { get; set; }
            public int TotalVulnerabilities { get; set; }
            public string AttackTypesString { get; set; }
        }
    }
}
