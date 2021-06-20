using System;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Config;

namespace Tauron.Host
{
    public enum ConfigurationOptions
    {
        None,
        ResolveFromContainer
    }

    [PublicAPI]
    public interface IActorApplicationBuilder
    {
        IActorApplicationBuilder ConfigureLogging(Action<HostBuilderContext, ISetupBuilder> config);

        IActorApplicationBuilder Configuration(Action<IConfigurationBuilder> config);

        IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config);

        IActorApplicationBuilder ConfigureServices(Action<IServiceCollection> config);

        IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> config);

        IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config);

        IActorApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system);

        IActorApplicationBuilder UpdateEnviroment(Func<IActorHostEnvironment, IActorHostEnvironment> updater);

        ActorApplication Build(ConfigurationOptions configuration = ConfigurationOptions.None);
    }
}