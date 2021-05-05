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
    [PublicAPI]
    public interface IApplicationBuilder
    {
        IApplicationBuilder ConfigureLogging(Action<HostBuilderContext, ISetupBuilder> config);

        IApplicationBuilder Configuration(Action<IConfigurationBuilder> config);

        IApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config);

        IApplicationBuilder ConfigureServices(Action<IServiceCollection> config);

        IApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> config);

        IApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config);

        IApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system);

        ActorApplication Build();
    }
}