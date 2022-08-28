using Akka.Actor;
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
        private readonly List<Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder>> _config = new();

        private readonly IHostBuilder _builder;
        private HostBuilderContext? _context;

        public IServiceCollection Collection { get; set; } = new ServiceCollection();

        internal ActorApplicationBluilder(IHostBuilder builder)
            => _builder = builder;

        public void Init(HostBuilderContext context)
            => _context = context;
        
        public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, AkkaConfigurationBuilder> system)
            => ConfigureAkka((h, _, c) => system(h, c));

        public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder> system)
        {
            _config.Add(system);

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
            Collection.AddAkka(GetActorSystemName(_context.Configuration), (builder, provider) => _config.ForEach(a => a(_context, provider, builder)));
        }
    }


    private sealed class ActorServiceProviderFactory : IServiceProviderFactory<IActorApplicationBuilder>
    {
        private readonly ActorApplicationBluilder _actorBuilder;

        internal ActorServiceProviderFactory(ActorApplicationBluilder actorBuilder) => _actorBuilder = actorBuilder;

        public IActorApplicationBuilder CreateBuilder(IServiceCollection services)
        {
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