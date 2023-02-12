using Microsoft.Extensions.DependencyInjection;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using ServiceHost.Services.Impl;
using Tauron;
using Tauron.Features;

namespace ServiceHost
{
    public sealed class HostModule : IModule
    {
        public void Load(IServiceCollection builder)
        {
            builder.RegisterFeature<AppManager, IAppManager>(AppManagerActor.New(), "App-Manager");
            builder.RegisterFeature<AutoUpdater, IAutoUpdater>(AutoUpdateActor.New(), "Auto-Updater");
            builder.RegisterFeature<Installation, IInstaller>(InstallManagerActor.New(), "Installer");
            builder.RegisterFeature<AppRegistry, IAppRegistry>(AppRegistryActor.New(), "Apps-Registry");

            builder.AddTransient<InstallChecker>();
        }
    }
}