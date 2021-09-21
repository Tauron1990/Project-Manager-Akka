using System;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
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
            builder.Services.AddHostedService<AkkaFusionServiceHost>();

            return builder.AddPublisher();
        }

        public static FusionBuilder AddAkkaFusionClient(this FusionBuilder builder, Action<AkkaFusionBuilder> akkaBuilder)
        {
            builder.Services.AddSingleton<IChannelProvider, AkkaChannelProvider>();

            akkaBuilder(new AkkaFusionBuilder(builder));

            return builder.AddReplicator();
        }

        public static FusionBuilder AddServicePublischer(this FusionBuilder builder, Action<PublisherBuilder> config)
        {
            builder.Services.AddHostedService<ServicePublisherHost>();
            config(new PublisherBuilder(builder.Services));

            return builder;
        }
    }
}