using System;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;

namespace WebTrafficInspector
{
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window
    {
        private readonly string _licenseText;

        public bool IsAccepted { get; private set; } = false;

        public LicenseWindow()
        {
            InitializeComponent();
            _licenseText = GetLicenseText();
            InitializeLicenseWindow();
        }

        private void InitializeLicenseWindow()
        {
            // Set the license text
            LicenseTextBlock.Text = _licenseText;

            // Set window properties
            this.Owner = Application.Current.MainWindow;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // Enable text selection
            //LicenseTextBlock.IsTextSelectionEnabled = true;

            // Focus the scroll viewer for keyboard navigation
            this.Loaded += (s, e) => LicenseScrollViewer.Focus();
        }

        private string GetLicenseText()
        {
            return @"WEB TRAFFIC INSPECTOR
SOFTWARE LICENSE AGREEMENT

Copyright © 2025 Googlex Technologies. All rights reserved.
Developed by Lasakru

================================================================================
FREEWARE LICENSE AGREEMENT
================================================================================

This Software License Agreement (""Agreement"") is a legal agreement between you 
(either an individual or a single entity) and Googlex Technologies for the 
software product identified above, which includes computer software and may 
include associated media, printed materials, and ""online"" or electronic 
documentation (""Software"").

BY INSTALLING, COPYING, OR OTHERWISE USING THE SOFTWARE, YOU AGREE TO BE BOUND 
BY THE TERMS OF THIS AGREEMENT. IF YOU DO NOT AGREE TO THE TERMS OF THIS 
AGREEMENT, DO NOT INSTALL OR USE THE SOFTWARE.

================================================================================
1. GRANT OF LICENSE
================================================================================

Googlex Technologies grants you the following rights:

✓ PERSONAL USE: You may install and use the Software on any number of computers 
  for personal, educational, or commercial purposes.

✓ DISTRIBUTION: You may distribute the Software in its original, unmodified form.

✓ BACKUP COPIES: You may make backup copies of the Software for archival purposes.

✓ NETWORK USE: You may use the Software on a computer network, provided that you 
  have obtained appropriate licenses for the number of users.

================================================================================
2. RESTRICTIONS
================================================================================

You may NOT:

✗ REVERSE ENGINEER: Reverse engineer, decompile, or disassemble the Software, 
  except and only to the extent that such activity is expressly permitted by 
  applicable law notwithstanding this limitation.

✗ MODIFY: Modify, adapt, alter, translate, or create derivative works of the 
  Software.

✗ REMOVE NOTICES: Remove or alter any copyright, trademark, or other proprietary 
  notices from the Software.

✗ SELL: Sell, rent, lease, or sublicense the Software as a standalone product.

✗ BUNDLE: Include the Software in any commercial software package without 
  written permission from Googlex Technologies.

================================================================================
3. FREEWARE TERMS
================================================================================

This Software is provided as FREEWARE. This means:

• NO PAYMENT REQUIRED: You are not required to pay any license fees or royalties 
  to use this Software.

• VOLUNTARY DONATIONS: Donations to support development are welcome but entirely 
  voluntary and do not create any additional rights or obligations.

• NO WARRANTY: The Software is provided ""AS IS"" without warranty of any kind.

• FUTURE VERSIONS: Future versions of the Software may include premium features 
  that require separate licensing.

================================================================================
4. THIRD-PARTY COMPONENTS
================================================================================

This Software includes the following third-party components:

• Microsoft WebView2 Runtime
  Copyright © Microsoft Corporation
  Licensed under Microsoft Software License Terms

• Titanium.Web.Proxy Library
  Copyright © Titanium Software
  Licensed under MIT License

• BouncyCastle Cryptography Library
  Copyright © The Legion of the Bouncy Castle Inc.
  Licensed under MIT License

• .NET Framework/Core Runtime
  Copyright © Microsoft Corporation
  Licensed under MIT License

Each third-party component is subject to its own license terms. Please refer to 
the respective component documentation for complete license information.

================================================================================
5. PRIVACY AND DATA COLLECTION
================================================================================

• NO DATA COLLECTION: This Software does not collect, store, or transmit any 
  personal information or usage data.

• LOCAL OPERATION: All traffic analysis is performed locally on your computer.

• NO TELEMETRY: No telemetry, analytics, or usage statistics are collected.

• SESSION DATA: Traffic capture sessions are stored locally and are not 
  transmitted to any external servers.

================================================================================
6. WARRANTY DISCLAIMER
================================================================================

THE SOFTWARE IS PROVIDED ""AS IS"" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT.

GOOGLEX TECHNOLOGIES DOES NOT WARRANT THAT THE SOFTWARE WILL MEET YOUR 
REQUIREMENTS OR THAT THE OPERATION OF THE SOFTWARE WILL BE UNINTERRUPTED OR 
ERROR-FREE.

================================================================================
7. LIMITATION OF LIABILITY
================================================================================

IN NO EVENT SHALL GOOGLEX TECHNOLOGIES BE LIABLE FOR ANY DAMAGES WHATSOEVER 
(INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS, BUSINESS 
INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR OTHER PECUNIARY LOSS) ARISING 
OUT OF THE USE OF OR INABILITY TO USE THE SOFTWARE, EVEN IF GOOGLEX TECHNOLOGIES 
HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

================================================================================
8. SUPPORT AND UPDATES
================================================================================

• BEST EFFORT SUPPORT: Support is provided on a best-effort basis through 
  community forums and email.

• FREE UPDATES: Updates and bug fixes are provided free of charge when available.

• NO GUARANTEE: There is no guarantee of continued development, support, or 
  updates.

================================================================================
9. TERMINATION
================================================================================

This license is effective until terminated. You may terminate it at any time by 
destroying the Software and all copies thereof. This license will also terminate 
if you fail to comply with any term or condition of this Agreement.

================================================================================
10. GOVERNING LAW
================================================================================

This Agreement shall be governed by the laws of the jurisdiction in which 
Googlex Technologies is established, without regard to conflict of law principles.

================================================================================
11. ENTIRE AGREEMENT
================================================================================

This Agreement constitutes the entire agreement between you and Googlex 
Technologies relating to the Software and supersedes all prior or contemporaneous 
understandings regarding such subject matter.

================================================================================
CONTACT INFORMATION
================================================================================

For questions about this license agreement, please contact:

Googlex Technologies
Email: support@googlex-technologies.com
Website: https://webtrafficinspector.com

================================================================================

By clicking ""I Accept"" below, you acknowledge that you have read this Agreement, 
understand it, and agree to be bound by its terms and conditions.

Last Updated: January 2025
Version: 1.0";
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Create a FlowDocument for printing
                    var flowDocument = new FlowDocument();
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(_licenseText));
                    flowDocument.Blocks.Add(paragraph);

                    // Set document properties
                    flowDocument.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
                    flowDocument.FontSize = 10;
                    flowDocument.PagePadding = new Thickness(50);

                    // Print the document
                    printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocument).DocumentPaginator,
                        "Web Traffic Inspector License");

                    MessageBox.Show("License printed successfully!", "Print Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to print license: {ex.Message}", "Print Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsText_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Save License Agreement",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = "WebTrafficInspector_License.txt",
                    DefaultExt = "txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, _licenseText);
                    MessageBox.Show($"License saved to:\n{saveFileDialog.FileName}", "Save Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save license: {ex.Message}", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_licenseText);
                MessageBox.Show("License text copied to clipboard!", "Copy Complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy license: {ex.Message}", "Copy Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            IsAccepted = true;

            // ✅ FIXED: Use simple file-based settings instead of Properties.Settings
            try
            {
                SaveLicenseAcceptance();
            }
            catch
            {
                // Ignore settings save errors
            }

            MessageBox.Show("Thank you for accepting the license agreement!", "License Accepted",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void SaveLicenseAcceptance()
        {
            try
            {
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebTrafficInspector");
                Directory.CreateDirectory(appDataPath);

                var settingsFile = Path.Combine(appDataPath, "settings.txt");
                var settings = $"LicenseAccepted=true\nLicenseAcceptedDate={DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                File.WriteAllText(settingsFile, settings);
            }
            catch
            {
                // Ignore file save errors
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // ✅ FIXED: Removed sender parameter reference
            switch (e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
                case Key.F1:
                    MessageBox.Show("Use the scroll bar or arrow keys to navigate through the license text.\n\n" +
                                  "Available actions:\n" +
                                  "• Print: Print the license agreement\n" +
                                  "• Save: Save license to a text file\n" +
                                  "• Copy: Copy license to clipboard\n" +
                                  "• Accept: Accept the license terms\n" +
                                  "• Close: Close this window\n\n" +
                                  "Keyboard shortcuts:\n" +
                                  "• Escape: Close window\n" +
                                  "• Ctrl+P: Print\n" +
                                  "• Ctrl+S: Save\n" +
                                  "• Ctrl+C: Copy\n" +
                                  "• Enter: Accept",
                                  "License Window Help", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case Key.P when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    Print_Click(this, null);
                    break;
                case Key.S when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    SaveAsText_Click(this, null);
                    break;
                case Key.C when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    CopyToClipboard_Click(this, null);
                    break;
                case Key.Enter:
                    Accept_Click(this, null);
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Set initial focus to the scroll viewer
            LicenseScrollViewer.Focus();
        }
    }
}

