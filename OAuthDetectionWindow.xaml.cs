using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using WebTrafficInspector.Services;

namespace WebTrafficInspector
{
    public partial class OAuthDetectionWindow : Window
    {
        private OAuthOIDCVulnerabilityScannerService _oauthScanner;
        private PrivilegeEscalationScannerService _privEscScanner;
        private ObservableCollection<OAuthFlowDisplayModel> _flows;
        private Dictionary<string, OAuthScanReport> _scanResults;

        public OAuthDetectionWindow(OAuthOIDCVulnerabilityScannerService oauthScanner,
            PrivilegeEscalationScannerService privEscScanner)
        {
            InitializeComponent();
            _oauthScanner = oauthScanner;
            _privEscScanner = privEscScanner;
            _flows = new ObservableCollection<OAuthFlowDisplayModel>();
            _scanResults = new Dictionary<string, OAuthScanReport>();

            FlowsDataGrid.ItemsSource = _flows;

            // Subscribe to real-time updates
            _oauthScanner.OAuthFlowDetected += OnOAuthFlowDetected;
            _oauthScanner.VulnerabilityFound += OnVulnerabilityFound;

            LoadDetectedFlows();
        }

        private void OnOAuthFlowDetected(object sender, OAuthDetectedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                LoadDetectedFlows();
                StatusText.Text = $"New OAuth flow detected: {e.FlowData.FlowType}";
            });
        }

        private void OnVulnerabilityFound(object sender, VulnerabilityFoundEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text = $"Vulnerability found: {e.Vulnerability.Type} ({e.Vulnerability.Severity})";
            });
        }

        private void LoadDetectedFlows()
        {
            _flows.Clear();

            var detectedFlows = _oauthScanner.GetDetectedFlows();
            foreach (var flow in detectedFlows)
            {
                _flows.Add(new OAuthFlowDisplayModel
                {
                    FlowId = flow.FlowId,
                    FlowType = flow.FlowType,
                    Host = flow.Host,
                    AttackStatus = _scanResults.ContainsKey(flow.FlowId) ?
                        (_scanResults[flow.FlowId].VulnerabilitiesFound > 0 ? "Vulnerable" : "Attacked") :
                        "Not Attacked",
                    FlowData = flow
                });
            }

            StatusText.Text = $"Loaded {_flows.Count} OAuth/OIDC flow(s)";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDetectedFlows();
        }

        private async void AttackSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (FlowsDataGrid.SelectedItem is OAuthFlowDisplayModel selectedFlow)
            {
                await AttackFlow(selectedFlow);
            }
            else
            {
                MessageBox.Show("Please select a flow to attack.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void AttackAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_flows.Count == 0)
            {
                MessageBox.Show("No flows to attack.", "No Flows",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Attack all {_flows.Count} detected OAuth flows?\n\nThis will test for 16 different vulnerability types.",
                "Confirm Attack All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AttackSelectedButton.IsEnabled = false;
                AttackAllButton.IsEnabled = false;
                StatusText.Text = "Attacking all flows...";

                int completedCount = 0;
                foreach (var flow in _flows)
                {
                    await AttackFlow(flow);
                    completedCount++;
                    StatusText.Text = $"Attacking flows... ({completedCount}/{_flows.Count})";
                }

                AttackSelectedButton.IsEnabled = true;
                AttackAllButton.IsEnabled = true;
                StatusText.Text = $"Attack completed. Scanned {_flows.Count} flow(s).";
            }
        }

        private async Task AttackFlow(OAuthFlowDisplayModel flowModel)
        {
            try
            {
                StatusText.Text = $"Attacking flow {flowModel.FlowId}...";

                var scanOptions = new OAuthScanOptions
                {
                    PerformActiveAttacks = true,
                    StopOnFirstVulnerability = false,
                    DelayBetweenRequests = 100
                };

                var scanReport = await _oauthScanner.ScanOAuthFlow(flowModel.FlowId, scanOptions);
                _scanResults[flowModel.FlowId] = scanReport;

                // Update attack status
                flowModel.AttackStatus = scanReport.VulnerabilitiesFound > 0 ? "Vulnerable" : "Attacked";

                StatusText.Text = $"Attack completed. Found {scanReport.VulnerabilitiesFound} vulnerability(ies).";

                // Refresh display if this flow is selected
                if (FlowsDataGrid.SelectedItem == flowModel)
                {
                    DisplayFlowDetails(flowModel);
                }

                // Show results notification
                if (scanReport.VulnerabilitiesFound > 0)
                {
                    MessageBox.Show(
                        $"Found {scanReport.VulnerabilitiesFound} vulnerability(ies)!\n\n" +
                        $"Flow: {flowModel.FlowType}\n" +
                        $"Host: {flowModel.Host}\n\n" +
                        $"Check the Vulnerabilities tab for details.",
                        "Vulnerabilities Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error attacking flow: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Attack failed.";
            }
        }

        private void FlowsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlowsDataGrid.SelectedItem is OAuthFlowDisplayModel selectedFlow)
            {
                DisplayFlowDetails(selectedFlow);
            }
        }

        private void DisplayFlowDetails(OAuthFlowDisplayModel flowModel)
        {
            var flow = flowModel.FlowData;

            // Display flow information
            FlowIdTextBox.Text = flow.FlowId;
            FlowTypeTextBox.Text = flow.FlowType;
            HostTextBox.Text = flow.Host;
            AuthEndpointTextBox.Text = flow.AuthorizationEndpoint ?? "N/A";
            ClientIdTextBox.Text = flow.ClientId ?? "N/A";
            RedirectUriTextBox.Text = flow.RedirectUri ?? "N/A";
            ScopeTextBox.Text = flow.Scope ?? "N/A";
            StateTextBox.Text = flow.State ?? "N/A";

            // Display request/response
            RequestTextBox.Text = FormatRequestDetails(flow);
            ResponseTextBox.Text = flow.ResponseBody ?? "No response body captured";

            // Display vulnerabilities if scan was performed
            if (_scanResults.ContainsKey(flow.FlowId))
            {
                var scanReport = _scanResults[flow.FlowId];
                DisplayVulnerabilities(scanReport);
                DisplayAttackResults(scanReport);
            }
            else
            {
                VulnerabilitiesDataGrid.ItemsSource = null;
                VulnerabilityDetailsTextBox.Text = "No attack performed yet. Click 'Attack Selected' to scan for vulnerabilities.";
                AttackResultsTextBox.Text = "No attack results available.";
            }
        }

        private string FormatRequestDetails(OAuthFlowData flow)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"URL: {flow.Url}");
            sb.AppendLine();
            sb.AppendLine("OAuth Parameters:");
            sb.AppendLine($"  client_id: {flow.ClientId ?? "N/A"}");
            sb.AppendLine($"  redirect_uri: {flow.RedirectUri ?? "N/A"}");
            sb.AppendLine($"  response_type: {flow.ResponseType ?? "N/A"}");
            sb.AppendLine($"  scope: {flow.Scope ?? "N/A"}");
            sb.AppendLine($"  state: {flow.State ?? "N/A"}");
            sb.AppendLine($"  nonce: {flow.Nonce ?? "N/A"}");

            if (!string.IsNullOrEmpty(flow.CodeChallenge))
            {
                sb.AppendLine($"  code_challenge: {flow.CodeChallenge}");
                sb.AppendLine($"  code_challenge_method: {flow.CodeChallengeMethod}");
            }

            if (flow.AllParameters.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("All Parameters:");
                foreach (var param in flow.AllParameters)
                {
                    sb.AppendLine($"  {param.Key}: {param.Value}");
                }
            }

            if (!string.IsNullOrEmpty(flow.RequestBody))
            {
                sb.AppendLine();
                sb.AppendLine("Request Body:");
                sb.AppendLine(flow.RequestBody);
            }

            return sb.ToString();
        }

        private void DisplayVulnerabilities(OAuthScanReport scanReport)
        {
            var vulnerabilities = new ObservableCollection<OAuthVulnerabilityDisplayModel>();

            foreach (var vuln in scanReport.Vulnerabilities)
            {
                vulnerabilities.Add(new OAuthVulnerabilityDisplayModel
                {
                    Type = vuln.Type,
                    Severity = vuln.Severity,
                    Description = vuln.Description,
                    VulnerabilityData = vuln
                });
            }

            VulnerabilitiesDataGrid.ItemsSource = vulnerabilities;

            if (vulnerabilities.Count == 0)
            {
                VulnerabilityDetailsTextBox.Text = "No vulnerabilities detected. The OAuth flow appears to be secure.";
            }
        }

        private void DisplayAttackResults(OAuthScanReport scanReport)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine("           OAuth/OIDC Attack Results");
            sb.AppendLine("═══════════════════════════════════════════════════════");
            sb.AppendLine($"Flow ID: {scanReport.FlowId}");
            sb.AppendLine($"Flow Type: {scanReport.FlowType}");
            sb.AppendLine($"Duration: {scanReport.Duration:F2}s");
            sb.AppendLine($"Vulnerabilities Found: {scanReport.VulnerabilitiesFound}");
            sb.AppendLine($"Attack Results: {scanReport.AttackResults.Count}");
            sb.AppendLine();

            if (scanReport.VulnerabilitiesFound > 0)
            {
                sb.AppendLine("VULNERABILITIES DETECTED:");
                sb.AppendLine(new string('-', 60));

                foreach (var vuln in scanReport.Vulnerabilities)
                {
                    sb.AppendLine($"\n[{vuln.Severity}] {vuln.Type}");
                    sb.AppendLine($"Description: {vuln.Description}");
                    sb.AppendLine($"Recommendation: {vuln.Recommendation}");

                    if (!string.IsNullOrEmpty(vuln.Evidence))
                    {
                        sb.AppendLine($"Evidence: {vuln.Evidence}");
                    }

                    if (!string.IsNullOrEmpty(vuln.CWE))
                    {
                        sb.AppendLine($"CWE: {vuln.CWE}");
                    }

                    // Generate automatic Proof of Concept (PoC)
                    var poc = GeneratePoC(vuln, scanReport);
                    if (!string.IsNullOrEmpty(poc))
                    {
                        sb.AppendLine();
                        sb.AppendLine("═══ PROOF OF CONCEPT (PoC) ═══");
                        sb.AppendLine(poc);
                        sb.AppendLine("═══════════════════════════════");
                    }
                }
            }
            else
            {
                sb.AppendLine("✓ No vulnerabilities detected.");
            }

            if (scanReport.AttackResults.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("ATTACK RESULTS:");
                sb.AppendLine(new string('-', 60));

                foreach (var attack in scanReport.AttackResults)
                {
                    sb.AppendLine($"\nAttack Type: {attack.AttackType}");
                    sb.AppendLine($"Success: {(attack.Success ? "YES" : "NO")}");
                    sb.AppendLine($"Details: {attack.Details}");

                    if (!string.IsNullOrEmpty(attack.Payload))
                    {
                        sb.AppendLine($"Payload: {attack.Payload}");
                    }

                    if (!string.IsNullOrEmpty(attack.Evidence))
                    {
                        sb.AppendLine($"Evidence: {attack.Evidence}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════════");

            AttackResultsTextBox.Text = sb.ToString();
        }

        private void VulnerabilitiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VulnerabilitiesDataGrid.SelectedItem is OAuthVulnerabilityDisplayModel selectedVuln)
            {
                var vuln = selectedVuln.VulnerabilityData;
                var sb = new StringBuilder();

                sb.AppendLine($"Type: {vuln.Type}");
                sb.AppendLine($"Severity: {vuln.Severity}");
                sb.AppendLine();
                sb.AppendLine($"Description:");
                sb.AppendLine(vuln.Description);
                sb.AppendLine();
                sb.AppendLine($"Recommendation:");
                sb.AppendLine(vuln.Recommendation);

                if (!string.IsNullOrEmpty(vuln.Evidence))
                {
                    sb.AppendLine();
                    sb.AppendLine($"Evidence:");
                    sb.AppendLine(vuln.Evidence);
                }

                if (!string.IsNullOrEmpty(vuln.CWE))
                {
                    sb.AppendLine();
                    sb.AppendLine($"CWE Reference: {vuln.CWE}");
                }

                VulnerabilityDetailsTextBox.Text = sb.ToString();
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scanResults.Count == 0)
            {
                MessageBox.Show("No scan results to report. Please attack at least one flow first.",
                    "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════════════");
                sb.AppendLine("     OAuth/OIDC Vulnerability Assessment Report");
                sb.AppendLine("═══════════════════════════════════════════════════════════");
                sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total Flows Scanned: {_scanResults.Count}");
                sb.AppendLine();

                int totalVulnerabilities = _scanResults.Values.Sum(r => r.VulnerabilitiesFound);
                sb.AppendLine($"Total Vulnerabilities Found: {totalVulnerabilities}");
                sb.AppendLine();

                foreach (var kvp in _scanResults)
                {
                    var report = kvp.Value;
                    sb.AppendLine();
                    sb.AppendLine("───────────────────────────────────────────────────────────");
                    sb.AppendLine($"Flow: {report.FlowId}");
                    sb.AppendLine($"Type: {report.FlowType}");
                    sb.AppendLine($"Duration: {report.Duration:F2}s");
                    sb.AppendLine($"Vulnerabilities: {report.VulnerabilitiesFound}");
                    sb.AppendLine();

                    if (report.Vulnerabilities.Count > 0)
                    {
                        foreach (var vuln in report.Vulnerabilities)
                        {
                            sb.AppendLine($"  [{vuln.Severity}] {vuln.Type}");
                            sb.AppendLine($"  {vuln.Description}");
                            sb.AppendLine($"  Recommendation: {vuln.Recommendation}");
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine("═══════════════════════════════════════════════════════════");

                var saveDialog = new SaveFileDialog
                {
                    Title = "Save OAuth Report",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"OAuth_Report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, sb.ToString());
                    StatusText.Text = $"Report saved to {saveDialog.FileName}";
                    MessageBox.Show("Report generated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportResultsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scanResults.Count == 0)
            {
                MessageBox.Show("No results to export. Please attack at least one flow first.",
                    "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Export Results",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    FileName = $"OAuth_Results_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(_scanResults,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveDialog.FileName, json);
                    StatusText.Text = $"Results exported to {saveDialog.FileName}";
                    MessageBox.Show("Results exported successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting results: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string GeneratePoC(OAuthVulnerability vuln, OAuthScanReport scanReport)
        {
            var poc = new StringBuilder();

            switch (vuln.Type)
            {
                case "Missing State Parameter":
                case "CSRF Vulnerability":
                    poc.AppendLine("Attack Vector: CSRF Attack");
                    poc.AppendLine("1. Create malicious HTML page:");
                    poc.AppendLine("```html");
                    poc.AppendLine("<!DOCTYPE html>");
                    poc.AppendLine("<html>");
                    poc.AppendLine("<body>");
                    poc.AppendLine($"  <iframe src=\"{scanReport.Url.Replace("&state=", "&state=")}\"></iframe>");
                    poc.AppendLine("</body>");
                    poc.AppendLine("</html>");
                    poc.AppendLine("```");
                    poc.AppendLine("2. Send link to victim");
                    poc.AppendLine("3. When victim clicks, attacker gains access to their account");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Account takeover, unauthorized access");
                    break;

                case "Open Redirect":
                    poc.AppendLine("Attack Vector: Open Redirect");
                    poc.AppendLine("1. Modify redirect_uri parameter:");
                    poc.AppendLine($"   Original: {scanReport.Url}");
                    poc.AppendLine($"   Malicious: {scanReport.Url.Replace("redirect_uri=", "redirect_uri=https://attacker.com&old_redirect_uri=")}");
                    poc.AppendLine("2. Authorization code/token sent to attacker");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Authorization code interception, account takeover");
                    break;

                case "Token Leakage":
                    poc.AppendLine("Attack Vector: Token Leakage in URL Fragment");
                    poc.AppendLine("1. Victim accesses OAuth endpoint");
                    poc.AppendLine("2. Token appears in URL fragment:");
                    poc.AppendLine($"   {scanReport.Url}#access_token=LEAKED_TOKEN");
                    poc.AppendLine("3. Token logged in browser history, referrer headers, analytics");
                    poc.AppendLine();
                    poc.AppendLine("Exploit:");
                    poc.AppendLine("- Check browser history");
                    poc.AppendLine("- Monitor analytics/logging systems");
                    poc.AppendLine("- Intercept referrer headers");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Token theft, unauthorized API access");
                    break;

                case "Missing PKCE":
                    poc.AppendLine("Attack Vector: Authorization Code Interception");
                    poc.AppendLine("1. Attacker intercepts authorization code from redirect");
                    poc.AppendLine("2. Exchange code for access token:");
                    poc.AppendLine("```bash");
                    poc.AppendLine("curl -X POST {token_endpoint}");
                    poc.AppendLine("  -d 'grant_type=authorization_code'");
                    poc.AppendLine("  -d 'code=INTERCEPTED_CODE'");
                    poc.AppendLine($"  -d 'client_id={vuln.Evidence}'");
                    poc.AppendLine("  -d 'redirect_uri=...'");
                    poc.AppendLine("```");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Account takeover on public/mobile clients");
                    break;

                case "Scope Manipulation":
                case "Privilege Escalation":
                    poc.AppendLine("Attack Vector: Scope Escalation");
                    poc.AppendLine("1. Modify scope parameter to request elevated privileges:");
                    poc.AppendLine($"   Original: scope=read");
                    poc.AppendLine($"   Malicious: scope=read+write+admin+delete");
                    poc.AppendLine("2. If server doesn't validate, attacker gets elevated access");
                    poc.AppendLine();
                    poc.AppendLine("Test URLs:");
                    poc.AppendLine($"   {scanReport.Url.Replace("scope=", "scope=admin+")}");
                    poc.AppendLine($"   {scanReport.Url.Replace("scope=", "scope=*+")}");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Unauthorized access to privileged operations");
                    break;

                case "Client Secret Exposure":
                    poc.AppendLine("Attack Vector: Client Secret Compromise");
                    poc.AppendLine($"Exposed Secret: {vuln.Evidence}");
                    poc.AppendLine();
                    poc.AppendLine("Exploitation:");
                    poc.AppendLine("1. Use exposed client secret to impersonate legitimate client");
                    poc.AppendLine("2. Request tokens on behalf of other users:");
                    poc.AppendLine("```bash");
                    poc.AppendLine("curl -X POST {token_endpoint}");
                    poc.AppendLine($"  -d 'client_id={vuln.Evidence}'");
                    poc.AppendLine("  -d 'client_secret=EXPOSED_SECRET'");
                    poc.AppendLine("  -d 'grant_type=client_credentials'");
                    poc.AppendLine("```");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Full application compromise, data breach");
                    break;

                case "Authorization Code Reuse":
                    poc.AppendLine("Attack Vector: Code Replay Attack");
                    poc.AppendLine("1. Intercept authorization code from legitimate flow");
                    poc.AppendLine("2. Replay code multiple times:");
                    poc.AppendLine("```bash");
                    poc.AppendLine("# Replay 1");
                    poc.AppendLine("curl -X POST {token_endpoint} -d 'code=INTERCEPTED_CODE' ...");
                    poc.AppendLine("# Replay 2");
                    poc.AppendLine("curl -X POST {token_endpoint} -d 'code=INTERCEPTED_CODE' ...");
                    poc.AppendLine("```");
                    poc.AppendLine("3. If accepted, attacker gets multiple access tokens");
                    poc.AppendLine();
                    poc.AppendLine("Impact: Session fixation, multiple token generation");
                    break;

                case "Zero-Day Vulnerability":
                    poc.AppendLine("Potential Zero-Day Vulnerability Detected!");
                    poc.AppendLine($"Suspicious Pattern: {vuln.Evidence}");
                    poc.AppendLine();
                    poc.AppendLine("Investigation Steps:");
                    poc.AppendLine("1. Analyze non-standard OAuth parameters");
                    poc.AppendLine("2. Test parameter manipulation");
                    poc.AppendLine("3. Check for undocumented endpoints");
                    poc.AppendLine("4. Fuzz all parameters");
                    poc.AppendLine();
                    poc.AppendLine("Recommended Actions:");
                    poc.AppendLine("- Deep security audit required");
                    poc.AppendLine("- Test with Burp Suite/ZAP");
                    poc.AppendLine("- Review application source code");
                    break;

                default:
                    poc.AppendLine($"Vulnerability Type: {vuln.Type}");
                    poc.AppendLine($"Evidence: {vuln.Evidence}");
                    poc.AppendLine();
                    poc.AppendLine("Manual exploitation required.");
                    poc.AppendLine("Consult security documentation for specific attack vectors.");
                    break;
            }

            return poc.ToString();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Unsubscribe from events
            _oauthScanner.OAuthFlowDetected -= OnOAuthFlowDetected;
            _oauthScanner.VulnerabilityFound -= OnVulnerabilityFound;
            base.OnClosing(e);
        }
    }

    // Display models for UI binding
    public class OAuthFlowDisplayModel : INotifyPropertyChanged
    {
        private string _attackStatus;

        public string FlowId { get; set; }
        public string FlowType { get; set; }
        public string Host { get; set; }
        public string AttackStatus
        {
            get => _attackStatus;
            set
            {
                _attackStatus = value;
                OnPropertyChanged(nameof(AttackStatus));
            }
        }
        public OAuthFlowData FlowData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OAuthVulnerabilityDisplayModel
    {
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public OAuthVulnerability VulnerabilityData { get; set; }
    }
}
