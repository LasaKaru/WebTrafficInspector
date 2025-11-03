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
            _proxyService = new ProxyService();
            _proxyService.TrafficCaptured += OnTrafficCaptured;

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
