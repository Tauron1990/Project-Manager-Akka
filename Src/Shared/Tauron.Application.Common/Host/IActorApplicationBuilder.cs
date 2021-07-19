using System;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using NLog.Config;

namespace Tauron.Host
{
    [PublicAPI]
    public interface IActorApplicationBuilder
    {
        IActorApplicationBuilder ConfigureLogging(Action<HostBuilderContext, ISetupBuilder> config);
        
        IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config);
        
        IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config);

        IActorApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system);
    }
}