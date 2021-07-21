using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;

namespace Tauron.AkkaHost
{
    public static class ActorApplicationExtensions
    {
        private sealed class ActorApplicationBluilder : IActorApplicationBuilder
        {
            private sealed class ActorSystemHolder
            {
                private ActorSystem? _system;

                public ActorSystem System
                {
                    get
                    {
                        if (_system == null)
                            throw new InvalidOperationException("ActorSystem not Created");
                        return _system;
                    }
                    set => _system = value;
                }
            }

            private readonly ContainerBuilder _containerBuilder = new();
            private readonly ActorSystemHolder _holder = new();

            private readonly HostBuilderContext _context;
            private readonly IHostBuilder _builder;
            private readonly List<Func<HostBuilderContext, Config>> _akkaConfig = new();
            private readonly List<Func<HostBuilderContext, Setup>> _akkaSetup = new();
            private readonly List<Action<HostBuilderContext, ActorSystem>> _systemConfig = new();

            public ActorApplicationBluilder(HostBuilderContext context, IHostBuilder builder)
            {
                _context = context;
                _builder = builder;
                
                _containerBuilder.RegisterModule<CommonModule>();
                _containerBuilder.RegisterInstance(_holder);
                _containerBuilder.Register(c => c.Resolve<ActorSystemHolder>().System);
            }

            public IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config)
            {
                config(_containerBuilder);
                return this;
            }

            public IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config)
            {
                _akkaConfig.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Setup> config)
            {
                _akkaSetup.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAkkaSystem(Action<HostBuilderContext, ActorSystem> system)
            {
                _systemConfig.Add(system);
                return this;
            }

            public IActorApplicationBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
            {
                _builder.ConfigureHostConfiguration(configureDelegate);
                return this;
            }

            public IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
            {
                _builder.ConfigureAppConfiguration(configureDelegate);
                return this;
            }

            public IActorApplicationBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
            {
                _builder.ConfigureServices(configureDelegate);
                return this;
            }

            public IActorApplicationBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
            {
                _builder.ConfigureContainer(configureDelegate);
                return this;
            }

            internal ActorSystem CreateSystem(IServiceProvider provider)
            {
                var config = _akkaConfig.Aggregate(Config.Empty, (current, cfunc) => current.WithFallback(cfunc(_context)));
                var setup = _akkaSetup.Aggregate(
                    ActorSystemSetup.Create(DependencyResolverSetup.Create(provider)),
                    (systemSetup, func) => systemSetup.And(func(_context)));
                var system = ActorSystem.Create(GetActorSystemName(provider.GetRequiredService<IConfiguration>()), setup.WithSetup(BootstrapSetup.Create().WithConfig(config)));
                system.RegisterExtension(new DependencyResolverExtension());
                _holder.System = system;

                foreach (var action in _systemConfig) 
                    action(_context, system);

                return system;
            }

            private string GetActorSystemName(IConfiguration config)
            {
                var name = config["actorsystem"];
                return !string.IsNullOrWhiteSpace(name)
                    ? name
                    : _context.HostingEnvironment.ApplicationName.Replace('.', '-');
            }

            internal void Populate(IServiceCollection collection)
            {
                ApplyFixes(_containerBuilder, collection);
                _containerBuilder.Populate(collection);
            }

            internal IServiceProvider CreateServiceProvider() 
                => new AutofacServiceProvider(_containerBuilder.Build());

            private static void ApplyFixes(ContainerBuilder builder, IServiceCollection serviceCollection)
            {
                if(TryRemove(serviceCollection, typeof(EventLogLoggerProvider)))
                    builder.Register(c => new EventLogLoggerProvider(c.Resolve<IOptions<EventLogSettings>>())).As<ILoggerProvider>();
            }

            private static bool TryRemove(IServiceCollection collection, Type impl)
            {
                var sd = collection.FindIndex(e => e.ImplementationType == impl);
                if (sd == -1) return false;

                collection.RemoveAt(sd);
                return true;
            }
        }

        private sealed class ActorServiceProviderFactory : IServiceProviderFactory<IActorApplicationBuilder>
        {
            private readonly ActorApplicationBluilder _actorBuilder;

            public ActorServiceProviderFactory(ActorApplicationBluilder actorBuilder) => _actorBuilder = actorBuilder;

            public IActorApplicationBuilder CreateBuilder(IServiceCollection services)
            {
                _actorBuilder.Populate(services);
                return _actorBuilder;
            }

            public IServiceProvider CreateServiceProvider(IActorApplicationBuilder containerBuilder)
            {
                if (_actorBuilder != containerBuilder) throw new InvalidOperationException("Builder was replaced during Configuration");

                var prov = _actorBuilder.CreateServiceProvider();

                var system = _actorBuilder.CreateSystem(prov);
                var lifetime = prov.GetRequiredService<IHostApplicationLifetime>();

                ActorApplication.ActorSystem = system;
                ActorApplication.LoggerFactory = prov.GetRequiredService<ILoggerFactory>();
                ActorApplication.ServiceProvider = prov;

                lifetime.ApplicationStopping.Register(() => system.Terminate());
                system.RegisterOnTermination(() => lifetime.StopApplication());

                return prov;
            }
        }

        [PublicAPI]
        public static IHostBuilder ConfigureAkkaApplication(this IHostBuilder hostBuilder, Action<IActorApplicationBuilder>? config = null)
            => hostBuilder.UseServiceProviderFactory(c =>
                                                     {
                                                         var builder = new ActorApplicationBluilder(c, hostBuilder);
                                                         config?.Invoke(builder);
                                                         return new ActorServiceProviderFactory(builder);
                                                     });
    }
}