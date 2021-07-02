using Autofac;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.AppCore.Settings;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application.Settings;
using Tauron.Features;

namespace ServiceManager.Server
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSettingsManager(c => c.WithProvider<LocalConfigurationProvider>());
            builder.RegisterType<LocalConfiguration>().As<ILocalConfiguration>();

            builder.RegisterType<ClusterConnectionTracker>().As<IClusterConnectionTracker>();
            builder.RegisterType<DatabaseConfig>().As<IDatabaseConfig>();

            builder.RegisterFeature<ClusterNodeManagerRef, IClusterNodeManager>(ClusterHostManagerActor.New());

            base.Load(builder);
        }
    }
}