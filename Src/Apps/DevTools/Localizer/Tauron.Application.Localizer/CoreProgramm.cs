using System.Threading.Tasks;
using Tauron.Application.Localizer.UIModels;

namespace Tauron.Application.Localizer
{
    public static class CoreProgramm
    {
        //"pack: //application:,,,/Tauron.Application.Localizer;component/Theme.xaml"

        public static async Task Main(string[] args)
        {
            //SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            await Host.CreateDefaultBuilder(args)
               .ConfigureLogging(lb => lb.AddNLog(sb => sb.ConfigDefaultLogging("Localizer").LoadConfiguration(c => c.Configuration.AddRuleForAllLevels(new LoggerViewerSink()))))
               .ConfigureAkkaApplication(
                    ab => ab.AddModule<MainModule>()
                       .AddModule<UIModule>()
                       .ConfigureAkkaSystem((_, system) => system.RegisterLocalization())
                       .UseWpf<MainWindow, App>())
               .Build().RunAsync();
        }
    }
}