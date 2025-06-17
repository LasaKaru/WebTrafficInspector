using System.Configuration;
using System.Data;
using System.Windows;

namespace WebTrafficInspector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Prevent default MainWindow creation
            //this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //// Show loading screen as the first and only window
            //var loadingScreen = new LoadingScreen();
            //loadingScreen.Show();

            //base.OnStartup(e);
        }
    }

}
