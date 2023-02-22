using Akka.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tauron.AkkaHost;

internal sealed class ActorApplicationBluilder : IActorApplicationBuilder
{
    private readonly IHostBuilder _builder;

    private List<Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder>>? _config = new();
    private HostBuilderContext? _context;

    internal ActorApplicationBluilder(IHostBuilder builder)
        => _builder = builder;

    internal IServiceCollection Collection { get; set; } = new ServiceCollection();

    internal bool ConfigurationFinisht { get; set; }

    public IActorApplicationBuilder ConfigureHost(Action<IHostBuilder> configure)
    {
        configure(_builder);
        return this;
    }

    public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, AkkaConfigurationBuilder> system)
        => ConfigureAkka((h, _, c) => system(h, c));

    public IActorApplicationBuilder ConfigureAkka(Action<HostBuilderContext, IServiceProvider, AkkaConfigurationBuilder> system)
    {
        if(_config is null)
            throw new InvalidOperationException("Akka Configuration is already Finisht");

        _config.Add(system);

        return this;
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

    internal void Init(HostBuilderContext context)
        => _context = context;

    private void CheckConfigFinisht()
    {
        if(ConfigurationFinisht)
            throw new InvalidOperationException("The Configuration Process is already Finisht");
    }

    private string GetActorSystemName(IConfiguration config)
    {
        if(_context is null)
            throw new InvalidOperationException("HostBuilder Context is Null");

        string? name = config["actorsystem"];

        return !string.IsNullOrWhiteSpace(name)
            ? name
            : _context.HostingEnvironment.ApplicationName.Replace('.', '-');
    }

    internal void AddAkka()
    {
        if(_context is null)
            throw new InvalidOperationException("HostBuilder Context is Null");

#pragma warning disable GU0011
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