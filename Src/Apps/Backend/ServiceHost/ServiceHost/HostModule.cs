using Autofac;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using ServiceHost.Services.Impl;
using ServiceHost.SharedApi;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Features;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFeature<AppManager, IAppManager>(AppManagerActor.New());
            builder.RegisterFeature<AutoUpdater, IAutoUpdater>(AutoUpdateActor.New());
            builder.RegisterFeature<Installer.Installer, IInstaller>(InstallManagerActor.New());
            builder.RegisterFeature<AppRegistry, IAppRegistry>(AppRegistryActor.New());

            builder.RegisterStartUpAction<ManualInstallationTrigger>();
            builder.RegisterStartUpAction<ServiceStartupTrigger>();
            builder.RegisterStartUpAction<CleanUpDedector>();
            builder.RegisterStartUpAction<ApiDispatcherStartup>();

            builder.RegisterType<InstallChecker>().AsSelf();

            base.Load(builder);
        }
    }
}