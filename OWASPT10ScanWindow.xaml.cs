using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using WebTrafficInspector.Services;

namespace WebTrafficInspector
{
    public partial class OWASPT10ScanWindow : Window
    {
        private OWASPT10_2025_ScannerService _scanner;
        private ObservableCollection<OWASPT10ScanReportViewModel> _scanReports;
        private ObservableCollection<OWASPT10Vulnerability> _allVulnerabilities;
        private ObservableCollection<OWASPT10Vulnerability> _filteredVulnerabilities;
        private OWASPT10ScanReport _currentReport;
        private OWASPT10Vulnerability _currentVulnerability;
        private bool _isScanning = false;

        public OWASPT10ScanWindow(OWASPT10_2025_ScannerService scanner)
        {
            try
            {
                InitializeComponent();
                _scanner = scanner;
                _scanReports = new ObservableCollection<OWASPT10ScanReportViewModel>();
                _allVulnerabilities = new ObservableCollection<OWASPT10Vulnerability>();
                _filteredVulnerabilities = new ObservableCollection<OWASPT10Vulnerability>();

                // Check that UI elements are properly initialized
                if (ScannedUrlsDataGrid != null)
                    ScannedUrlsDataGrid.ItemsSource = _scanReports;
                if (VulnerabilitiesDataGrid != null)
                    VulnerabilitiesDataGrid.ItemsSource = _filteredVulnerabilities;

                // Subscribe to scanner events (with null check)
                if (_scanner != null)
                {
                    _scanner.ScanProgress += OnScanProgress;
                    _scanner.VulnerabilityFound += OnVulnerabilityFound;
                }

                LoadExistingScanResults();
                UpdateStatistics();
                
                // Check that UI elements are properly initialized before accessing them
                if (EnableScannerCheckBox != null)
                    EnableScannerCheckBox.IsChecked = _scanner?.IsEnabled ?? false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize OWASP scanner window: {ex.Message}\n\n{ex.StackTrace}", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void LoadExistingScanResults()
        {
            try
            {
                if (_scanner == null || _scanReports == null)
                    return;

                var results = _scanner.GetAllScanResults();
                foreach (var result in results)
                {
                    var viewModel = new OWASPT10ScanReportViewModel(result.Value);
                    _scanReports.Add(viewModel);
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error loading existing scan results: {ex.Message}");
            }
        }

        private void OnScanProgress(object sender, OWASPT10ScanProgressEventArgs e)
        {
            if (Dispatcher == null) return;

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (StatusText != null)
                        StatusText.Text = $"Scanning: {e.CurrentCategory} - {e.Message}";
                    
                    if (ScanProgressBar != null)
                        ScanProgressBar.Value = e.ProgressPercentage;
                    
                    if (ProgressText != null)
                        ProgressText.Text = $"{e.ProgressPercentage:F0}% - {e.Message}";

                    if (ScanProgressBar != null && ScanProgressBar.Visibility == Visibility.Collapsed)
                    {
                        ScanProgressBar.Visibility = Visibility.Visible;
                        if (ProgressText != null)
                            ProgressText.Visibility = Visibility.Visible;
                    }

                    // Update the specific report status
                    if (_scanReports != null)
                    {
                        var report = _scanReports.FirstOrDefault(r => r.Url == e.Url);
                        if (report != null)
                        {
                            report.Status = "Scanning...";
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnScanProgress: {ex.Message}");
                }
            });
        }

        private void OnVulnerabilityFound(object sender, OWASPT10VulnerabilityFoundEventArgs e)
        {
            if (Dispatcher == null) return;

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (StatusText != null)
                    {
                        StatusText.Text = $"Vulnerability found: {e.Vulnerability?.Type} ({e.Vulnerability?.Severity})";
                        StatusText.Foreground = System.Windows.Media.Brushes.Red;
                    }

                    // Update statistics
                    UpdateStatistics();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OnVulnerabilityFound: {ex.Message}");
                }
            });
        }

