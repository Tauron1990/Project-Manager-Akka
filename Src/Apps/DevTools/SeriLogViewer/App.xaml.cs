using System.Windows;
using Syncfusion.Licensing;

namespace SeriLogViewer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            SyncfusionLicenseProvider.RegisterLicense(
                "MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");
        }
    }
}