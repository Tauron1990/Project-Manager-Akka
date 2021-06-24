using Autofac;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.ClusterTracking;
using Tauron.Application.ServiceManager.AppCore.Configuration;
using Tauron.Application.ServiceManager.AppCore.Helper;
using Tauron.Features;

namespace Tauron.Application.ServiceManager
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ClusterConnectionTracker>().As<IClusterConnectionTracker>();
            builder.RegisterType<DatabaseConfig>().As<IDatabaseConfig>();

            builder.RegisterFeature<ClusterNodeManagerRef, IClusterNodeManager>(ClusterHostManagerActor.New());

            base.Load(builder);
        }
    }
}