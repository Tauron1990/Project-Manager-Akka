using Autofac;
using Tauron.Application.CommonUI;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.ClusterTracking;
using Tauron.Application.ServiceManager.AppCore.Helper;
using Tauron.Application.ServiceManager.AppCore.ServiceDeamon;
using Tauron.Application.ServiceManager.AppCore.Settings;
using Tauron.Application.ServiceManager.Components.Dialog;
using Tauron.Application.ServiceManager.ViewModels;
using Tauron.Application.Settings;
using Tauron.Features;

namespace Tauron.Application.ServiceManager
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModel<IndexViewModel>().InstancePerLifetimeScope();
            builder.RegisterModel<ConfigurationViewModel>().InstancePerLifetimeScope();
            builder.RegisterType<ConfirmRestartDialog>().AsSelf();

            builder.RegisterSettingsManager(c => c.WithProvider<LocalConfigurationProvider>());
            builder.RegisterType<LocalConfiguration>().As<ILocalConfiguration>();

            builder.RegisterType<ClusterConnectionTracker>().As<IClusterConnectionTracker>();
            builder.RegisterType<DatabaseConfig>().As<IDatabaseConfig>();

            builder.RegisterFeature<ClusterNodeManagerRef, IClusterNodeManager>(ClusterHostManagerActor.New());

            base.Load(builder);
        }
    }
}