using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Settings;
using Akka.MGIHelper.Settings.Provider;
using Akka.MGIHelper.UI;
using Akka.MGIHelper.UI.FanControl;
using Akka.MGIHelper.UI.MgiStarter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tauron.AkkaHost;
using Tauron.Application.CommonUI;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;
using Tauron.TAkka;

#pragma warning disable GU0011

namespace Akka.MGIHelper
{
    public sealed class MainModule : AkkaModule
    {
        public override void Load(IActorApplicationBuilder builder)
        {
            builder.RegisterSettingsManager(
                c => c.WithProvider<XmlProvider>()
                    .WithProvider(ProviderConfig.Json(SettingTypes.FanControlOptions, "fancontrol.json"))
                    .WithProvider(ProviderConfig.Json(SettingTypes.WindowOptions, "window.json")));
        }

        public override void Load(IServiceCollection collection)
        {
            collection.RegisterView<MainWindow, MainWindowViewModel>();
            collection.RegisterView<MgiStarterControl, MgiStarterControlModel>();
            collection.RegisterView<AutoFanControl, AutoFanControlModel>();
            collection.RegisterView<LogWindow, LogWindowViewModel>();


            collection.AddSingleton<WindowOptions>(
                sp =>
                    new WindowOptions(
                        sp.GetRequiredService<IDefaultActorRef<SettingsManager>>(),
                        SettingTypes.WindowOptions,
                        sp.GetRequiredService<ILogger<WindowOptions>>()));
                
            collection.AddSingleton<FanControlOptions>(
                sp =>
                    new FanControlOptions(
                        sp.GetRequiredService<IDefaultActorRef<SettingsManager>>(),
                        SettingTypes.FanControlOptions,
                        sp.GetRequiredService<ILogger<FanControlOptions>>()));
            
            collection.AddSingleton<ProcessConfig>(
                sp =>
                    new ProcessConfig(
                        sp.GetRequiredService<IDefaultActorRef<SettingsManager>>(),
                        SettingTypes.ProcessOptions,
                        sp.GetRequiredService<ILogger<ProcessConfig>>()));
        }
    }
}