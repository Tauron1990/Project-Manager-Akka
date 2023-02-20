using Microsoft.Extensions.DependencyInjection;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using ServiceHost.Services.Impl;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Features;

namespace ServiceHost
{
    public sealed class HostModule : AkkaModule
    {
        public override void Load(IActorApplicationBuilder builder)
        {
            builder.RegisterFeature<AppManager>(AppManagerActor.New(), "App-Manager");
            builder.RegisterFeature<AutoUpdater>(AutoUpdateActor.New(), "Auto-Updater");
            builder.RegisterFeature<Installation>(InstallManagerActor.New(), "Installer");
            builder.RegisterFeature<AppRegistry>(AppRegistryActor.New(), "Apps-Registry");
        }

        public override void Load(IServiceCollection builder) 
            => builder.AddTransient<InstallChecker>();
    }
}