using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stl.Collections;
using Tauron.Application;

namespace Tauron.AkkaHost;

internal sealed class ActorServiceProviderFactory : IServiceProviderFactory<IActorApplicationBuilder>
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

        ServiceProvider prov = _actorBuilder.Collection.BuildServiceProvider();
        TauronEnviromentSetup.Run(prov);

        var lifetime = prov.GetRequiredService<IHostApplicationLifetime>();

        var system = prov.GetRequiredService<ActorSystem>();
        ActorApplication.ActorSystem = system;
        
        lifetime.ApplicationStopping.Register(() => system.Terminate());

        return prov;
    }
}