using Autofac;
using Tauron.Application.ServiceManager.AppCore;
using Tauron.Application.ServiceManager.AppCore.Helper;

namespace Tauron.Application.ServiceManager
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ClusterConnectionTracker>().As<IClusterConnectionTracker>();

            base.Load(builder);
        }
    }
}