using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Stl.Fusion.AkkaBridge.Connector;
using Stl.Fusion.AkkaBridge.Internal;
using Stl.Fusion.Bridge;
using Tauron.Features;

namespace Stl.Fusion.AkkaBridge
{
    [PublicAPI]
    public static class Extensions
    {
        public static void AddAkkaBridge(this ContainerBuilder builder)
        {
            builder.RegisterFeature<ServiceRegisterActorRef, IServiceRegistryActor>(ServiceRegistryActor.Factory());
            builder.RegisterType<AkkaProxyGenerator>().AsSelf();
        }
        
        public static FusionBuilder AddAkkaFusionServer(this FusionBuilder builder)
        {
            builder.Services.TryAddSingleton<IHostedService, AkkaFusionServiceHost>();

            return builder.AddPublisher();
        }

        public static FusionBuilder AddAkkaFusionClient(this FusionBuilder builder)
        {
            builder.Services.AddSingleton<IChannelProvider, AkkaChannelProvider>();

            return builder.AddReplicator();
        }
    }
}