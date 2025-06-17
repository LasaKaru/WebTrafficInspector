using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Interaction logic for DonationWindow.xaml
    /// </summary>
    public partial class DonationWindow : Window
    {
        public DonationWindow()
        {
            InitializeComponent();
        }

        private void Donate5_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("https://paypal.me/webtrafficinspector/5");
        }

        private void Donate15_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("https://paypal.me/webtrafficinspector/15");
        }

        private void Donate50_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("https://paypal.me/webtrafficinspector/50");
        }

        private void DonateCustom_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("https://paypal.me/webtrafficinspector");
        }

        private void RateApp_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("ms-windows-store://review/?ProductId=9NBLGGH4NNS1");
        }

        private void ShareApp_Click(object sender, RoutedEventArgs e)
        {
            var shareText = "Check out Web Traffic Inspector - a free professional HTTP/HTTPS traffic analyzer! https://webtrafficinspector.com";
            Clipboard.SetText(shareText);
            MessageBox.Show("Share text copied to clipboard!", "Share",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportBugs_Click(object sender, RoutedEventArgs e)
        {
            OpenDonationLink("https://github.com/webtrafficinspector/issues");
        }

        private void OpenDonationLink(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
