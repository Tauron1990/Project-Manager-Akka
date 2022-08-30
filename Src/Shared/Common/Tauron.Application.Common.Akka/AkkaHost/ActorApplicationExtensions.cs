﻿using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Hosting;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stl.Collections;
using Tauron.Application;

namespace Tauron.AkkaHost;

public static class ActorApplicationExtensions
{
    [PublicAPI]
    public static IHostBuilder ConfigureAkkaApplication(this IHostBuilder hostBuilder, Action<IActorApplicationBuilder>? config = null)
    {
        var builder = new ActorApplicationBluilder(hostBuilder);
        config?.Invoke(builder);
        
        return hostBuilder.UseServiceProviderFactory(
            builderContext =>
            {
                builder.Init(builderContext);
                return new ActorServiceProviderFactory(builder);
            });
    }

    private sealed class ActorApplicationBluilder : IActorApplicationBuilder
    {
        private readonly IHostBuilder _builder;
        
        private List<Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder>>? _config = new();
        private HostBuilderContext? _context;

        public IServiceCollection Collection { get; set; } = new ServiceCollection();

        public bool ConfigurationFinisht { get; set; }
        
        internal ActorApplicationBluilder(IHostBuilder builder)
            => _builder = builder;

        public void Init(HostBuilderContext context)
            => _context = context;
        
        public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, AkkaConfigurationBuilder> system)
            => ConfigureAkka((h, _, c) => system(h, c));

        public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder> system)
        {
            if(_config is null)
                throw new InvalidOperationException("Akka Configuration is already Finisht");
            _config.Add(system);

            return this;
        }

        private void CheckConfigFinisht()
        {
            if(ConfigurationFinisht)
                throw new InvalidOperationException("The Configuration Process is already Finisht");
        }
        

        public IActorApplicationBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            CheckConfigFinisht();
            _builder.ConfigureHostConfiguration(configureDelegate);

            return this;
        }

        public IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            CheckConfigFinisht();
            _builder.ConfigureAppConfiguration(configureDelegate);

            return this;
        }

        public IActorApplicationBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if(ConfigurationFinisht)
                configureDelegate(_context!, Collection);
            else
                _builder.ConfigureServices(configureDelegate);

            return this;
        }

        public IActorApplicationBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _builder.ConfigureContainer(configureDelegate);

            return this;
        }

        private string GetActorSystemName(IConfiguration config)
        {
            if(_context is null)
                throw new InvalidOperationException("HostBuilder Context is Null");
            
            var name = config["actorsystem"];
            
            return !string.IsNullOrWhiteSpace(name)
                ? name
                : _context.HostingEnvironment.ApplicationName.Replace('.', '-');
        }

        public void AddAkka()
        {
            if(_context is null)
                throw new InvalidOperationException("HostBuilder Context is Null");

            Collection.AddAkka(
                GetActorSystemName(_context.Configuration),
                (builder, provider) =>
                {
                    if(_config is null)
                        throw new InvalidOperationException("Akka _config is null");
                    
                    _config.ForEach(a => a(_context, provider, builder));
                    _config = null;
                });
        }
    }


    private sealed class ActorServiceProviderFactory : IServiceProviderFactory<IActorApplicationBuilder>
    {
        private readonly ActorApplicationBluilder _actorBuilder;

        internal ActorServiceProviderFactory(ActorApplicationBluilder actorBuilder) => _actorBuilder = actorBuilder;

        public IActorApplicationBuilder CreateBuilder(IServiceCollection services)
        {
            _actorBuilder.ConfigurationFinisht = true;
            services.AddRange(_actorBuilder.Collection);
            _actorBuilder.Collection = services;

            return _actorBuilder;
        }


        public IServiceProvider CreateServiceProvider(IActorApplicationBuilder containerBuilder)
        {
            if(_actorBuilder != containerBuilder) throw new InvalidOperationException("Builder was replaced during Configuration");

            _actorBuilder.AddAkka();
            var prov = _actorBuilder.Collection.BuildServiceProvider();

            var lifetime = prov.GetRequiredService<IHostApplicationLifetime>();

            var system = prov.GetRequiredService<ActorSystem>();
            new TauronEnviromentSetup().Run(prov);

            lifetime.ApplicationStopping.Register(() => system.Terminate());

            return prov;
        }
    }
}