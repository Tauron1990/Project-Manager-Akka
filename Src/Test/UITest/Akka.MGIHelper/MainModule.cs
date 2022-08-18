using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Settings;
using Akka.MGIHelper.Settings.Provider;
using Akka.MGIHelper.UI;
using Akka.MGIHelper.UI.FanControl;
using Akka.MGIHelper.UI.MgiStarter;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Tauron;
using Tauron.Application.CommonUI;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace Akka.MGIHelper
{
    public sealed class MainModule : IModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterView<MainWindow, MainWindowViewModel>();
            builder.RegisterView<MgiStarterControl, MgiStarterControlModel>();
            builder.RegisterView<AutoFanControl, AutoFanControlModel>();
            builder.RegisterView<LogWindow, LogWindowViewModel>();

            builder.RegisterSettingsManager(
                c => c.WithProvider<XmlProvider>()
                   .WithProvider(ProviderConfig.Json(SettingTypes.FanControlOptions, "fancontrol.json"))
                   .WithProvider(ProviderConfig.Json(SettingTypes.WindowOptions, "window.json")));

            builder.RegisterType<WindowOptions>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.WindowOptions);
            builder.RegisterType<FanControlOptions>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.FanControlOptions);
            builder.RegisterType<ProcessConfig>().AsSelf().InstancePerLifetimeScope().WithParameter("scope", SettingTypes.ProcessOptions);
        }

        public void Load(IServiceCollection collection)
        {
            
        }
    }
}