        private void EnableScannerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_scanner != null)
                {
                    _scanner.IsEnabled = true;
                    if (StatusText != null)
                    {
                        StatusText.Text = "Automatic scanning enabled - All new URLs will be scanned";
                        StatusText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnableScannerCheckBox_Checked: {ex.Message}");
            }
        }

        private void EnableScannerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_scanner != null)
                {
                    _scanner.IsEnabled = false;
                    if (StatusText != null)
                    {
                        StatusText.Text = "Automatic scanning disabled";
                        StatusText.Foreground = System.Windows.Media.Brushes.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnableScannerCheckBox_Unchecked: {ex.Message}");
            }
        }

        private async void ScanSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReport = ScannedUrlsDataGrid.SelectedItem as OWASPT10ScanReportViewModel;
            if (selectedReport == null)
            {
                MessageBox.Show("Please select a URL to scan", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await ScanUrl(selectedReport.Url);
        }

        private async void ScanAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scanReports.Count == 0)
            {
                MessageBox.Show("No URLs to scan", "No URLs",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"This will scan {_scanReports.Count} URLs. Continue?",
                "Scan All URLs", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await ScanAllUrls();
            }
        }

        private async Task ScanUrl(string url)
        {
            if (_isScanning)
            {
                MessageBox.Show("A scan is already in progress", "Scan In Progress",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _isScanning = true;
                StopScanButton.IsEnabled = true;
                ScanSelectedButton.IsEnabled = false;
                ScanAllButton.IsEnabled = false;

                StatusText.Text = $"Scanning {url}...";
                ScanProgressBar.Visibility = Visibility.Visible;
                ProgressText.Visibility = Visibility.Visible;
                ScanProgressBar.Value = 0;

                var report = await _scanner.ScanUrl(url);

                // Update or add report
                var existingReport = _scanReports.FirstOrDefault(r => r.Url == url);
                if (existingReport != null)
                {
                    _scanReports.Remove(existingReport);
                }

                var viewModel = new OWASPT10ScanReportViewModel(report);
                _scanReports.Add(viewModel);

                StatusText.Text = $"Scan complete: {report.TotalVulnerabilitiesFound} vulnerabilities found";
                StatusText.Foreground = report.TotalVulnerabilitiesFound > 0
                    ? System.Windows.Media.Brushes.Red
                    : System.Windows.Media.Brushes.Green;

                ScanProgressBar.Value = 100;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Scan failed: {ex.Message}", "Scan Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Scan failed";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                _isScanning = false;
                StopScanButton.IsEnabled = false;
                ScanSelectedButton.IsEnabled = true;
                ScanAllButton.IsEnabled = true;

                await Task.Delay(3000);
                ScanProgressBar.Visibility = Visibility.Collapsed;
                ProgressText.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ScanAllUrls()
        {
            if (_isScanning)
                return;

            try
            {
                _isScanning = true;
                StopScanButton.IsEnabled = true;
                ScanSelectedButton.IsEnabled = false;
                ScanAllButton.IsEnabled = false;

                var urls = _scanReports.Select(r => r.Url).ToList();
                int totalUrls = urls.Count;
                int currentUrl = 0;

                foreach (var url in urls)
                {
                    if (!_isScanning)
                        break;

                    currentUrl++;
                    StatusText.Text = $"Scanning {currentUrl}/{totalUrls}: {url}";

                    var report = await _scanner.ScanUrl(url);

                    var existingReport = _scanReports.FirstOrDefault(r => r.Url == url);
                    if (existingReport != null)
                    {
                        _scanReports.Remove(existingReport);
                    }

                    var viewModel = new OWASPT10ScanReportViewModel(report);
                    _scanReports.Add(viewModel);

                    UpdateStatistics();
                }

                StatusText.Text = $"Scan complete: Scanned {currentUrl} URLs";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Scan failed: {ex.Message}", "Scan Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isScanning = false;
                StopScanButton.IsEnabled = false;
                ScanSelectedButton.IsEnabled = true;
                ScanAllButton.IsEnabled = true;
            }
        }

        private void StopScanButton_Click(object sender, RoutedEventArgs e)
        {
            _isScanning = false;
            StatusText.Text = "Scan stopped by user";
            StatusText.Foreground = System.Windows.Media.Brushes.Orange;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _scanReports.Clear();
            LoadExistingScanResults();
            StatusText.Text = "Results refreshed";
            StatusText.Foreground = System.Windows.Media.Brushes.Green;
        }

        private void ScannedUrlsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ScannedUrlsDataGrid == null)
                    return;

                var selectedReport = ScannedUrlsDataGrid.SelectedItem as OWASPT10ScanReportViewModel;
                if (selectedReport == null)
                    return;

                _currentReport = selectedReport.Report;
                DisplayScanReport(_currentReport);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ScannedUrlsDataGrid_SelectionChanged: {ex.Message}");
            }
        }

        private void DisplayScanReport(OWASPT10ScanReport report)
        {
            try
            {
                if (report == null)
                    return;

                // Update Summary tab
                if (SummaryUrlTextBox != null)
                    SummaryUrlTextBox.Text = report.Url;
                if (ScanDurationTextBox != null)
                    ScanDurationTextBox.Text = $"{report.Duration:F2} seconds";
                if (TotalVulnerabilitiesTextBox != null)
                    TotalVulnerabilitiesTextBox.Text = report.TotalVulnerabilitiesFound.ToString();

                // Count vulnerabilities by severity
                var allVulns = report.VulnerabilitiesByCategory.Values.SelectMany(v => v).ToList();
                if (CriticalCountText != null)
                    CriticalCountText.Text = allVulns.Count(v => v.Severity == "Critical").ToString();
                if (HighCountText != null)
                    HighCountText.Text = allVulns.Count(v => v.Severity == "High").ToString();
                if (MediumCountText != null)
                    MediumCountText.Text = allVulns.Count(v => v.Severity == "Medium").ToString();
                if (LowCountText != null)
                    LowCountText.Text = allVulns.Count(v => v.Severity == "Low").ToString();

                // Display categories
                var categoriesSummary = "";
                foreach (var category in report.VulnerabilitiesByCategory)
                {
                    categoriesSummary += $"✓ {category.Key}: {category.Value.Count} vulnerabilities found\n";
                }
                if (CategoriesTextBox != null)
                    CategoriesTextBox.Text = categoriesSummary;

                // Load all vulnerabilities
                if (_allVulnerabilities != null)
                {
                    _allVulnerabilities.Clear();
                    foreach (var vuln in allVulns)
                    {
                        _allVulnerabilities.Add(vuln);
                    }
                    ApplyVulnerabilityFilter();
                }

                // Display original request/response
                if (OriginalRequestTextBox != null)
                    OriginalRequestTextBox.Text = report.OriginalRequest ?? "No original request captured";
                if (OriginalResponseTextBox != null)
                    OriginalResponseTextBox.Text = report.OriginalResponse ?? "No original response captured";

                // Clear other fields
                if (VulnerabilityDetailsTextBox != null)
                    VulnerabilityDetailsTextBox.Clear();
                if (PoCTextBox != null)
                    PoCTextBox.Clear();
                if (TestRequestTextBox != null)
                    TestRequestTextBox.Clear();
                if (TestResponseTextBox != null)
                    TestResponseTextBox.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DisplayScanReport: {ex.Message}");
            }
        }

        private void VulnerabilitiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (VulnerabilitiesDataGrid == null)
                    return;

                var selectedVuln = VulnerabilitiesDataGrid.SelectedItem as OWASPT10Vulnerability;
                if (selectedVuln == null)
                    return;

                _currentVulnerability = selectedVuln;
                DisplayVulnerabilityDetails(selectedVuln);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in VulnerabilitiesDataGrid_SelectionChanged: {ex.Message}");
            }
        }

        private void DisplayVulnerabilityDetails(OWASPT10Vulnerability vuln)
        {
            try
            {
                if (vuln == null)
                    return;

                // Display vulnerability details
                var details = $"Category: {vuln.Category}\n\n";
                details += $"Type: {vuln.Type}\n\n";
                details += $"Severity: {vuln.Severity}\n\n";
                details += $"Description: {vuln.Description}\n\n";
                details += $"Evidence: {vuln.Evidence}\n\n";
                details += $"CWE: {vuln.CWE}\n\n";
                details += $"Test URL: {vuln.TestUrl}\n";

                if (VulnerabilityDetailsTextBox != null)
                    VulnerabilityDetailsTextBox.Text = details;

                // Display PoC
                if (PoCTextBox != null)
                    PoCTextBox.Text = vuln.PoC ?? "No Proof of Concept available";

                // Display test request/response
                if (TestRequestTextBox != null)
                    TestRequestTextBox.Text = vuln.TestRequest ?? "No test request available";
                if (TestResponseTextBox != null)
                    TestResponseTextBox.Text = vuln.TestResponse ?? "No test response available";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DisplayVulnerabilityDetails: {ex.Message}");
            }
        }

        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Add null check to prevent NullReferenceException
            if (_filteredVulnerabilities != null && _allVulnerabilities != null)
            {
                ApplyVulnerabilityFilter();
            }
        }

        private void ApplyVulnerabilityFilter()
        {
            // Add null checks to prevent NullReferenceException
            if (_filteredVulnerabilities == null || _allVulnerabilities == null)
                return;

            _filteredVulnerabilities.Clear();

            var showCritical = FilterCriticalCheckBox?.IsChecked ?? true;
            var showHigh = FilterHighCheckBox?.IsChecked ?? true;
            var showMedium = FilterMediumCheckBox?.IsChecked ?? true;
            var showLow = FilterLowCheckBox?.IsChecked ?? true;

            foreach (var vuln in _allVulnerabilities)
            {
                bool show = false;
                if (vuln.Severity == "Critical" && showCritical) show = true;
                if (vuln.Severity == "High" && showHigh) show = true;
                if (vuln.Severity == "Medium" && showMedium) show = true;
                if (vuln.Severity == "Low" && showLow) show = true;

                if (show)
                {
                    _filteredVulnerabilities.Add(vuln);
                }
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                if (_scanReports == null || StatsText == null)
                    return;

                var totalScanned = _scanReports.Count;
                var totalVulnerable = _scanReports.Count(r => r.HasVulnerabilities);

                StatsText.Text = $"Scanned: {totalScanned} | Vulnerable: {totalVulnerable}";
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error updating statistics: {ex.Message}");
            }
        }

        private async void ExportHTMLReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "HTML Files (*.html)|*.html",
                    DefaultExt = "html",
                    FileName = $"OWASP_Top10_2025_Security_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var allReports = _scanner.GetAllScanResults();
                    var reportGenerator = new OWASPT10HTMLReportGenerator();
                    var html = reportGenerator.GenerateHTMLReport(allReports);

                    await File.WriteAllTextAsync(saveFileDialog.FileName, html);

                    var result = MessageBox.Show(
                        $"HTML report exported successfully!\n\nPath: {saveFileDialog.FileName}\n\nDo you want to open the report now?",
                        "Export Successful",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }

                    StatusText.Text = "HTML report exported successfully";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export HTML report: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportJSONButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON Files (*.json)|*.json",
                    DefaultExt = "json",
                    FileName = $"OWASP_Top10_2025_Scan_Results_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var allReports = _scanner.GetAllScanResults();
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    var json = JsonSerializer.Serialize(allReports, options);

                    await File.WriteAllTextAsync(saveFileDialog.FileName, json);

                    MessageBox.Show($"JSON report exported successfully!\n\nPath: {saveFileDialog.FileName}",
                        "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                    StatusText.Text = "JSON report exported successfully";
                    StatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export JSON report: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyPoCButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PoCTextBox.Text))
            {
                Clipboard.SetText(PoCTextBox.Text);
                StatusText.Text = "PoC copied to clipboard";
                StatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                MessageBox.Show("No PoC available to copy", "No PoC",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all scan results?\n\nThis action cannot be undone.",
                "Clear Results",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _scanner.ClearAllResults();
                _scanReports.Clear();
                _allVulnerabilities.Clear();
                _filteredVulnerabilities.Clear();

                SummaryUrlTextBox.Clear();
                ScanDurationTextBox.Clear();
                TotalVulnerabilitiesTextBox.Clear();
                CategoriesTextBox.Clear();
                VulnerabilityDetailsTextBox.Clear();
                PoCTextBox.Clear();
                TestRequestTextBox.Clear();
                TestResponseTextBox.Clear();
                OriginalRequestTextBox.Clear();
                OriginalResponseTextBox.Clear();

                CriticalCountText.Text = "0";
                HighCountText.Text = "0";
                MediumCountText.Text = "0";
                LowCountText.Text = "0";

                UpdateStatistics();

                StatusText.Text = "All results cleared";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Unsubscribe from events
            if (_scanner != null)
            {
                _scanner.ScanProgress -= OnScanProgress;
                _scanner.VulnerabilityFound -= OnVulnerabilityFound;
            }

            base.OnClosing(e);
        }
    }

    // ViewModel class for displaying scan reports in DataGrid
    public class OWASPT10ScanReportViewModel : INotifyPropertyChanged
    {
        public OWASPT10ScanReport Report { get; }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Url => Report.Url;
        public int TotalVulnerabilitiesFound => Report.TotalVulnerabilitiesFound;
        public bool HasVulnerabilities => Report.TotalVulnerabilitiesFound > 0;

        public OWASPT10ScanReportViewModel(OWASPT10ScanReport report)
        {
            Report = report;
            _status = string.IsNullOrEmpty(report.Error) ? "Complete" : "Error";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
