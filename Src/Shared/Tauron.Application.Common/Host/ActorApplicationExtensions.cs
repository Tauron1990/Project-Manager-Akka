using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.Host
{
    public static class ActorApplicationExtensions
    {
        private sealed class ActorApplicationBluilder : IActorApplicationBuilder
        {
            private readonly List<Action<ContainerBuilder>> _autoFacBuilder = new();
            private readonly List<Func<HostBuilderContext, Config>> _akkaConfig = new();
            private readonly List<Action<HostBuilderContext, ActorSystem>> _systemConfig = new();

            public IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config)
            {
                _autoFacBuilder.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config)
            {
                _akkaConfig.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system)
            {
                _systemConfig.Add(system);
                return this;
            }
        }

        private sealed class ServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>, IActorApplicationBuilder
        {
            public ContainerBuilder CreateBuilder(IServiceCollection services)
            {

            }

            public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
            {

            }
        }

        public static IHostBuilder ConfigurateAkkaApplication(this IHostBuilder hostBuilder, Action<IActorApplicationBuilder> config)
        {

            return hostBuilder;
        }
    }
}