using System.Threading.Tasks;
using Autofac;
using NLog;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Logging;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.SerilogViewer;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Localizer
{
    public static class CoreProgramm
    {
        //"pack: //application:,,,/Tauron.Application.Localizer;component/Theme.xaml"

        public static async Task Main(string[] args)
        {
            //SyncfusionLicenseProvider.RegisterLicense("MjY0ODk0QDMxMzgyZTMxMmUzMEx6Vkt0M1ZIRFVPRWFqMEcwbWVrK3dqUldkYzZiaXA3TGFlWDFORDFNSms9");

            var builder = ActorApplication.Create(args);

            builder
                .ConfigureLogging((_, configuration)
                    => configuration.ConfigDefaultLogging("Localizer").LoadConfiguration(c => c.Configuration.AddRuleForAllLevels(new LoggerViewerSink())))
                .ConfigureAutoFac(cb => cb.RegisterModule<MainModule>().RegisterModule<UIModule>())
                .ConfigurateAkkaSystem((_, system) => system.RegisterLocalization())
                .UseWpf<MainWindow>(c => c.WithAppFactory(() => new WpfFramework.DelegateApplication(new App())));

            using var app = builder.Build();
            await app.Run();
        }
    }
}