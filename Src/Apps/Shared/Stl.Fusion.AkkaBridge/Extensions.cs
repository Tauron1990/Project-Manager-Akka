using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Stl.Fusion.AkkaBridge.Internal;
using Stl.Fusion.Bridge;

namespace Stl.Fusion.AkkaBridge
{
    public static class Extensions
    {
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