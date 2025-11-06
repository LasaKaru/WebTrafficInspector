using Microsoft.Web.WebView2.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using WebTrafficInspector.Models;
using WebTrafficInspector.Services;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;


namespace WebTrafficInspector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //    private ProxyService _proxyService;
        //    private SessionService _sessionService;
        //    private ObservableCollection<TrafficEntry> _trafficEntries;
        //    private bool _isProxyStarted = false;
        //    private string _currentSessionPath = null;
        //    private string _currentSessionName = "Untitled Session";
        //    private bool _hasUnsavedChanges = false;

        //    public MainWindow()
        //    {
        //        InitializeComponent();
        //        InitializeApplication();
        //        SetupKeyboardShortcuts();
        //    }

        //    private void SetupKeyboardShortcuts()
        //    {
        //        var newSessionGesture = new KeyGesture(Key.N, ModifierKeys.Control);
        //        var openSessionGesture = new KeyGesture(Key.O, ModifierKeys.Control);
        //        var saveSessionGesture = new KeyGesture(Key.S, ModifierKeys.Control);
        //        var saveAsGesture = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

        //        InputBindings.Add(new KeyBinding(new RelayCommand(_ => NewSession_Click(null, null)), newSessionGesture));
        //        InputBindings.Add(new KeyBinding(new RelayCommand(_ => OpenSession_Click(null, null)), openSessionGesture));
        //        InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSession_Click(null, null)), saveSessionGesture));
        //        InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSessionAs_Click(null, null)), saveAsGesture));
        //    }

        //    private async void InitializeApplication()
        //    {
        //        _trafficEntries = new ObservableCollection<TrafficEntry>();
        //        TrafficDataGrid.ItemsSource = _trafficEntries;

        //        _sessionService = new SessionService();
        //        _proxyService = new ProxyService();
        //        _proxyService.TrafficCaptured += OnTrafficCaptured;

        //        await StartProxyAsync();
        //        await InitializeWebViewAsync();

        //        UpdateRecentSessionsMenu();
        //        UpdateUI();
        //    }

        //    private async Task StartProxyAsync()
        //    {
        //        try
        //        {
        //            await _proxyService.StartProxyAsync();
        //            _isProxyStarted = true;
        //            Title = $"Web Traffic Inspector - Proxy Running on Port {_proxyService.ProxyPort}";
        //            StatusText.Text = $"Proxy running on port {_proxyService.ProxyPort}";
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Failed to start proxy: {ex.Message}", "Error",
        //                MessageBoxButton.OK, MessageBoxImage.Error);
        //            StatusText.Text = "Proxy failed to start";
        //        }
        //    }

        //    private async Task InitializeWebViewAsync()
        //    {
        //        try
        //        {
        //            var options = new CoreWebView2EnvironmentOptions();
        //            options.AdditionalBrowserArguments = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors --disable-web-security --allow-running-insecure-content";

        //            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
        //            await WebViewControl.EnsureCoreWebView2Async(environment);

        //            WebViewControl.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
        //            WebViewControl.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        //            WebViewControl.CoreWebView2.Settings.AreDevToolsEnabled = true;

        //            WebViewControl.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        //            WebViewControl.CoreWebView2.DOMContentLoaded += WebView_DOMContentLoaded;
        //            WebViewControl.CoreWebView2.NavigationStarting += WebView_NavigationStarting;

        //            WebViewControl.CoreWebView2.Navigate("https://httpbin.org/get");
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nMake sure WebView2 Runtime is installed.",
        //                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }

        //    private void OnTrafficCaptured(TrafficEntry entry)
        //    {
        //        Dispatcher.BeginInvoke(() =>
        //        {
        //            _trafficEntries.Add(entry);
        //            _hasUnsavedChanges = true;

        //            if (_trafficEntries.Count > 0)
        //            {
        //                TrafficDataGrid.ScrollIntoView(_trafficEntries[_trafficEntries.Count - 1]);
        //            }

        //            UpdateUI();
        //        });
        //    }

        //    private void NewSession_Click(object sender, RoutedEventArgs e)
        //    {
        //        if (_hasUnsavedChanges)
        //        {
        //            var result = MessageBox.Show("You have unsaved changes. Do you want to save the current session?",
        //                "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        //            if (result == MessageBoxResult.Yes)
        //            {
        //                SaveSession_Click(sender, e);
        //            }
        //            else if (result == MessageBoxResult.Cancel)
        //            {
        //                return;
        //            }
        //        }

        //        _trafficEntries.Clear();
        //        _currentSessionPath = null;
        //        _currentSessionName = "Untitled Session";
        //        _hasUnsavedChanges = false;

        //        RequestTextBox.Text = "";
        //        ResponseTextBox.Text = "";

        //        UpdateUI();
        //        StatusText.Text = "New session created";
        //    }

        //    private async void OpenSession_Click(object sender, RoutedEventArgs e)
        //    {
        //        var openFileDialog = new OpenFileDialog
        //        {
        //            Title = "Open Session",
        //            Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis|All files (*.*)|*.*",
        //            InitialDirectory = _sessionService.GetDefaultSessionsPath()
        //        };

        //        if (openFileDialog.ShowDialog() == true)
        //        {
        //            await LoadSessionAsync(openFileDialog.FileName);
        //        }
        //    }

        //    private async Task LoadSessionAsync(string filePath)
        //    {
        //        try
        //        {
        //            StatusText.Text = "Loading session...";

        //            var sessionData = await _sessionService.LoadSessionAsync(filePath);
        //            if (sessionData != null)
        //            {
        //                _trafficEntries.Clear();

        //                foreach (var entry in sessionData.TrafficEntries)
        //                {
        //                    _trafficEntries.Add(entry);
        //                }

        //                _currentSessionPath = filePath;
        //                _currentSessionName = sessionData.SessionName;
        //                _hasUnsavedChanges = false;

        //                UpdateUI();
        //                UpdateRecentSessionsMenu();

        //                StatusText.Text = $"Session loaded: {sessionData.TrafficEntries.Count} entries";

        //                MessageBox.Show($"Session loaded successfully!\n\nSession: {sessionData.SessionName}\nEntries: {sessionData.TrafficEntries.Count}\nCreated: {sessionData.CreatedDate:yyyy-MM-dd HH:mm}",
        //                    "Session Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Failed to load session: {ex.Message}", "Load Error",
        //                MessageBoxButton.OK, MessageBoxImage.Error);
        //            StatusText.Text = "Failed to load session";
        //        }
        //    }

        //    private async void SaveSession_Click(object sender, RoutedEventArgs e)
        //    {
        //        if (string.IsNullOrEmpty(_currentSessionPath))
        //        {
        //            SaveSessionAs_Click(sender, e);
        //            return;
        //        }

        //        await SaveCurrentSessionAsync(_currentSessionPath);
        //    }

        //    private async void SaveSessionAs_Click(object sender, RoutedEventArgs e)
        //    {
        //        var saveFileDialog = new SaveFileDialog
        //        {
        //            Title = "Save Session As",
        //            Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis",
        //            InitialDirectory = _sessionService.GetDefaultSessionsPath(),
        //            FileName = _currentSessionName.Replace(" ", "_")
        //        };

        //        if (saveFileDialog.ShowDialog() == true)
        //        {
        //            await SaveCurrentSessionAsync(saveFileDialog.FileName);
        //        }
        //    }

        //    private async Task SaveCurrentSessionAsync(string filePath)
        //    {
        //        try
        //        {
        //            StatusText.Text = "Saving session...";

        //            var sessionName = Path.GetFileNameWithoutExtension(filePath);
        //            var description = $"Session saved on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        //            var success = await _sessionService.SaveSessionAsync(filePath, _trafficEntries.ToList(), sessionName, description);

        //            if (success)
        //            {
        //                _currentSessionPath = filePath;
        //                _currentSessionName = sessionName;
        //                _hasUnsavedChanges = false;

        //                UpdateUI();
        //                UpdateRecentSessionsMenu();

        //                StatusText.Text = $"Session saved: {_trafficEntries.Count} entries";

        //                MessageBox.Show($"Session saved successfully!\n\nFile: {Path.GetFileName(filePath)}\nEntries: {_trafficEntries.Count}",
        //                    "Session Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Failed to save session: {ex.Message}", "Save Error",
        //                MessageBoxButton.OK, MessageBoxImage.Error);
        //            StatusText.Text = "Failed to save session";
        //        }
        //    }

        //    private void UpdateRecentSessionsMenu()
        //    {
        //        RecentSessionsMenu.Items.Clear();

        //        var recentSessions = _sessionService.GetRecentSessions();

        //        if (recentSessions.Any())
        //        {
        //            foreach (var sessionPath in recentSessions)
        //            {
        //                var menuItem = new MenuItem
        //                {
        //                    Header = Path.GetFileNameWithoutExtension(sessionPath),
        //                    Tag = sessionPath
        //                };
        //                menuItem.Click += async (s, e) => await LoadSessionAsync(sessionPath);
        //                RecentSessionsMenu.Items.Add(menuItem);
        //            }
        //        }
        //        else
        //        {
        //            var noRecentItem = new MenuItem
        //            {
        //                Header = "No recent sessions",
        //                IsEnabled = false
        //            };
        //            RecentSessionsMenu.Items.Add(noRecentItem);
        //        }
        //    }

        //    private void UpdateUI()
        //    {
        //        SessionNameText.Text = _currentSessionName + (_hasUnsavedChanges ? "*" : "");
        //        TrafficCountText.Text = $"Traffic: {_trafficEntries.Count}";

        //        Title = $"Web Traffic Inspector - {_currentSessionName}" +
        //               (_hasUnsavedChanges ? "*" : "") +
        //               (_isProxyStarted ? $" (Proxy: {_proxyService.ProxyPort})" : "");
        //    }

        //    private void SessionInfo_Click(object sender, RoutedEventArgs e)
        //    {
        //        var info = $"Session Information\n\n" +
        //                  $"Name: {_currentSessionName}\n" +
        //                  $"File: {_currentSessionPath ?? "Not saved"}\n" +
        //                  $"Total Entries: {_trafficEntries.Count}\n" +
        //                  $"Unsaved Changes: {(_hasUnsavedChanges ? "Yes" : "No")}\n\n" +
        //                  $"Methods:\n";

        //        var methods = _trafficEntries.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count());
        //        foreach (var method in methods)
        //        {
        //            info += $"  {method.Key}: {method.Value}\n";
        //        }

        //        info += "\nStatus Codes:\n";
        //        var statuses = _trafficEntries.Where(e => e.Status > 0).GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count());
        //        foreach (var status in statuses)
        //        {
        //            info += $"  {status.Key}: {status.Value}\n";
        //        }

        //        MessageBox.Show(info, "Session Information", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }

        //    private void ExportTraffic_Click(object sender, RoutedEventArgs e)
        //    {
        //        MessageBox.Show("Export functionality coming soon!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }

        //    private void ImportTraffic_Click(object sender, RoutedEventArgs e)
        //    {
        //        MessageBox.Show("Import functionality coming soon!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }

        //    private void Exit_Click(object sender, RoutedEventArgs e)
        //    {
        //        Close();
        //    }

        //    private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Navigation starting to: {e.Uri}");
        //    }

        //    private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            UrlTextBox.Text = WebViewControl.CoreWebView2.Source;
        //        });
        //        System.Diagnostics.Debug.WriteLine($"Navigation completed: {e.IsSuccess}");
        //    }

        //    private void WebView_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        //    {
        //        System.Diagnostics.Debug.WriteLine("DOM content loaded");
        //    }

        //    private void NavigateButton_Click(object sender, RoutedEventArgs e)
        //    {
        //        NavigateToUrl();
        //    }

        //    private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        //    {
        //        if (e.Key == Key.Enter)
        //        {
        //            NavigateToUrl();
        //        }
        //    }

        //    private void NavigateToUrl()
        //    {
        //        try
        //        {
        //            var url = UrlTextBox.Text.Trim();
        //            if (!string.IsNullOrEmpty(url))
        //            {
        //                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        //                {
        //                    url = "https://" + url;
        //                }

        //                WebViewControl.CoreWebView2?.Navigate(url);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Navigation error: {ex.Message}", "Error",
        //                MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }

        //    private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        //    {
        //        try
        //        {
        //            var url = UrlTextBox.Text.Trim();
        //            if (string.IsNullOrEmpty(url))
        //                url = "https://httpbin.org/get";

        //            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        //            {
        //                url = "https://" + url;
        //            }

        //            var chromeArgs = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors --new-window \"{url}\"";

        //            try
        //            {
        //                var chromeProcess = new ProcessStartInfo
        //                {
        //                    FileName = "chrome.exe",
        //                    Arguments = chromeArgs,
        //                    UseShellExecute = true
        //                };
        //                Process.Start(chromeProcess);

        //                MessageBox.Show($"Chrome opened with proxy settings.\nProxy: 127.0.0.1:{_proxyService.ProxyPort}\nAll traffic from this browser window will be captured.",
        //                    "Browser Opened", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }
        //            catch
        //            {
        //                try
        //                {
        //                    var edgeArgs = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors \"{url}\"";
        //                    var edgeProcess = new ProcessStartInfo
        //                    {
        //                        FileName = "msedge.exe",
        //                        Arguments = edgeArgs,
        //                        UseShellExecute = true
        //                    };
        //                    Process.Start(edgeProcess);

        //                    MessageBox.Show($"Edge opened with proxy settings.\nProxy: 127.0.0.1:{_proxyService.ProxyPort}\nAll traffic from this browser window will be captured.",
        //                        "Browser Opened", MessageBoxButton.OK, MessageBoxImage.Information);
        //                }
        //                catch
        //                {
        //                    MessageBox.Show($"Could not automatically configure browser.\n\nManually configure your browser proxy settings:\nHTTP Proxy: 127.0.0.1:{_proxyService.ProxyPort}\nHTTPS Proxy: 127.0.0.1:{_proxyService.ProxyPort}\n\nThen navigate to: {url}",
        //                        "Manual Configuration Required", MessageBoxButton.OK, MessageBoxImage.Information);

        //                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Failed to open browser: {ex.Message}", "Error",
        //                MessageBoxButton.OK, MessageBoxImage.Warning);
        //        }
        //    }

        //    private void TrafficDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //    {
        //        if (TrafficDataGrid.SelectedItem is TrafficEntry selectedEntry)
        //        {
        //            RequestTextBox.Text = selectedEntry.RawRequest ?? "No request data available";
        //            ResponseTextBox.Text = selectedEntry.RawResponse ?? "No response data available";
        //        }
        //        else
        //        {
        //            RequestTextBox.Text = "";
        //            ResponseTextBox.Text = "";
        //        }
        //    }

        //    private void ClearButton_Click(object sender, RoutedEventArgs e)
        //    {
        //        if (_trafficEntries.Count > 0)
        //        {
        //            var result = MessageBox.Show("Are you sure you want to clear all captured traffic?",
        //                "Clear Traffic", MessageBoxButton.YesNo, MessageBoxImage.Question);

        //            if (result == MessageBoxResult.Yes)
        //            {
        //                _trafficEntries.Clear();
        //                RequestTextBox.Text = "";
        //                ResponseTextBox.Text = "";
        //                _hasUnsavedChanges = true;
        //                UpdateUI();
        //            }
        //        }
        //    }

        //    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        //    {
        //        if (_hasUnsavedChanges)
        //        {
        //            var result = MessageBox.Show("You have unsaved changes. Do you want to save before closing?",
        //                "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        //            if (result == MessageBoxResult.Yes)
        //            {
        //                SaveSession_Click(null, null);
        //            }
        //            else if (result == MessageBoxResult.Cancel)
        //            {
        //                e.Cancel = true;
        //                return;
        //            }
        //        }

        //        _proxyService?.StopProxy();
        //        base.OnClosing(e);
        //    }

        //    protected override void OnClosed(EventArgs e)
        //    {
        //        _proxyService?.StopProxy();
        //        base.OnClosed(e);
        //    }

        //    private void ToggleTrafficLog_Click(object sender, RoutedEventArgs e)
        //    {
        //        var menuItem = sender as MenuItem;
        //        var trafficLogRow = MainGrid.RowDefinitions[3]; // Adjust index based on your grid

        //        if (menuItem.IsChecked)
        //        {
        //            trafficLogRow.Height = new GridLength(2, GridUnitType.Star);
        //        }
        //        else
        //        {
        //            trafficLogRow.Height = new GridLength(0);
        //        }
        //    }

        //}

        //// Helper class for keyboard shortcuts
        //public class RelayCommand : ICommand
        //{
        //    private readonly Action<object> _execute;
        //    private readonly Action _executeParameterless;
        //    private readonly Func<object, bool> _canExecute;

        //    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        //    {
        //        _execute = execute;
        //        _canExecute = canExecute;
        //    }

        //    public RelayCommand(Action execute, Func<bool> canExecute = null)
        //    {
        //        _executeParameterless = execute;
        //        _canExecute = _ => canExecute?.Invoke() ?? true;
        //    }

        //    public event EventHandler CanExecuteChanged
        //    {
        //        add { CommandManager.RequerySuggested += value; }
        //        remove { CommandManager.RequerySuggested -= value; }
        //    }

        //    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        //    public void Execute(object parameter)
        //    {
        //        _execute?.Invoke(parameter);
        //        _executeParameterless?.Invoke();
        //    }
        //}

        private ProxyService _proxyService;
        private SessionService _sessionService;
        private MoveService _moveService;
        private UndoRedoService _undoRedoService;
        private ClipboardService _clipboardService;
        private RequestReplayerService _requestReplayerService;
        private EncodingService _encodingService;
        private SecurityAnalyzerService _securityAnalyzerService;
        private ExportService _exportService;
        private PatternMatcherService _patternMatcherService;
        private ComparisonService _comparisonService;
        private AdvancedFilterService _advancedFilterService;
        private InterceptService _interceptService;
        private VisualizationService _visualizationService;
        private CookieManagerService _cookieManagerService;
        private ResponseValidatorService _responseValidatorService;
        private AttackSurfaceMapperService _attackSurfaceMapperService;
        private AutoResponderService _autoResponderService;
        private MacroRecorderService _macroRecorderService;
        private AutoAttackModeService _autoAttackService;
        private WebSocketInterceptorService _webSocketService;
        private GraphQLAnalyzerService _graphQLService;
        private IntruderService _intruderService;
        private JWTManipulationService _jwtService;
        private AdvancedReportGeneratorService _advancedReportService;
        private OAuthOIDCVulnerabilityScannerService _oauthScanner;
        private PrivilegeEscalationScannerService _privEscScanner;
        private ObservableCollection<TrafficEntry> _trafficEntries;
        private ObservableCollection<TrafficEntry> _filteredTrafficEntries;
        private bool _isProxyStarted = false;
        private string _currentSessionPath = null;
        private string _currentSessionName = "Untitled Session";
        private bool _hasUnsavedChanges = false;

        // Drag and drop support
        private Point _dragStartPoint;
        private TrafficEntry _draggedEntry = null;
        //private Grid MainGrid;

        public MainWindow()
        {
            InitializeComponent();
            //MainGrid = (Grid)this.Content;
            //MainGrid = FindGridInContent();
            InitializeApplication();
            SetupKeyboardShortcuts();
        }

        private Grid FindGridInContent()
        {
            // Method 1: Find by name if your Grid has a Name attribute
            var grid = this.FindName("MainGrid") as Grid;
            if (grid != null) return grid;

            // Method 2: Search through the visual tree
            if (this.Content is DockPanel dockPanel)
            {
                foreach (var child in dockPanel.Children)
                {
                    if (child is Grid foundGrid)
                        return foundGrid;
                }
            }

            // Method 3: Create a new Grid if none found (fallback)
            return new Grid();
        }

        private void SetupKeyboardShortcuts()
        {
            // File operations
            var newSessionGesture = new KeyGesture(Key.N, ModifierKeys.Control);
            var openSessionGesture = new KeyGesture(Key.O, ModifierKeys.Control);
            var saveSessionGesture = new KeyGesture(Key.S, ModifierKeys.Control);
            var saveAsGesture = new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => NewSession_Click(null, null)), newSessionGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => OpenSession_Click(null, null)), openSessionGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSession_Click(null, null)), saveSessionGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSessionAs_Click(null, null)), saveAsGesture));

            // Edit operations
            var undoGesture = new KeyGesture(Key.Z, ModifierKeys.Control);
            var redoGesture = new KeyGesture(Key.Y, ModifierKeys.Control);
            var cutGesture = new KeyGesture(Key.X, ModifierKeys.Control);
            var copyGesture = new KeyGesture(Key.C, ModifierKeys.Control);
            var pasteGesture = new KeyGesture(Key.V, ModifierKeys.Control);
            var selectAllGesture = new KeyGesture(Key.A, ModifierKeys.Control);

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Undo_Click(null, null)), undoGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Redo_Click(null, null)), redoGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Cut_Click(null, null)), cutGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Copy_Click(null, null)), copyGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => Paste_Click(null, null)), pasteGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => SelectAll_Click(null, null)), selectAllGesture));

            // Move operations
            var moveUpGesture = new KeyGesture(Key.Up, ModifierKeys.Control);
            var moveDownGesture = new KeyGesture(Key.Down, ModifierKeys.Control);
            var moveToTopGesture = new KeyGesture(Key.Home, ModifierKeys.Control);
            var moveToBottomGesture = new KeyGesture(Key.End, ModifierKeys.Control);
            var deleteGesture = new KeyGesture(Key.Delete, ModifierKeys.None);

            InputBindings.Add(new KeyBinding(new RelayCommand(_ => MoveUp_Click(null, null)), moveUpGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => MoveDown_Click(null, null)), moveDownGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => MoveToTop_Click(null, null)), moveToTopGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => MoveToBottom_Click(null, null)), moveToBottomGesture));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => DeleteSelected_Click(null, null)), deleteGesture));
        }

        private async void InitializeApplication()
        {
            _trafficEntries = new ObservableCollection<TrafficEntry>();
            _filteredTrafficEntries = new ObservableCollection<TrafficEntry>();
            TrafficDataGrid.ItemsSource = _filteredTrafficEntries;

            _sessionService = new SessionService();
            _moveService = new MoveService();
            _undoRedoService = new UndoRedoService();
            _clipboardService = new ClipboardService();
            _requestReplayerService = new RequestReplayerService();
            _encodingService = new EncodingService();
            _securityAnalyzerService = new SecurityAnalyzerService();
            _exportService = new ExportService();
            _patternMatcherService = new PatternMatcherService();
            _comparisonService = new ComparisonService();
            _advancedFilterService = new AdvancedFilterService();
            _interceptService = new InterceptService();
            _visualizationService = new VisualizationService();
            _cookieManagerService = new CookieManagerService();
            _responseValidatorService = new ResponseValidatorService();
            _attackSurfaceMapperService = new AttackSurfaceMapperService();
            _autoResponderService = new AutoResponderService();
            _macroRecorderService = new MacroRecorderService();
            _autoAttackService = new AutoAttackModeService();
            _webSocketService = new WebSocketInterceptorService();
            _graphQLService = new GraphQLAnalyzerService();
            _intruderService = new IntruderService();
            _jwtService = new JWTManipulationService();
            _advancedReportService = new AdvancedReportGeneratorService();
            _oauthScanner = new OAuthOIDCVulnerabilityScannerService();
            _privEscScanner = new PrivilegeEscalationScannerService();
            _proxyService = new ProxyService();
            _proxyService.TrafficCaptured += OnTrafficCaptured;

            // Setup OAuth/OIDC detection events
            _oauthScanner.OAuthFlowDetected += OnOAuthFlowDetected;
            _oauthScanner.VulnerabilityFound += OnOAuthVulnerabilityFound;

            await StartProxyAsync();
            await InitializeWebViewAsync();

            UpdateRecentSessionsMenu();
            UpdateUI();
        }

        private async Task StartProxyAsync()
        {
            try
            {
                await _proxyService.StartProxyAsync();
                _isProxyStarted = true;
                Title = $"Web Traffic Inspector - Proxy Running on Port {_proxyService.ProxyPort}";
                StatusText.Text = $"Proxy running on port {_proxyService.ProxyPort}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start proxy: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Proxy failed to start";
            }
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                //    var options = new CoreWebView2EnvironmentOptions();
                //    options.AdditionalBrowserArguments = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors --disable-web-security --allow-running-insecure-content";

                //    var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                //    await WebViewControl.EnsureCoreWebView2Async(environment);

                //    WebViewControl.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                //    WebViewControl.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                //    WebViewControl.CoreWebView2.Settings.AreDevToolsEnabled = true;

                //    WebViewControl.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
                //    WebViewControl.CoreWebView2.DOMContentLoaded += WebView_DOMContentLoaded;
                //    WebViewControl.CoreWebView2.NavigationStarting += WebView_NavigationStarting;

                //    WebViewControl.CoreWebView2.Navigate("https://httpbin.org/get");
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\nMake sure WebView2 Runtime is installed.",
                //        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}

                var options = new CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors --disable-web-security --allow-running-insecure-content";

                var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                await WebViewControl.EnsureCoreWebView2Async(environment);

                // Configure settings
                WebViewControl.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                WebViewControl.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                WebViewControl.CoreWebView2.Settings.AreDevToolsEnabled = true;

                // Load your custom start page
                //var startPagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StartPage.html");
                //WebViewControl.CoreWebView2.Navigate($"file:///{startPagePath}");
                WebViewControl.CoreWebView2.Navigate("https://httpbin.org/get");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize WebView2: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnTrafficCaptured(TrafficEntry entry)
        {
            Dispatcher.BeginInvoke(() =>
            {
                _trafficEntries.Add(entry);
                _filteredTrafficEntries.Add(entry);
                _hasUnsavedChanges = true;

                if (_filteredTrafficEntries.Count > 0)
                {
                    TrafficDataGrid.ScrollIntoView(_filteredTrafficEntries[_filteredTrafficEntries.Count - 1]);
                }

                // Automatically detect OAuth/OIDC flows
                if (_oauthScanner != null)
                {
                    _ = Task.Run(() =>
                    {
                        var oauthFlow = _oauthScanner.DetectOAuthFlow(entry);
                        if (oauthFlow != null)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                StatusText.Text = $"OAuth flow detected: {oauthFlow.FlowType}";
                            });
                        }
                    });
                }

                // Process through Auto Attack Mode if enabled
                if (_autoAttackService != null && _autoAttackService.IsEnabled)
                {
                    _ = Task.Run(async () =>
                    {
                        await _autoAttackService.ProcessTrafficEntry(entry);
                    });
                }

                UpdateUI();
            });
        }

        #region File Menu Handlers

        private void NewSession_Click(object sender, RoutedEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save the current session?",
                    "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSession_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            _trafficEntries.Clear();
            _filteredTrafficEntries.Clear();
            _currentSessionPath = null;
            _currentSessionName = "Untitled Session";
            _hasUnsavedChanges = false;

            RequestTextBox.Text = "";
            ResponseTextBox.Text = "";

            UpdateUI();
            StatusText.Text = "New session created";
        }

        private async void OpenSession_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Session",
                Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis|All files (*.*)|*.*",
                InitialDirectory = _sessionService.GetDefaultSessionsPath()
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadSessionAsync(openFileDialog.FileName);
            }
        }

        private async Task LoadSessionAsync(string filePath)
        {
            try
            {
                StatusText.Text = "Loading session...";

                var sessionData = await _sessionService.LoadSessionAsync(filePath);
                if (sessionData != null)
                {
                    _trafficEntries.Clear();
                    _filteredTrafficEntries.Clear();

                    foreach (var entry in sessionData.TrafficEntries)
                    {
                        _trafficEntries.Add(entry);
                        _filteredTrafficEntries.Add(entry);
                    }

                    _currentSessionPath = filePath;
                    _currentSessionName = sessionData.SessionName;
                    _hasUnsavedChanges = false;

                    UpdateUI();
                    UpdateRecentSessionsMenu();

                    StatusText.Text = $"Session loaded: {sessionData.TrafficEntries.Count} entries";

                    MessageBox.Show($"Session loaded successfully!\n\nSession: {sessionData.SessionName}\nEntries: {sessionData.TrafficEntries.Count}\nCreated: {sessionData.CreatedDate:yyyy-MM-dd HH:mm}",
                        "Session Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load session: {ex.Message}", "Load Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to load session";
            }
        }

        private async void SaveSession_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentSessionPath))
            {
                SaveSessionAs_Click(sender, e);
                return;
            }

            await SaveCurrentSessionAsync(_currentSessionPath);
        }

        private async void SaveSessionAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Session As",
                Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis",
                InitialDirectory = _sessionService.GetDefaultSessionsPath(),
                FileName = _currentSessionName.Replace(" ", "_")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await SaveCurrentSessionAsync(saveFileDialog.FileName);
            }
        }

        private async Task SaveCurrentSessionAsync(string filePath)
        {
            try
            {
                StatusText.Text = "Saving session...";

                var sessionName = Path.GetFileNameWithoutExtension(filePath);
                var description = $"Session saved on {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                var success = await _sessionService.SaveSessionAsync(filePath, _trafficEntries.ToList(), sessionName, description);

                if (success)
                {
                    _currentSessionPath = filePath;
                    _currentSessionName = sessionName;
                    _hasUnsavedChanges = false;

                    UpdateUI();
                    UpdateRecentSessionsMenu();

                    StatusText.Text = $"Session saved: {_trafficEntries.Count} entries";

                    MessageBox.Show($"Session saved successfully!\n\nFile: {Path.GetFileName(filePath)}\nEntries: {_trafficEntries.Count}",
                        "Session Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save session: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to save session";
            }
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to JSON functionality coming soon!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to CSV functionality coming soon!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportXml_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export to XML functionality coming soon!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportJson_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Import from JSON functionality coming soon!", "Import",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportBurp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Import from Burp Suite functionality coming soon!", "Import",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportFiddler_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Import from Fiddler functionality coming soon!", "Import",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region View Menu Handlers

        private void ToggleTrafficLog_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var trafficLogRow = MainGrid.RowDefinitions[3];

            if (menuItem.IsChecked)
            {
                trafficLogRow.Height = new GridLength(2, GridUnitType.Star);
            }
            else
            {
                trafficLogRow.Height = new GridLength(0);
            }
        }

        private void ToggleRequestResponse_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var requestResponseRow = MainGrid.RowDefinitions[5];

            if (menuItem.IsChecked)
            {
                requestResponseRow.Height = new GridLength(1.5, GridUnitType.Star);
            }
            else
            {
                requestResponseRow.Height = new GridLength(0);
            }
        }

        private void ToggleBrowser_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var browserRow = MainGrid.RowDefinitions[1];

            if (menuItem.IsChecked)
            {
                browserRow.Height = new GridLength(2, GridUnitType.Star);
            }
            else
            {
                browserRow.Height = new GridLength(0);
            }
        }

        private void FilterAll_Click(object sender, RoutedEventArgs e)
        {
            _filteredTrafficEntries.Clear();
            foreach (var entry in _trafficEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }
            UpdateFilterMenuItems(sender as MenuItem);
        }

        private void FilterHttp_Click(object sender, RoutedEventArgs e)
        {
            _filteredTrafficEntries.Clear();
            var filteredEntries = _trafficEntries.Where(entry =>
                !entry.Host.Contains("443") && entry.Host.Contains("80"));
            foreach (var entry in filteredEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }
            UpdateFilterMenuItems(sender as MenuItem);
        }

        private void FilterHttps_Click(object sender, RoutedEventArgs e)
        {
            _filteredTrafficEntries.Clear();
            var filteredEntries = _trafficEntries.Where(entry =>
                entry.Host.Contains("443") || entry.Path.StartsWith("https"));
            foreach (var entry in filteredEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }
            UpdateFilterMenuItems(sender as MenuItem);
        }

        private void FilterErrors_Click(object sender, RoutedEventArgs e)
        {
            _filteredTrafficEntries.Clear();
            var filteredEntries = _trafficEntries.Where(entry => entry.Status >= 400);
            foreach (var entry in filteredEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }
            UpdateFilterMenuItems(sender as MenuItem);
        }

        private void UpdateFilterMenuItems(MenuItem selectedItem)
        {
            // Update filter menu item states - implementation depends on your menu structure
        }

        private void ToggleMethodColumn_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var methodColumn = TrafficDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Method");
            if (methodColumn != null)
            {
                methodColumn.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleStatusColumn_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var statusColumn = TrafficDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Status");
            if (statusColumn != null)
            {
                statusColumn.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleLengthColumn_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var lengthColumn = TrafficDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Length");
            if (lengthColumn != null)
            {
                lengthColumn.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ToggleTimeColumn_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var timeColumn = TrafficDataGrid.Columns.FirstOrDefault(c => c.Header.ToString() == "Time");
            if (timeColumn != null)
            {
                timeColumn.Visibility = menuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void SetLightTheme_Click(object sender, RoutedEventArgs e)
        {
            // Implement light theme
            Application.Current.Resources.MergedDictionaries.Clear();
            UpdateThemeMenuItems(sender as MenuItem);
        }

        private void SetDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            // Implement dark theme
            UpdateThemeMenuItems(sender as MenuItem);
        }

        private void SetAutoTheme_Click(object sender, RoutedEventArgs e)
        {
            // Implement auto theme based on system
            UpdateThemeMenuItems(sender as MenuItem);
        }

        private void UpdateThemeMenuItems(MenuItem selectedItem)
        {
            // Update theme menu item states
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            var currentZoom = this.LayoutTransform as System.Windows.Media.ScaleTransform;
            if (currentZoom == null)
            {
                currentZoom = new System.Windows.Media.ScaleTransform(1.1, 1.1);
                this.LayoutTransform = currentZoom;
            }
            else
            {
                currentZoom.ScaleX = Math.Min(currentZoom.ScaleX * 1.1, 2.0);
                currentZoom.ScaleY = Math.Min(currentZoom.ScaleY * 1.1, 2.0);
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            var currentZoom = this.LayoutTransform as System.Windows.Media.ScaleTransform;
            if (currentZoom != null)
            {
                currentZoom.ScaleX = Math.Max(currentZoom.ScaleX / 1.1, 0.5);
                currentZoom.ScaleY = Math.Max(currentZoom.ScaleY / 1.1, 0.5);
            }
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            this.LayoutTransform = null;
        }

        private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        private void ToggleAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            Topmost = menuItem.IsChecked;
        }

        #endregion

        #region Tools Menu Handlers

        private void ProxySettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Proxy Settings window coming soon!", "Proxy Settings",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CertificateManager_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Certificate Manager window coming soon!", "Certificate Manager",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchTraffic_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Search Traffic window coming soon!", "Search Traffic",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AdvancedFilter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Advanced Filter window coming soon!", "Advanced Filter",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterAll_Click(sender, e);
        }

        private void RequestBuilder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Request Builder window coming soon!", "Request Builder",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResponseAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Response Analyzer window coming soon!", "Response Analyzer",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TrafficStats_Click(object sender, RoutedEventArgs e)
        {
            var stats = $"Traffic Statistics\n\n" +
                       $"Total Entries: {_trafficEntries.Count}\n" +
                       $"Filtered Entries: {_filteredTrafficEntries.Count}\n\n" +
                       $"Methods:\n";

            var methods = _trafficEntries.GroupBy(e => e.Method).ToDictionary(g => g.Key, g => g.Count());
            foreach (var method in methods)
            {
                stats += $"  {method.Key}: {method.Value}\n";
            }

            stats += "\nStatus Codes:\n";
            var statuses = _trafficEntries.Where(e => e.Status > 0).GroupBy(e => e.Status).ToDictionary(g => g.Key, g => g.Count());
            foreach (var status in statuses)
            {
                stats += $"  {status.Key}: {status.Value}\n";
            }

            MessageBox.Show(stats, "Traffic Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Options window coming soon!", "Options",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Move and Organize Handlers

        // Basic Move Operations
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No entries selected";
                return;
            }

            if (selectedItems.Count == 1)
            {
                if (_moveService.MoveUp(_filteredTrafficEntries, selectedItems[0]))
                {
                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = "Entry moved up";
                }
                else
                {
                    StatusText.Text = "Cannot move entry up";
                }
            }
            else
            {
                if (_moveService.MoveBatchUp(_filteredTrafficEntries, selectedItems))
                {
                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = $"{selectedItems.Count} entries moved up";
                }
                else
                {
                    StatusText.Text = "Cannot move selected entries up";
                }
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No entries selected";
                return;
            }

            if (selectedItems.Count == 1)
            {
                if (_moveService.MoveDown(_filteredTrafficEntries, selectedItems[0]))
                {
                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = "Entry moved down";
                }
                else
                {
                    StatusText.Text = "Cannot move entry down";
                }
            }
            else
            {
                if (_moveService.MoveBatchDown(_filteredTrafficEntries, selectedItems))
                {
                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = $"{selectedItems.Count} entries moved down";
                }
                else
                {
                    StatusText.Text = "Cannot move selected entries down";
                }
            }
        }

        private void MoveToTop_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                StatusText.Text = "No entry selected";
                return;
            }

            if (_moveService.MoveToTop(_filteredTrafficEntries, selectedEntry))
            {
                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = "Entry moved to top";
            }
            else
            {
                StatusText.Text = "Cannot move entry to top";
            }
        }

        private void MoveToBottom_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                StatusText.Text = "No entry selected";
                return;
            }

            if (_moveService.MoveToBottom(_filteredTrafficEntries, selectedEntry))
            {
                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = "Entry moved to bottom";
            }
            else
            {
                StatusText.Text = "Cannot move entry to bottom";
            }
        }

        private void MoveToPosition_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select an entry to move.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter target position (1 to {_filteredTrafficEntries.Count}):",
                "Move to Position",
                "1");

            if (int.TryParse(input, out int position) && position >= 1 && position <= _filteredTrafficEntries.Count)
            {
                if (_moveService.MoveToPosition(_filteredTrafficEntries, selectedEntry, position - 1))
                {
                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = $"Entry moved to position {position}";
                }
            }
            else
            {
                MessageBox.Show("Invalid position. Please enter a number between 1 and " + _filteredTrafficEntries.Count,
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Reorder Operations
        private void SortByTimeAsc_Click(object sender, RoutedEventArgs e)
        {
            _moveService.SortByTime(_filteredTrafficEntries, true);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries sorted by time (ascending)";
        }

        private void SortByTimeDesc_Click(object sender, RoutedEventArgs e)
        {
            _moveService.SortByTime(_filteredTrafficEntries, false);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries sorted by time (descending)";
        }

        private void GroupByHost_Click(object sender, RoutedEventArgs e)
        {
            _moveService.GroupByHost(_filteredTrafficEntries);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries grouped by host";
        }

        private void GroupByMethod_Click(object sender, RoutedEventArgs e)
        {
            _moveService.GroupByMethod(_filteredTrafficEntries);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries grouped by method";
        }

        private void GroupByStatus_Click(object sender, RoutedEventArgs e)
        {
            _moveService.GroupByStatus(_filteredTrafficEntries);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries grouped by status code";
        }

        private void ReverseOrder_Click(object sender, RoutedEventArgs e)
        {
            _moveService.ReverseOrder(_filteredTrafficEntries);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Entries order reversed";
        }

        // Smart Move Operations
        private void MoveByHost_Click(object sender, RoutedEventArgs e)
        {
            var host = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter host name or pattern to move:",
                "Move by Host",
                "");

            if (string.IsNullOrWhiteSpace(host))
                return;

            var newCollection = new ObservableCollection<TrafficEntry>();
            var movedEntries = _moveService.MoveEntriesByHost(_filteredTrafficEntries, newCollection, host, true);

            if (movedEntries.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Found {movedEntries.Count} entries matching '{host}'.\n\nDo you want to save them to a new session?",
                    "Move by Host",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveExtractedEntries(movedEntries, $"Host_{host}");
                }

                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = $"{movedEntries.Count} entries moved by host filter";
            }
            else
            {
                MessageBox.Show($"No entries found matching '{host}'",
                    "Move by Host", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MoveByStatus_Click(object sender, RoutedEventArgs e)
        {
            var statusInput = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter status code to move (e.g., 200, 404, 500):",
                "Move by Status Code",
                "");

            if (int.TryParse(statusInput, out int statusCode))
            {
                var newCollection = new ObservableCollection<TrafficEntry>();
                var movedEntries = _moveService.MoveEntriesByStatus(_filteredTrafficEntries, newCollection, statusCode, true);

                if (movedEntries.Count > 0)
                {
                    var result = MessageBox.Show(
                        $"Found {movedEntries.Count} entries with status code {statusCode}.\n\nDo you want to save them to a new session?",
                        "Move by Status",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveExtractedEntries(movedEntries, $"Status_{statusCode}");
                    }

                    SyncFilteredToMain();
                    _hasUnsavedChanges = true;
                    UpdateUI();
                    StatusText.Text = $"{movedEntries.Count} entries moved by status code";
                }
                else
                {
                    MessageBox.Show($"No entries found with status code {statusCode}",
                        "Move by Status", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void MoveByMethod_Click(object sender, RoutedEventArgs e)
        {
            var method = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter HTTP method to move (GET, POST, PUT, DELETE, etc.):",
                "Move by Method",
                "GET");

            if (string.IsNullOrWhiteSpace(method))
                return;

            var newCollection = new ObservableCollection<TrafficEntry>();
            var movedEntries = _moveService.MoveEntriesByMethod(_filteredTrafficEntries, newCollection, method, true);

            if (movedEntries.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Found {movedEntries.Count} entries with method '{method}'.\n\nDo you want to save them to a new session?",
                    "Move by Method",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveExtractedEntries(movedEntries, $"Method_{method}");
                }

                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = $"{movedEntries.Count} entries moved by method filter";
            }
            else
            {
                MessageBox.Show($"No entries found with method '{method}'",
                    "Move by Method", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MoveByTimeRange_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Time range move functionality coming soon!\n\nThis will allow you to move all entries within a specific time range to a new session.",
                "Move by Time Range", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Extract and Session Management
        private void ExtractToSession_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries to extract.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sessionName = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter name for new session ({selectedItems.Count} entries):",
                "Extract to New Session",
                "Extracted_Session");

            if (string.IsNullOrWhiteSpace(sessionName))
                return;

            var result = MessageBox.Show(
                "Do you want to remove the selected entries from the current session?",
                "Extract Entries",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            bool removeFromSource = result == MessageBoxResult.Yes;
            var extractedEntries = _moveService.ExtractEntries(_filteredTrafficEntries, selectedItems, removeFromSource);

            if (removeFromSource)
            {
                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
            }

            SaveExtractedEntries(extractedEntries, sessionName);
        }

        // Context Menu Handlers
        private void CopyRequest_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry != null && !string.IsNullOrEmpty(selectedEntry.RawRequest))
            {
                Clipboard.SetText(selectedEntry.RawRequest);
                StatusText.Text = "Request copied to clipboard";
            }
        }

        private void CopyResponse_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry != null && !string.IsNullOrEmpty(selectedEntry.RawResponse))
            {
                Clipboard.SetText(selectedEntry.RawResponse);
                StatusText.Text = "Response copied to clipboard";
            }
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry != null)
            {
                var url = $"{selectedEntry.Host}{selectedEntry.Path}";
                Clipboard.SetText(url);
                StatusText.Text = "URL copied to clipboard";
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No entries selected";
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedItems.Count} selected entry/entries?",
                "Delete Entries",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var entry in selectedItems)
                {
                    _filteredTrafficEntries.Remove(entry);
                }

                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = $"{selectedItems.Count} entries deleted";
            }
        }

        // Drag and Drop Support
        private void TrafficDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null)
            {
                _draggedEntry = row.Item as TrafficEntry;
            }
        }

        private void TrafficDataGrid_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TrafficEntry)))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void TrafficDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TrafficEntry)))
            {
                var droppedEntry = e.Data.GetData(typeof(TrafficEntry)) as TrafficEntry;
                var target = ((FrameworkElement)e.OriginalSource).DataContext as TrafficEntry;

                if (droppedEntry != null && target != null && droppedEntry != target)
                {
                    int sourceIndex = _filteredTrafficEntries.IndexOf(droppedEntry);
                    int targetIndex = _filteredTrafficEntries.IndexOf(target);

                    if (sourceIndex >= 0 && targetIndex >= 0)
                    {
                        _filteredTrafficEntries.Move(sourceIndex, targetIndex);
                        _moveService.ReassignIds(_filteredTrafficEntries);
                        SyncFilteredToMain();
                        _hasUnsavedChanges = true;
                        UpdateUI();
                        StatusText.Text = "Entry moved via drag and drop";
                    }
                }
            }
        }

        // Helper Methods
        private void SyncFilteredToMain()
        {
            // Synchronize filtered entries back to main collection
            // This ensures both collections stay in sync after move operations
            _trafficEntries.Clear();
            foreach (var entry in _filteredTrafficEntries)
            {
                _trafficEntries.Add(entry);
            }
        }

        private async void SaveExtractedEntries(List<TrafficEntry> entries, string sessionName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Extracted Session",
                Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis",
                InitialDirectory = _sessionService.GetDefaultSessionsPath(),
                FileName = sessionName.Replace(" ", "_")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var description = $"Extracted session with {entries.Count} entries";
                var success = await _sessionService.SaveSessionAsync(
                    saveFileDialog.FileName,
                    entries,
                    Path.GetFileNameWithoutExtension(saveFileDialog.FileName),
                    description);

                if (success)
                {
                    MessageBox.Show(
                        $"Session saved successfully!\n\nFile: {Path.GetFileName(saveFileDialog.FileName)}\nEntries: {entries.Count}",
                        "Session Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    StatusText.Text = $"Extracted session saved: {entries.Count} entries";
                }
            }
        }

        #endregion

        #region Edit Menu Handlers

        // Undo/Redo
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoRedoService.CanUndo)
            {
                _undoRedoService.Undo(_filteredTrafficEntries);
                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = $"Undone: {_undoRedoService.GetUndoDescription()}";
            }
            else
            {
                StatusText.Text = "Nothing to undo";
            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_undoRedoService.CanRedo)
            {
                _undoRedoService.Redo(_filteredTrafficEntries);
                SyncFilteredToMain();
                _hasUnsavedChanges = true;
                UpdateUI();
                StatusText.Text = $"Redone: {_undoRedoService.GetRedoDescription()}";
            }
            else
            {
                StatusText.Text = "Nothing to redo";
            }
        }

        // Clipboard operations
        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No entries selected";
                return;
            }

            _clipboardService.Cut(selectedItems);
            StatusText.Text = $"{selectedItems.Count} entries cut to clipboard";
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No entries selected";
                return;
            }

            _clipboardService.Copy(selectedItems);
            StatusText.Text = $"{selectedItems.Count} entries copied to clipboard";
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (!_clipboardService.HasContent)
            {
                StatusText.Text = "Clipboard is empty";
                return;
            }

            var action = new MoveAction("Paste entries", _filteredTrafficEntries);

            var pastedEntries = _clipboardService.Paste();
            var cutEntries = _clipboardService.GetCutEntries();

            // Remove cut entries from source
            foreach (var entry in cutEntries)
            {
                _filteredTrafficEntries.Remove(entry);
            }

            // Add pasted entries
            foreach (var entry in pastedEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }

            action.CaptureAfterState(_filteredTrafficEntries);
            _undoRedoService.RecordAction(action);

            _moveService.ReassignIds(_filteredTrafficEntries);
            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"{pastedEntries.Count} entries pasted";
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            TrafficDataGrid.SelectAll();
            StatusText.Text = $"All {_filteredTrafficEntries.Count} entries selected";
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            TrafficDataGrid.UnselectAll();
            StatusText.Text = "Selection cleared";
        }

        #endregion

        #region Pin/Unpin Handlers

        private void PinSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries to pin.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var action = new ModifyAction("Pin entries", selectedItems);
            _moveService.PinEntries(selectedItems);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"{selectedItems.Count} entries pinned";
        }

        private void UnpinSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries to unpin.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var action = new ModifyAction("Unpin entries", selectedItems);
            _moveService.UnpinEntries(selectedItems);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"{selectedItems.Count} entries unpinned";
        }

        private void MovePinnedToTop_Click(object sender, RoutedEventArgs e)
        {
            var action = new MoveAction("Move pinned to top", _filteredTrafficEntries);
            _moveService.MovePinnedToTop(_filteredTrafficEntries);
            action.CaptureAfterState(_filteredTrafficEntries);
            _undoRedoService.RecordAction(action);

            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = "Pinned entries moved to top";
        }

        #endregion

        #region Tags and Color Handlers

        private void AddTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries to tag.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var tags = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter tags (comma-separated):",
                "Add Tags",
                "");

            if (string.IsNullOrWhiteSpace(tags))
                return;

            var action = new ModifyAction("Add tags", selectedItems);
            _moveService.AddTags(selectedItems, tags);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"Tags added to {selectedItems.Count} entries";
        }

        private void RemoveTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var tags = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter tags to remove (comma-separated):",
                "Remove Tags",
                "");

            if (string.IsNullOrWhiteSpace(tags))
                return;

            var action = new ModifyAction("Remove tags", selectedItems);
            _moveService.RemoveTags(selectedItems, tags);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"Tags removed from {selectedItems.Count} entries";
        }

        private void FilterByTags_Click(object sender, RoutedEventArgs e)
        {
            var tags = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter tags to filter by (comma-separated):",
                "Filter by Tags",
                "");

            if (string.IsNullOrWhiteSpace(tags))
                return;

            var matchingEntries = _moveService.FilterByTags(_trafficEntries, tags);

            _filteredTrafficEntries.Clear();
            foreach (var entry in matchingEntries)
            {
                _filteredTrafficEntries.Add(entry);
            }

            UpdateUI();
            StatusText.Text = $"Filtered: {matchingEntries.Count} entries with tags '{tags}'";
        }

        private void SetColor_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more entries.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var color = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter color (Red, Green, Blue, Yellow, Orange, etc.):",
                "Set Color",
                "Yellow");

            if (string.IsNullOrWhiteSpace(color))
                return;

            var action = new ModifyAction("Set color", selectedItems);
            _moveService.SetColor(selectedItems, color);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"Color set for {selectedItems.Count} entries";
        }

        private void ClearColor_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedItems.Count == 0)
            {
                selectedItems = _filteredTrafficEntries.Where(e => !string.IsNullOrEmpty(e.Color)).ToList();
            }

            if (selectedItems.Count == 0)
            {
                StatusText.Text = "No colored entries found";
                return;
            }

            var action = new ModifyAction("Clear color", selectedItems);
            _moveService.SetColor(selectedItems, null);
            action.CaptureAfterState(selectedItems);
            _undoRedoService.RecordAction(action);

            _hasUnsavedChanges = true;
            UpdateUI();
            StatusText.Text = $"Color cleared from {selectedItems.Count} entries";
        }

        #endregion

        #region Duplicate Handlers

        private void FindDuplicates_Click(object sender, RoutedEventArgs e)
        {
            var duplicates = _moveService.FindDuplicates(_filteredTrafficEntries, DuplicateCriteria.UrlAndMethod);

            if (duplicates.Count == 0)
            {
                MessageBox.Show("No duplicates found!", "Find Duplicates",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var message = $"Found {duplicates.Count} groups of duplicates:\n\n";
            int count = 0;
            foreach (var group in duplicates.Take(10))
            {
                message += $"• {group.Key}: {group.Count} entries\n";
                count++;
            }

            if (duplicates.Count > 10)
            {
                message += $"\n... and {duplicates.Count - 10} more groups";
            }

            message += $"\n\nTotal duplicate entries: {duplicates.Sum(g => g.Count - 1)}";

            MessageBox.Show(message, "Duplicates Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveDuplicates_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will remove duplicate entries (keeping the first occurrence).\n\nContinue?",
                "Remove Duplicates",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var action = new MoveAction("Remove duplicates", _filteredTrafficEntries);
            var removedEntries = _moveService.RemoveDuplicates(_filteredTrafficEntries, DuplicateCriteria.UrlAndMethod, true);
            action.CaptureAfterState(_filteredTrafficEntries);
            _undoRedoService.RecordAction(action);

            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();

            MessageBox.Show($"Removed {removedEntries.Count} duplicate entries.",
                "Duplicates Removed", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText.Text = $"{removedEntries.Count} duplicates removed";
        }

        #endregion

        #region Session Management Handlers

        private void MergeSessions_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Sessions to Merge",
                Filter = "Web Traffic Inspector Session (*.wtis)|*.wtis",
                InitialDirectory = _sessionService.GetDefaultSessionsPath(),
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true || openFileDialog.FileNames.Length == 0)
                return;

            var sources = new List<List<TrafficEntry>>();

            foreach (var file in openFileDialog.FileNames)
            {
                try
                {
                    var sessionData = _sessionService.LoadSessionAsync(file).Result;
                    if (sessionData != null)
                    {
                        sources.Add(sessionData.TrafficEntries);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load {Path.GetFileName(file)}: {ex.Message}",
                        "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            if (sources.Count == 0)
            {
                MessageBox.Show("No sessions loaded successfully.", "Merge Sessions",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var action = new MoveAction("Merge sessions", _filteredTrafficEntries);
            _moveService.MergeCollections(_filteredTrafficEntries, sources, true);
            action.CaptureAfterState(_filteredTrafficEntries);
            _undoRedoService.RecordAction(action);

            SyncFilteredToMain();
            _hasUnsavedChanges = true;
            UpdateUI();

            var totalEntries = sources.Sum(s => s.Count);
            MessageBox.Show($"Merged {sources.Count} sessions ({totalEntries} entries) into current session.",
                "Sessions Merged", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText.Text = $"{sources.Count} sessions merged";
        }

        private void SplitByHost_Click(object sender, RoutedEventArgs e)
        {
            var splitGroups = _moveService.SplitByHost(_filteredTrafficEntries);

            if (splitGroups.Count <= 1)
            {
                MessageBox.Show("Not enough unique hosts to split.",
                    "Split Session", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var message = $"Session can be split into {splitGroups.Count} sessions by host:\n\n";
            foreach (var group in splitGroups.Take(10))
            {
                message += $"• {group.Key}: {group.Value.Count} entries\n";
            }

            if (splitGroups.Count > 10)
            {
                message += $"\n... and {splitGroups.Count - 10} more hosts";
            }

            message += "\n\nSave each host to a separate session file?";

            var result = MessageBox.Show(message, "Split by Host",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveSplitSessions(splitGroups);
            }
        }

        private void SplitByStatus_Click(object sender, RoutedEventArgs e)
        {
            var splitGroups = _moveService.SplitByStatusCategory(_filteredTrafficEntries);

            var message = $"Session can be split into {splitGroups.Count} sessions by status category:\n\n";
            foreach (var group in splitGroups)
            {
                message += $"• {group.Key}: {group.Value.Count} entries\n";
            }

            message += "\n\nSave each category to a separate session file?";

            var result = MessageBox.Show(message, "Split by Status",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveSplitSessions(splitGroups);
            }
        }

        private void SplitByTime_Click(object sender, RoutedEventArgs e)
        {
            var intervalInput = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter time interval in seconds:",
                "Split by Time Interval",
                "60");

            if (!int.TryParse(intervalInput, out int seconds) || seconds <= 0)
            {
                MessageBox.Show("Invalid interval. Please enter a positive number.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var splitGroups = _moveService.SplitByTimeInterval(_filteredTrafficEntries, TimeSpan.FromSeconds(seconds));

            var message = $"Session can be split into {splitGroups.Count} time intervals:\n\n";
            foreach (var group in splitGroups.Take(10))
            {
                message += $"• {group.Key}: {group.Value.Count} entries\n";
            }

            if (splitGroups.Count > 10)
            {
                message += $"\n... and {splitGroups.Count - 10} more intervals";
            }

            message += "\n\nSave each interval to a separate session file?";

            var result = MessageBox.Show(message, "Split by Time",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveSplitSessions(splitGroups);
            }
        }

        private async void SaveSplitSessions(Dictionary<string, List<TrafficEntry>> splitGroups)
        {
            var folderDialog = new OpenFileDialog
            {
                Title = "Select Folder to Save Split Sessions",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection."
            };

            if (folderDialog.ShowDialog() != true)
                return;

            var folder = Path.GetDirectoryName(folderDialog.FileName);
            int savedCount = 0;

            foreach (var group in splitGroups)
            {
                var safeFileName = string.Join("_", group.Key.Split(Path.GetInvalidFileNameChars()));
                var filePath = Path.Combine(folder, $"{safeFileName}.wtis");

                try
                {
                    var success = await _sessionService.SaveSessionAsync(
                        filePath,
                        group.Value,
                        safeFileName,
                        $"Split session - {group.Value.Count} entries");

                    if (success)
                        savedCount++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save {safeFileName}: {ex.Message}",
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            MessageBox.Show($"Successfully saved {savedCount} of {splitGroups.Count} sessions.",
                "Split Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText.Text = $"{savedCount} split sessions saved";
        }

        #endregion

        #region Help Menu Handlers

        private void UserGuide_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://docs.webtrafficinspector.com/user-guide",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open user guide: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void QuickStart_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Quick Start Tutorial window coming soon!", "Quick Start",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void KeyboardShortcuts_Click(object sender, RoutedEventArgs e)
        {
            var shortcuts = "Keyboard Shortcuts\n\n" +
                           "File Operations:\n" +
                           "  Ctrl+N - New Session\n" +
                           "  Ctrl+O - Open Session\n" +
                           "  Ctrl+S - Save Session\n" +
                           "  Ctrl+Shift+S - Save Session As\n\n" +
                           "Move Operations:\n" +
                           "  Ctrl+Up - Move Entry Up\n" +
                           "  Ctrl+Down - Move Entry Down\n" +
                           "  Ctrl+Home - Move to Top\n" +
                           "  Ctrl+End - Move to Bottom\n" +
                           "  Delete - Delete Selected Entries\n\n" +
                           "View Operations:\n" +
                           "  F11 - Toggle Full Screen\n" +
                           "  Ctrl++ - Zoom In\n" +
                           "  Ctrl+- - Zoom Out\n" +
                           "  Ctrl+0 - Reset Zoom\n\n" +
                           "Tools:\n" +
                           "  Ctrl+F - Search Traffic\n" +
                           "  Ctrl+Shift+F - Advanced Filter\n" +
                           "  Ctrl+, - Options\n\n" +
                           "Help:\n" +
                           "  F1 - User Guide\n" +
                           "  Alt+F4 - Exit";

            MessageBox.Show(shortcuts, "Keyboard Shortcuts", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void VideoTutorials_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://youtube.com/webtrafficinspector",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open video tutorials: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnlineDocumentation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://docs.webtrafficinspector.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open documentation: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CommunityForum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://community.webtrafficinspector.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open community forum: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReportBug_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bug Report window coming soon!", "Report Bug",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FeatureRequest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Feature Request window coming soon!", "Feature Request",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ContactSupport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mailto:support@googlex-technologies.com?subject=Web Traffic Inspector Support",
                    UseShellExecute = true
                });
            }
            catch
            {
                Clipboard.SetText("support@googlex-technologies.com");
                MessageBox.Show("Support email copied to clipboard:\nsupport@googlex-technologies.com",
                    "Support Contact", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SupportDevelopment_Click(object sender, RoutedEventArgs e)
        {
            var donationWindow = new DonationWindow();
            donationWindow.Owner = this;
            donationWindow.ShowDialog();
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Update checker coming soon!", "Check Updates",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        #endregion

        #region Navigation and Browser Handlers

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation starting to: {e.Uri}");
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UrlTextBox.Text = WebViewControl.CoreWebView2.Source;
            });
            System.Diagnostics.Debug.WriteLine($"Navigation completed: {e.IsSuccess}");
        }

        private void WebView_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DOM content loaded");
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToUrl();
        }

        private void UrlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NavigateToUrl();
            }
        }

        private void NavigateToUrl()
        {
            try
            {
                var url = UrlTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    {
                        url = "https://" + url;
                    }

                    WebViewControl.CoreWebView2?.Navigate(url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Navigation error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = UrlTextBox.Text.Trim();
                if (string.IsNullOrEmpty(url))
                    url = "https://httpbin.org/get";

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                var chromeArgs = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors --new-window \"{url}\"";

                try
                {
                    var chromeProcess = new ProcessStartInfo
                    {
                        FileName = "chrome.exe",
                        Arguments = chromeArgs,
                        UseShellExecute = true
                    };
                    Process.Start(chromeProcess);

                    MessageBox.Show($"Chrome opened with proxy settings.\nProxy: 127.0.0.1:{_proxyService.ProxyPort}\nAll traffic from this browser window will be captured.",
                        "Browser Opened", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    try
                    {
                        var edgeArgs = $"--proxy-server=127.0.0.1:{_proxyService.ProxyPort} --ignore-certificate-errors --ignore-ssl-errors \"{url}\"";
                        var edgeProcess = new ProcessStartInfo
                        {
                            FileName = "msedge.exe",
                            Arguments = edgeArgs,
                            UseShellExecute = true
                        };
                        Process.Start(edgeProcess);

                        MessageBox.Show($"Edge opened with proxy settings.\nProxy: 127.0.0.1:{_proxyService.ProxyPort}\nAll traffic from this browser window will be captured.",
                            "Browser Opened", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show($"Could not automatically configure browser.\n\nManually configure your browser proxy settings:\nHTTP Proxy: 127.0.0.1:{_proxyService.ProxyPort}\nHTTPS Proxy: 127.0.0.1:{_proxyService.ProxyPort}\n\nThen navigate to: {url}",
                            "Manual Configuration Required", MessageBoxButton.OK, MessageBoxImage.Information);

                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Traffic Management

        private void TrafficDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TrafficDataGrid.SelectedItem is TrafficEntry selectedEntry)
            {
                RequestTextBox.Text = selectedEntry.RawRequest ?? "No request data available";
                ResponseTextBox.Text = selectedEntry.RawResponse ?? "No response data available";
            }
            else
            {
                RequestTextBox.Text = "";
                ResponseTextBox.Text = "";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_trafficEntries.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to clear all captured traffic?",
                    "Clear Traffic", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _trafficEntries.Clear();
                    _filteredTrafficEntries.Clear();
                    RequestTextBox.Text = "";
                    ResponseTextBox.Text = "";
                    _hasUnsavedChanges = true;
                    UpdateUI();
                }
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateRecentSessionsMenu()
        {
            RecentSessionsMenu.Items.Clear();

            var recentSessions = _sessionService.GetRecentSessions();

            if (recentSessions.Any())
            {
                foreach (var sessionPath in recentSessions)
                {
                    var menuItem = new MenuItem
                    {
                        Header = Path.GetFileNameWithoutExtension(sessionPath),
                        Tag = sessionPath
                    };
                    menuItem.Click += async (s, e) => await LoadSessionAsync(sessionPath);
                    RecentSessionsMenu.Items.Add(menuItem);
                }
            }
            else
            {
                var noRecentItem = new MenuItem
                {
                    Header = "No recent sessions",
                    IsEnabled = false
                };
                RecentSessionsMenu.Items.Add(noRecentItem);
            }
        }

        private void UpdateUI()
        {
            SessionNameText.Text = _currentSessionName + (_hasUnsavedChanges ? "*" : "");
            TrafficCountText.Text = $"Traffic: {_filteredTrafficEntries.Count}/{_trafficEntries.Count}";

            Title = $"Web Traffic Inspector - {_currentSessionName}" +
                   (_hasUnsavedChanges ? "*" : "") +
                   (_isProxyStarted ? $" (Proxy: {_proxyService.ProxyPort})" : "");
        }

        private void ApplyAdvancedFilter(object filterCriteria)
        {
            // Placeholder for advanced filter implementation
        }

        #endregion

        #region Request Replayer Handlers

        private async void ReplayRequest_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select a request to replay.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var result = await _requestReplayerService.ReplayRequest(selectedEntry);

                if (result.Success)
                {
                    var message = $"Request replayed successfully!\n\n" +
                                $"Status: {result.StatusCode}\n" +
                                $"Duration: {result.Duration.TotalMilliseconds:F0}ms\n" +
                                $"Response Length: {result.ResponseBody?.Length ?? 0} bytes\n\n" +
                                $"View full response in the Response tab?";

                    var viewResult = MessageBox.Show(message, "Replay Success",
                        MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (viewResult == MessageBoxResult.Yes)
                    {
                        ResponseTextBox.Text = result.ResponseBody ?? "No response body";
                    }
                }
                else
                {
                    MessageBox.Show($"Request replay failed:\n{result.Error}", "Replay Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error replaying request: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ReplayModified_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select a request to replay.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Create a dialog for modifying request parameters
            var modifiedUrl = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter modified URL (or leave blank to keep original):",
                "Modify Request",
                $"http://{selectedEntry.Host}{selectedEntry.Path}");

            if (string.IsNullOrEmpty(modifiedUrl))
                return;

            try
            {
                var options = new ReplayOptions { ModifiedUrl = modifiedUrl };
                var result = await _requestReplayerService.ReplayRequest(selectedEntry, options);

                if (result.Success)
                {
                    MessageBox.Show($"Modified request replayed successfully!\n\n" +
                                  $"Status: {result.StatusCode}\n" +
                                  $"Duration: {result.Duration.TotalMilliseconds:F0}ms",
                                  "Replay Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Request replay failed:\n{result.Error}", "Replay Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error replaying request: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BatchReplay_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select a request to replay.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var countStr = Microsoft.VisualBasic.Interaction.InputBox(
                "How many times should the request be replayed?",
                "Batch Replay",
                "10");

            if (!int.TryParse(countStr, out int count) || count < 1 || count > 100)
            {
                MessageBox.Show("Please enter a valid number between 1 and 100.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var results = await _requestReplayerService.BatchReplay(selectedEntry, count);
                var timeComparison = _comparisonService.CompareResponseTimes(results);

                var message = $"Batch Replay Complete!\n\n" +
                            $"Total Requests: {count}\n" +
                            $"Successful: {results.Count(r => r.Success)}\n" +
                            $"Failed: {results.Count(r => !r.Success)}\n\n" +
                            $"Response Times:\n" +
                            $"  Average: {timeComparison.AverageMs:F2}ms\n" +
                            $"  Min: {timeComparison.MinMs:F2}ms\n" +
                            $"  Max: {timeComparison.MaxMs:F2}ms\n" +
                            $"  Median: {timeComparison.MedianMs:F2}ms\n" +
                            $"  Std Dev: {timeComparison.StandardDeviation:F2}ms";

                MessageBox.Show(message, "Batch Replay Results",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during batch replay: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void FuzzReplay_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select a request to fuzz.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var parameter = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter parameter name to fuzz:",
                "Fuzzing Mode",
                "");

            if (string.IsNullOrEmpty(parameter))
                return;

            // Generate common fuzzing payloads
            var payloads = new List<string>
            {
                "' OR '1'='1",
                "\" OR \"1\"=\"1",
                "<script>alert('XSS')</script>",
                "../../../etc/passwd",
                "; ls -la",
                "{{7*7}}",
                "${7*7}",
                "' UNION SELECT NULL--",
                "admin' --",
                "1' ORDER BY 1--"
            };

            try
            {
                var results = await _requestReplayerService.FuzzReplay(selectedEntry, payloads, parameter);

                var uniqueResponses = results.GroupBy(r => r.StatusCode).ToList();
                var message = $"Fuzzing Complete!\n\n" +
                            $"Total Payloads: {payloads.Count}\n" +
                            $"Successful Requests: {results.Count(r => r.Success)}\n" +
                            $"Unique Status Codes: {uniqueResponses.Count}\n\n" +
                            $"Status Code Distribution:\n" +
                            string.Join("\n", uniqueResponses.Select(g => $"  {g.Key}: {g.Count()} times"));

                MessageBox.Show(message, "Fuzzing Results",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during fuzzing: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Encoding/Decoding Handlers

        private void EncoderDecoder_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter text to encode/decode:",
                "Encoder/Decoder Tool",
                "");

            if (string.IsNullOrEmpty(input))
                return;

            var options = new string[]
            {
                "URL Encode", "URL Decode",
                "Base64 Encode", "Base64 Decode",
                "Hex Encode", "Hex Decode",
                "HTML Encode", "HTML Decode",
                "ROT13", "Auto-Decode"
            };

            var choice = Microsoft.VisualBasic.Interaction.InputBox(
                "Select operation:\n" + string.Join("\n", options.Select((o, i) => $"{i + 1}. {o}")),
                "Choose Operation",
                "1");

            if (!int.TryParse(choice, out int operation) || operation < 1 || operation > options.Length)
                return;

            try
            {
                string result = operation switch
                {
                    1 => _encodingService.UrlEncode(input),
                    2 => _encodingService.UrlDecode(input),
                    3 => _encodingService.Base64Encode(input),
                    4 => _encodingService.Base64Decode(input),
                    5 => _encodingService.HexEncode(input),
                    6 => _encodingService.HexDecode(input),
                    7 => _encodingService.HtmlEncode(input),
                    8 => _encodingService.HtmlDecode(input),
                    9 => _encodingService.ROT13(input),
                    10 => _encodingService.AutoDecode(input).BestGuess?.Result ?? "No decoding detected",
                    _ => "Invalid operation"
                };

                MessageBox.Show($"Result:\n\n{result}", "Encoding/Decoding Result",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during encoding/decoding: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void JwtDecoder_Click(object sender, RoutedEventArgs e)
        {
            var token = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter JWT token:",
                "JWT Decoder",
                "");

            if (string.IsNullOrEmpty(token))
                return;

            try
            {
                var jwtInfo = _encodingService.DecodeJwt(token);

                if (jwtInfo.IsValid)
                {
                    var message = $"JWT Decoded Successfully!\n\n" +
                                $"Header:\n{jwtInfo.Header}\n\n" +
                                $"Payload:\n{jwtInfo.Payload}\n\n" +
                                $"Signature:\n{jwtInfo.Signature}";

                    MessageBox.Show(message, "JWT Decoder",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Invalid JWT: {jwtInfo.Error}", "JWT Decoder",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error decoding JWT: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HashCalculator_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter text to hash:",
                "Hash Calculator",
                "");

            if (string.IsNullOrEmpty(input))
                return;

            try
            {
                var md5 = _encodingService.MD5Hash(input);
                var sha1 = _encodingService.SHA1Hash(input);
                var sha256 = _encodingService.SHA256Hash(input);
                var sha512 = _encodingService.SHA512Hash(input);

                var message = $"Hash Results:\n\n" +
                            $"MD5:\n{md5}\n\n" +
                            $"SHA1:\n{sha1}\n\n" +
                            $"SHA256:\n{sha256}\n\n" +
                            $"SHA512:\n{sha512}";

                MessageBox.Show(message, "Hash Calculator",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating hashes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AutoDecode_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select an entry to auto-decode.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var text = selectedEntry.RawRequest ?? selectedEntry.RawResponse ?? "";
                var extracted = _encodingService.ExtractEncodedStrings(text);

                if (extracted.Any())
                {
                    var message = $"Found {extracted.Count} encoded strings:\n\n" +
                                string.Join("\n\n", extracted.Select(e =>
                                    $"{e.Type} at position {e.Position}:\n" +
                                    $"Original: {e.Original.Substring(0, Math.Min(50, e.Original.Length))}...\n" +
                                    $"Decoded: {e.Decoded.Substring(0, Math.Min(50, e.Decoded.Length))}..."));

                    MessageBox.Show(message, "Auto-Decode Results",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No encoded strings detected.", "Auto-Decode",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during auto-decode: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Pattern Matcher Handlers

        private void SearchPattern_Click(object sender, RoutedEventArgs e)
        {
            var pattern = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter pattern to search (regex supported):",
                "Pattern Search",
                "");

            if (string.IsNullOrEmpty(pattern))
                return;

            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var matches = _patternMatcherService.SearchPattern(entries, pattern, isRegex: true);

                if (matches.Any())
                {
                    var message = $"Found {matches.Count} matches:\n\n" +
                                string.Join("\n", matches.Take(20).Select(m =>
                                    $"Entry #{m.EntryId}: {m.MatchedText.Substring(0, Math.Min(50, m.MatchedText.Length))}..."));

                    if (matches.Count > 20)
                        message += $"\n\n... and {matches.Count - 20} more matches";

                    MessageBox.Show(message, "Pattern Search Results",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No matches found.", "Pattern Search",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during pattern search: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractEmails_Click(object sender, RoutedEventArgs e)
        {
            ExtractCommonPattern("Email");
        }

        private void ExtractUrls_Click(object sender, RoutedEventArgs e)
        {
            ExtractCommonPattern("URL");
        }

        private void ExtractIPs_Click(object sender, RoutedEventArgs e)
        {
            ExtractCommonPattern("IP");
        }

        private void ExtractAPIKeys_Click(object sender, RoutedEventArgs e)
        {
            ExtractCommonPattern("APIKey");
        }

        private void ExtractJWTs_Click(object sender, RoutedEventArgs e)
        {
            ExtractCommonPattern("JWT");
        }

        private void ExtractCommonPattern(string patternType)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var extracted = _patternMatcherService.ExtractCommonPattern(entries, patternType);

                if (extracted.Any())
                {
                    var uniqueValues = extracted.Select(e => e.Value).Distinct().ToList();
                    var message = $"Found {uniqueValues.Count} unique {patternType}(s):\n\n" +
                                string.Join("\n", uniqueValues.Take(50));

                    if (uniqueValues.Count > 50)
                        message += $"\n\n... and {uniqueValues.Count - 50} more";

                    MessageBox.Show(message, $"Extract {patternType}s",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No {patternType}s found.", $"Extract {patternType}s",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting {patternType}s: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractParameters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var parameters = _patternMatcherService.ExtractParameters(entries);

                if (parameters.Any())
                {
                    var message = $"Found {parameters.Count} unique parameters:\n\n" +
                                string.Join("\n", parameters.Take(50).Select(p =>
                                    $"{p.Name} ({p.Type}) - {p.OccurrenceCount} occurrences"));

                    if (parameters.Count > 50)
                        message += $"\n\n... and {parameters.Count - 50} more";

                    MessageBox.Show(message, "Extract Parameters",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No parameters found.", "Extract Parameters",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting parameters: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractHeaders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var headers = _patternMatcherService.ExtractHeaders(entries);

                if (headers.Any())
                {
                    var message = $"Found {headers.Count} unique headers:\n\n" +
                                string.Join("\n", headers.Take(50).Select(h =>
                                    $"{h.Name} - {h.OccurrenceCount} occurrences\n  Sample: {h.SampleValue?.Substring(0, Math.Min(50, h.SampleValue.Length))}..."));

                    if (headers.Count > 50)
                        message += $"\n\n... and {headers.Count - 50} more";

                    MessageBox.Show(message, "Extract Headers",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No headers found.", "Extract Headers",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting headers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtractCookies_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var cookies = _patternMatcherService.ExtractCookies(entries);

                if (cookies.Any())
                {
                    var message = $"Found {cookies.Count} unique cookies:\n\n" +
                                string.Join("\n", cookies.Take(50).Select(c =>
                                    $"{c.Name} - {c.OccurrenceCount} occurrences"));

                    if (cookies.Count > 50)
                        message += $"\n\n... and {cookies.Count - 50} more";

                    MessageBox.Show(message, "Extract Cookies",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No cookies found.", "Extract Cookies",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting cookies: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListEndpoints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var endpoints = _patternMatcherService.ExtractEndpoints(entries);

                if (endpoints.Any())
                {
                    var message = $"Found {endpoints.Count} unique endpoints:\n\n" +
                                string.Join("\n", endpoints.Take(50).Select(ep =>
                                    $"{ep.Method} {ep.Path} - {ep.RequestCount} requests"));

                    if (endpoints.Count > 50)
                        message += $"\n\n... and {endpoints.Count - 50} more";

                    MessageBox.Show(message, "List Endpoints",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No endpoints found.", "List Endpoints",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error listing endpoints: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Security Analyzer Handlers

        private void AnalyzeSecurity_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select an entry to analyze.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var report = _securityAnalyzerService.AnalyzeEntry(selectedEntry);

                var message = $"Security Analysis Report\n\n" +
                            $"Risk Level: {report.RiskLevel}\n" +
                            $"Risk Score: {report.RiskScore}/100\n\n" +
                            $"Issues Found: {report.TotalIssues}\n" +
                            $"  High: {report.HighSeverityCount}\n" +
                            $"  Medium: {report.MediumSeverityCount}\n" +
                            $"  Low: {report.LowSeverityCount}\n\n";

                if (report.Issues.Any())
                {
                    message += "Issues:\n" +
                             string.Join("\n", report.Issues.Take(10).Select(i =>
                                 $"  [{i.Severity}] {i.Type}: {i.Description}"));

                    if (report.Issues.Count > 10)
                        message += $"\n\n... and {report.Issues.Count - 10} more issues";
                }

                MessageBox.Show(message, "Security Analysis",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during security analysis: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnalyzeSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();

                var highRiskCount = reports.Count(r => r.RiskLevel == "High");
                var mediumRiskCount = reports.Count(r => r.RiskLevel == "Medium");
                var lowRiskCount = reports.Count(r => r.RiskLevel == "Low");
                var totalIssues = reports.Sum(r => r.TotalIssues);

                var message = $"Session Security Analysis\n\n" +
                            $"Total Entries: {entries.Count}\n" +
                            $"Total Issues: {totalIssues}\n\n" +
                            $"Risk Distribution:\n" +
                            $"  High Risk: {highRiskCount}\n" +
                            $"  Medium Risk: {mediumRiskCount}\n" +
                            $"  Low Risk: {lowRiskCount}\n\n" +
                            $"Would you like to export a detailed security report?";

                var result = MessageBox.Show(message, "Session Security Analysis",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var htmlReport = _exportService.ExportSecurityReport(reports, _currentSessionName);
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "HTML Files (*.html)|*.html",
                        FileName = $"SecurityReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveDialog.FileName, htmlReport);
                        MessageBox.Show("Security report saved successfully!", "Export Complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing session: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SecurityHeaders_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select an entry to analyze headers.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var report = _securityAnalyzerService.AnalyzeEntry(selectedEntry);
                var headersAnalysis = report.SecurityHeaders;

                if (headersAnalysis != null)
                {
                    var message = $"Security Headers Analysis\n\n" +
                                $"HSTS: {(headersAnalysis.HasHSTS ? "✓" : "✗")}\n" +
                                $"CSP: {(headersAnalysis.HasCSP ? "✓" : "✗")}\n" +
                                $"X-Frame-Options: {(headersAnalysis.HasXFrameOptions ? "✓" : "✗")}\n" +
                                $"X-Content-Type-Options: {(headersAnalysis.HasXContentTypeOptions ? "✓" : "✗")}\n" +
                                $"Referrer-Policy: {(headersAnalysis.HasReferrerPolicy ? "✓" : "✗")}\n\n";

                    if (headersAnalysis.MissingHeaders.Any())
                    {
                        message += "Missing Headers:\n" +
                                 string.Join("\n", headersAnalysis.MissingHeaders.Select(h => $"  - {h}"));
                    }

                    MessageBox.Show(message, "Security Headers Analysis",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No response headers available for analysis.", "Security Headers",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing security headers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindVulnerabilities_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();

                var allIssues = reports.SelectMany(r => r.Issues)
                    .Where(i => i.Severity == "High" || i.Severity == "Medium")
                    .GroupBy(i => i.Type)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                if (allIssues.Any())
                {
                    var message = $"Found {allIssues.Sum(g => g.Count())} potential vulnerabilities:\n\n" +
                                string.Join("\n", allIssues.Take(10).Select(g =>
                                    $"{g.Key}: {g.Count()} occurrences"));

                    if (allIssues.Count > 10)
                        message += $"\n\n... and {allIssues.Count - 10} more vulnerability types";

                    MessageBox.Show(message, "Find Vulnerabilities",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("No high or medium severity vulnerabilities found.", "Find Vulnerabilities",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding vulnerabilities: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DetectInjections_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();

                var injectionIssues = reports.SelectMany(r => r.Issues)
                    .Where(i => i.Type.Contains("Injection"))
                    .ToList();

                if (injectionIssues.Any())
                {
                    var byType = injectionIssues.GroupBy(i => i.Type).OrderByDescending(g => g.Count());

                    var message = $"Found {injectionIssues.Count} potential injection points:\n\n" +
                                string.Join("\n", byType.Select(g =>
                                    $"{g.Key}: {g.Count()} occurrences"));

                    MessageBox.Show(message, "Detect Injections",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("No potential injection points detected.", "Detect Injections",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error detecting injections: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindSensitiveData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();

                var allSensitiveData = reports.SelectMany(r => r.SensitiveDataFound).ToList();

                if (allSensitiveData.Any())
                {
                    var byType = allSensitiveData.GroupBy(d => d.Type).OrderByDescending(g => g.Count());

                    var message = $"Found {allSensitiveData.Count} sensitive data exposures:\n\n" +
                                string.Join("\n", byType.Select(g =>
                                    $"{g.Key}: {g.Count()} occurrences"));

                    MessageBox.Show(message, "Find Sensitive Data",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("No sensitive data found.", "Find Sensitive Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding sensitive data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateSecurityReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                if (!entries.Any())
                {
                    MessageBox.Show("No entries to analyze.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();
                var htmlReport = _exportService.ExportSecurityReport(reports, _currentSessionName);

                var saveDialog = new SaveFileDialog
                {
                    Filter = "HTML Files (*.html)|*.html",
                    FileName = $"SecurityReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, htmlReport);
                    MessageBox.Show("Security report generated and saved successfully!", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    var openResult = MessageBox.Show("Would you like to open the report?", "Open Report",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (openResult == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating security report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Auto Attack Mode Handlers

        private void ToggleAutoAttackMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _autoAttackService.IsEnabled = !_autoAttackService.IsEnabled;

                if (_autoAttackService.IsEnabled)
                {
                    // Prompt for target URL using InputBox
                    string targetUrl = Microsoft.VisualBasic.Interaction.InputBox(
                        "Enter the base URL to attack (e.g., http://example.com):",
                        "Enter Target URL",
                        "",
                        -1, -1);

                    if (!string.IsNullOrWhiteSpace(targetUrl))
                    {
                        _autoAttackService.TargetUrl = targetUrl.Trim();
                        StatusText.Text = $"Auto Attack Mode: ENABLED - Target: {targetUrl.Trim()}";
                        MessageBox.Show($"Auto Attack Mode enabled for: {targetUrl.Trim()}\n\n" +
                            "All traffic matching this domain will be automatically scanned for vulnerabilities.",
                            "Auto Attack Mode Enabled", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _autoAttackService.IsEnabled = false;
                        MessageBox.Show("Auto Attack Mode requires a target URL.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    StatusText.Text = "Auto Attack Mode: DISABLED";
                    MessageBox.Show("Auto Attack Mode disabled.", "Auto Attack Mode",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling Auto Attack Mode: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAttackResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resultsWindow = new Windows.AttackResultsWindow(_autoAttackService);
                resultsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Attack Results window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadXSSPayloads_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Select XSS Payloads File"
                };

                if (openDialog.ShowDialog() == true)
                {
                    _autoAttackService.LoadXSSPayloadsFromFile(openDialog.FileName);
                    MessageBox.Show($"XSS payloads loaded successfully from:\n{openDialog.FileName}",
                        "Payloads Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading XSS payloads: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureAutoAttack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = "Auto Attack Mode Configuration:\n\n" +
                    $"Status: {(_autoAttackService.IsEnabled ? "ENABLED" : "DISABLED")}\n" +
                    $"Target URL: {_autoAttackService.TargetUrl ?? "Not Set"}\n\n" +
                    "Attack Types:\n" +
                    $"• XSS Scanning: {_autoAttackService.Options.EnableXSSScanning}\n" +
                    $"• SQL Injection: {_autoAttackService.Options.EnableSQLInjectionScanning}\n" +
                    $"• IDOR Scanning: {_autoAttackService.Options.EnableIDORScanning}\n\n" +
                    $"Delay Between Requests: {_autoAttackService.Options.DelayBetweenRequests}ms";

                MessageBox.Show(message, "Auto Attack Configuration",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing configuration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region GraphQL Analyzer Handlers

        private void AnalyzeGraphQL_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntry = TrafficDataGrid.SelectedItem as TrafficEntry;
            if (selectedEntry == null)
            {
                MessageBox.Show("Please select a traffic entry to analyze.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (!_graphQLService.IsGraphQLRequest(selectedEntry))
                {
                    MessageBox.Show("Selected entry does not appear to be a GraphQL request.", "Not GraphQL",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var analysis = _graphQLService.AnalyzeEntry(selectedEntry);
                var report = _graphQLService.GenerateReport(analysis);

                MessageBox.Show(report, "GraphQL Analysis Report",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing GraphQL: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region JWT Manipulation Handlers

        private void AnalyzeJWT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string token = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter JWT token to analyze:",
                    "JWT Analyzer",
                    "",
                    -1, -1);

                if (string.IsNullOrWhiteSpace(token)) return;

                if (!_jwtService.IsJWT(token.Trim()))
                {
                    MessageBox.Show("Invalid JWT format.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var analysis = _jwtService.AnalyzeToken(token.Trim());
                var report = _jwtService.GenerateReport(analysis);

                MessageBox.Show(report, "JWT Analysis Report",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing JWT: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateJWTAttacks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string token = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter JWT token to generate attack variants:",
                    "JWT Attack Generator",
                    "",
                    -1, -1);

                if (string.IsNullOrWhiteSpace(token)) return;

                var variants = _jwtService.GenerateAttackVariants(token.Trim());

                var report = new System.Text.StringBuilder();
                report.AppendLine("JWT Attack Variants Generated:");
                report.AppendLine(new string('=', 60));
                report.AppendLine();

                foreach (var variant in variants)
                {
                    report.AppendLine($"[{variant.AttackType}] {variant.Name}");
                    report.AppendLine($"Description: {variant.Description}");
                    report.AppendLine($"Token: {variant.Token}");
                    report.AppendLine();
                }

                MessageBox.Show(report.ToString(), "JWT Attack Variants",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating JWT attacks: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Intruder/Fuzzer Handlers

        private void LaunchIntruder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Intruder/Fuzzer interface: Comprehensive parameter fuzzing with 4 attack modes:\n\n" +
                "• Sniper: One payload set, one position at a time\n" +
                "• Battering Ram: Same payload in all positions\n" +
                "• Pitchfork: Multiple payload sets in parallel\n" +
                "• Cluster Bomb: All combinations\n\n" +
                "Includes payloads for: SQL Injection, XSS, Path Traversal, Command Injection, LDAP, XXE, NoSQL, SSRF, and more.",
                "Intruder/Fuzzer", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Advanced Report Handlers

        private void GenerateHTMLReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "HTML files (*.html)|*.html|All files (*.*)|*.*",
                    FileName = $"Security_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html",
                    DefaultExt = ".html"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var html = _advancedReportService.GenerateSecurityReport(_trafficEntries.ToList());
                    _advancedReportService.SaveReportToFile(html, saveDialog.FileName);

                    MessageBox.Show($"HTML report generated successfully:\n{saveDialog.FileName}",
                        "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ask if user wants to open it
                    var result = MessageBox.Show("Would you like to open the report now?", "Open Report",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating HTML report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Comparison Tool Handlers

        private void CompareEntries_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntries = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedEntries.Count != 2)
            {
                MessageBox.Show("Please select exactly 2 entries to compare.", "Invalid Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var comparison = _comparisonService.CompareEntries(selectedEntries[0], selectedEntries[1]);

                var message = $"Entry Comparison\n\n" +
                            $"Entry 1: {comparison.Entry1Url}\n" +
                            $"Entry 2: {comparison.Entry2Url}\n\n" +
                            $"Overall Similarity: {comparison.OverallSimilarity:F2}%\n\n" +
                            $"Method: {(comparison.MethodMatches ? "✓" : "✗")}\n" +
                            $"Host: {(comparison.HostMatches ? "✓" : "✗")}\n" +
                            $"Path: {(comparison.PathMatches ? "✓" : "✗")}\n" +
                            $"Status: {(comparison.StatusMatches ? "✓" : "✗")}\n\n" +
                            $"Request Similarity: {comparison.RequestComparison.SimilarityPercentage:F2}%\n" +
                            $"Response Similarity: {comparison.ResponseComparison.SimilarityPercentage:F2}%";

                MessageBox.Show(message, "Compare Entries",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error comparing entries: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompareRequests_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntries = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedEntries.Count != 2)
            {
                MessageBox.Show("Please select exactly 2 entries to compare requests.", "Invalid Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var request1 = selectedEntries[0].RawRequest ?? "";
                var request2 = selectedEntries[1].RawRequest ?? "";
                var comparison = _comparisonService.CompareText(request1, request2);

                var diffReport = _comparisonService.GenerateDiffReport(comparison);

                MessageBox.Show(diffReport, "Compare Requests",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error comparing requests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompareResponses_Click(object sender, RoutedEventArgs e)
        {
            var selectedEntries = TrafficDataGrid.SelectedItems.Cast<TrafficEntry>().ToList();
            if (selectedEntries.Count != 2)
            {
                MessageBox.Show("Please select exactly 2 entries to compare responses.", "Invalid Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var response1 = selectedEntries[0].RawResponse ?? "";
                var response2 = selectedEntries[1].RawResponse ?? "";
                var comparison = _comparisonService.CompareText(response1, response2);

                var diffReport = _comparisonService.GenerateDiffReport(comparison);

                MessageBox.Show(diffReport, "Compare Responses",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error comparing responses: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompareSessions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("To compare sessions, please:\n" +
                          "1. Tag entries from first session with 'session1'\n" +
                          "2. Tag entries from second session with 'session2'\n" +
                          "3. Click this button again",
                          "Compare Sessions", MessageBoxButton.OK, MessageBoxImage.Information);

            // For now, compare all vs filtered if they're different
            if (_trafficEntries.Count != _filteredTrafficEntries.Count)
            {
                try
                {
                    var comparison = _comparisonService.CompareSessions(
                        _trafficEntries.ToList(),
                        _filteredTrafficEntries.ToList());

                    var message = $"Session Comparison\n\n" +
                                $"Session 1 Entries: {comparison.Session1Count}\n" +
                                $"Session 2 Entries: {comparison.Session2Count}\n\n" +
                                $"Common URLs: {comparison.CommonUrls.Count}\n" +
                                $"Unique to Session 1: {comparison.UniqueToSession1.Count}\n" +
                                $"Unique to Session 2: {comparison.UniqueToSession2.Count}";

                    MessageBox.Show(message, "Compare Sessions",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error comparing sessions: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Export Handlers

        private void ExportHAR_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("HAR", "*.har", entries => _exportService.ExportToHAR(entries));
        }

        private void ExportJSON_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("JSON", "*.json", _exportService.ExportToJSON);
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("CSV", "*.csv", _exportService.ExportToCSV);
        }

        private void ExportXML_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("XML", "*.xml", _exportService.ExportToXML);
        }

        private void ExportMarkdown_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("Markdown", "*.md", entries => _exportService.ExportToMarkdown(entries));
        }

        private void ExportBurp_Click(object sender, RoutedEventArgs e)
        {
            ExportToFormat("Burp", "*.xml", _exportService.ExportToBurp);
        }

        private void ExportSecurityHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                if (!entries.Any())
                {
                    MessageBox.Show("No entries to export.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var reports = entries.Select(entry => _securityAnalyzerService.AnalyzeEntry(entry)).ToList();
                var htmlReport = _exportService.ExportSecurityReport(reports, _currentSessionName);

                var saveDialog = new SaveFileDialog
                {
                    Filter = "HTML Files (*.html)|*.html",
                    FileName = $"SecurityReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, htmlReport);
                    MessageBox.Show($"Exported {entries.Count} entries to security report successfully!", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting security report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToFormat(string formatName, string extension, Func<List<TrafficEntry>, string> exportFunc)
        {
            try
            {
                var entries = _filteredTrafficEntries.ToList();
                if (!entries.Any())
                {
                    MessageBox.Show("No entries to export.", "No Data",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = $"{formatName} Files ({extension})|{extension}",
                    FileName = $"Traffic_{DateTime.Now:yyyyMMdd_HHmmss}{extension.Replace("*", "")}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var content = exportFunc(entries);
                    File.WriteAllText(saveDialog.FileName, content);
                    MessageBox.Show($"Exported {entries.Count} entries to {formatName} successfully!", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to {formatName}: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region OAuth/OIDC and Privilege Escalation Detection

        private void OnOAuthFlowDetected(object sender, OAuthDetectedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text = $"OAuth {e.FlowData.FlowType} flow detected from {e.FlowData.Host}";

                // Show notification to user
                if (MessageBox.Show(
                    $"OAuth/OIDC flow detected!\n\n" +
                    $"Type: {e.FlowData.FlowType}\n" +
                    $"Host: {e.FlowData.Host}\n" +
                    $"Flow ID: {e.FlowData.FlowId}\n\n" +
                    $"Would you like to view and attack this OAuth flow?",
                    "OAuth Flow Detected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    ShowOAuthDetectionWindow();
                }
            });
        }

        private void OnOAuthVulnerabilityFound(object sender, VulnerabilityFoundEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusText.Text = $"OAuth vulnerability found: {e.Vulnerability.Type} ({e.Vulnerability.Severity})";
            });
        }

        private void ShowOAuthDetectionWindow()
        {
            try
            {
                var detectionWindow = new OAuthDetectionWindow(_oauthScanner, _privEscScanner);
                detectionWindow.Owner = this;
                detectionWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening OAuth detection window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Window Management

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSession_Click(null, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            _proxyService?.StopProxy();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _proxyService?.StopProxy();
            base.OnClosed(e);
        }

        #endregion
    }

    // Helper class for keyboard shortcuts
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = _ => execute();
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
    }
}
