using System;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.AkkaHost
{
    [PublicAPI]
    public interface IActorApplicationBuilder
    {
        IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config);

        IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config);

        IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Setup> config);

        IActorApplicationBuilder ConfigureAkkaSystem(Action<HostBuilderContext, ActorSystem> system);

        IActorApplicationBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate);

        IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);

        IActorApplicationBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate);

        IActorApplicationBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate);
    }
}