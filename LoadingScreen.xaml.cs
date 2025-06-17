using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Threading.Tasks;

namespace WebTrafficInspector
{
    /// <summary>
    /// Interaction logic for LoadingScreen.xaml
    /// </summary>
    public partial class LoadingScreen : Window
    {
        private DispatcherTimer _loadingTimer;
        private int _currentStep = 0;
        private readonly string[] _loadingSteps = {
            "Initializing application...",
            "Loading configuration...",
            "Setting up proxy service...",
            "Initializing WebView2 engine...",
            "Loading user interface...",
            "Preparing traffic capture...",
            "Finalizing startup...",
            "Ready to launch!"
        };

        public LoadingScreen()
        {
            InitializeComponent();
            InitializeLoadingScreen();
        }

        private void InitializeLoadingScreen()
        {
            // Set version information
            SetVersionInfo();

            // Load logo with fallback
            LoadLogo();

            // Start loading animation
            StartLoadingAnimation();
        }

        private void SetVersionInfo()
        {
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                VersionText.Text = $"{version.Major}.{version.Minor}.{version.Build}";
            }
            catch
            {
                VersionText.Text = "1.0.0";
            }
        }

        private void LoadLogo()
        {
            try
            {
                // Try to load logo.png from the application directory
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");

                if (File.Exists(logoPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(logoPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    LogoImage.Source = bitmap;
                }
                else
                {
                    // Create a simple fallback logo using text
                    LogoImage.Visibility = Visibility.Collapsed;

                    // Add a text-based logo as fallback
                    var fallbackLogo = new System.Windows.Controls.TextBlock
                    {
                        Text = "WTI",
                        FontSize = 48,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontFamily = new System.Windows.Media.FontFamily("Segoe UI")
                    };

                    // Add to the logo container
                    ((System.Windows.Controls.StackPanel)LogoImage.Parent).Children.Insert(0, fallbackLogo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load logo: {ex.Message}");
                LogoImage.Visibility = Visibility.Collapsed;
            }
        }

        private void StartLoadingAnimation()
        {
            _loadingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800) // 800ms per step
            };

            _loadingTimer.Tick += LoadingTimer_Tick;
            _loadingTimer.Start();
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            if (_currentStep < _loadingSteps.Length)
            {
                // Update loading text
                LoadingText.Text = _loadingSteps[_currentStep];

                // Update progress bar
                var progress = (double)(_currentStep + 1) / _loadingSteps.Length * 100;
                LoadingProgressBar.Value = progress;
                ProgressText.Text = $"{progress:F0}%";

                _currentStep++;
            }
            else
            {
                // Loading complete
                _loadingTimer.Stop();
                CompleteLoading();
            }
        }

        private async void CompleteLoading()
        {
            // Small delay for the final step
            await Task.Delay(500);

            // Create and show main window
            var mainWindow = new MainWindow();

            // Set as application main window and change shutdown mode
            Application.Current.MainWindow = mainWindow;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            mainWindow.Show();

            // Close loading screen
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _loadingTimer?.Stop();
            base.OnClosing(e);
        }
    }
}

