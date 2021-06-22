using Autofac;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.Helper;
using Tauron.Features;

namespace Tauron.Application.ServiceManager
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ClusterConnectionTracker>().As<IClusterConnectionTracker>();

            builder.RegisterFeature<ClusterHostManagerRef, IClusterHostManager>(ClusterHostManagerActor.New());

            base.Load(builder);
        }
    }
}