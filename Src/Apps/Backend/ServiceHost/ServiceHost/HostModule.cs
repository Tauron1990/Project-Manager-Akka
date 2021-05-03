using Autofac;
using ServiceHost.ApplicationRegistry;
using ServiceHost.AutoUpdate;
using ServiceHost.Installer;
using ServiceHost.Services;
using ServiceHost.SharedApi;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Features;

namespace ServiceHost
{
    public sealed class HostModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppManager>().As<IAppManager>().SingleInstance();
            builder.RegisterFeature<AutoUpdater, IAutoUpdater>(AutoUpdateActor.New()).SingleInstance();
            builder.RegisterFeature<Installer.Installer, IInstaller>(InstallManagerActor.New()).SingleInstance();
            builder.RegisterFeature<AppRegistry, IAppRegistry>(AppRegistryActor.New()).SingleInstance();

            builder.RegisterType<ManualInstallationTrigger>().As<IStartUpAction>();
            builder.RegisterType<ServiceStartupTrigger>().As<IStartUpAction>();
            builder.RegisterType<CleanUpDedector>().As<IStartUpAction>();
            builder.RegisterType<ApiDispatcherStartup>().As<IStartUpAction>();

            builder.RegisterType<InstallChecker>().AsSelf();

            base.Load(builder);
        }
    }
}