using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WebTrafficInspector
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            InitializeAboutWindow();
        }

        private void InitializeAboutWindow()
        {
            // Set version information from assembly
            SetVersionInfo();

            // Set window properties
            this.Owner = Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void SetVersionInfo()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                // Set version text with detailed information
                VersionText.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";

                // Add build date if available
                var buildDate = GetBuildDate(assembly);
                if (buildDate.HasValue)
                {
                    VersionText.Text += $" (Built: {buildDate.Value:yyyy-MM-dd})";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get version info: {ex.Message}");
                VersionText.Text = "Version 1.0.0";
            }
        }

        private DateTime? GetBuildDate(Assembly assembly)
        {
            try
            {
                // Get build date from assembly
                var attribute = assembly.GetCustomAttribute<System.Reflection.AssemblyMetadataAttribute>();
                if (attribute != null && attribute.Key == "BuildDate")
                {
                    if (DateTime.TryParse(attribute.Value, out DateTime buildDate))
                    {
                        return buildDate;
                    }
                }

                // Fallback: use file creation time
                var fileInfo = new System.IO.FileInfo(assembly.Location);
                return fileInfo.CreationTime;
            }
            catch
            {
                return null;
            }
        }

        private void Website_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var websiteUrl = "https://webtrafficinspector.com";
                Process.Start(new ProcessStartInfo
                {
                    FileName = websiteUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Unable to open website", ex.Message);
            }
        }

        private void Support_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var supportUrl = "mailto:support@googlex-technologies.com?subject=Web Traffic Inspector Support";
                Process.Start(new ProcessStartInfo
                {
                    FileName = supportUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Fallback: copy email to clipboard
                try
                {
                    Clipboard.SetText("support@googlex-technologies.com");
                    MessageBox.Show("Support email copied to clipboard:\nsupport@googlex-technologies.com",
                        "Support Contact", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    ShowErrorMessage("Unable to open email client",
                        "Please contact support at: support@googlex-technologies.com");
                }
            }
        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var licenseWindow = new LicenseWindow();
                licenseWindow.Owner = this;
                licenseWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                // Fallback: show license information in message box
                var licenseText = GetLicenseText();
                var licenseMessageWindow = new LicenseMessageWindow(licenseText);
                licenseMessageWindow.Owner = this;
                licenseMessageWindow.ShowDialog();
            }
        }

        private string GetLicenseText()
        {
            return @"WEB TRAFFIC INSPECTOR LICENSE

Copyright © 2025 Googlex Technologies. All rights reserved.

FREEWARE LICENSE

This software is provided free of charge for personal and commercial use.

PERMISSIONS:
✓ Use the software for any purpose
✓ Distribute the software
✓ Modify configuration files
✓ Use in commercial environments

RESTRICTIONS:
✗ Reverse engineering the core application
✗ Removing copyright notices
✗ Claiming ownership of the software
✗ Selling the software as standalone product

WARRANTY DISCLAIMER:
This software is provided 'as is' without warranty of any kind, either express or implied, including but not limited to the implied warranties of merchantability and fitness for a particular purpose.

LIMITATION OF LIABILITY:
In no event shall Googlex Technologies be liable for any damages whatsoever arising out of the use of or inability to use this software.

THIRD-PARTY COMPONENTS:
This software includes components from:
- Microsoft WebView2 Runtime (Microsoft Corporation)
- Titanium.Web.Proxy (Titanium Software)
- BouncyCastle Cryptography (The Legion of the Bouncy Castle Inc.)

For complete license terms, visit: https://webtrafficinspector.com/license";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // Handle keyboard shortcuts
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
            else if (e.Key == System.Windows.Input.Key.F1)
            {
                // Show help
                Website_Click(null, null);
            }

            base.OnKeyDown(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the close button for keyboard navigation
            var closeButton = this.FindName("CloseButton") as System.Windows.Controls.Button;
            closeButton?.Focus();
        }
    }

    // Simple license message window for fallback
    public class LicenseMessageWindow : Window
    {
        public LicenseMessageWindow(string licenseText)
        {
            Title = "License Information";
            Width = 600;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResize;

            var scrollViewer = new System.Windows.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = licenseText,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new System.Windows.Media.FontFamily("Consolas, Courier New"),
                FontSize = 11,
                Padding = new Thickness(10)
            };

            scrollViewer.Content = textBlock;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

            System.Windows.Controls.Grid.SetRow(scrollViewer, 0);
            grid.Children.Add(scrollViewer);

            var closeButton = new System.Windows.Controls.Button
            {
                Content = "Close",
                Width = 100,
                Height = 30,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => this.Close();

            System.Windows.Controls.Grid.SetRow(closeButton, 1);
            grid.Children.Add(closeButton);

            this.Content = grid;
        }
    }
}

