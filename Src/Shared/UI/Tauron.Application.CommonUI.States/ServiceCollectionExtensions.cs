using JetBrains.Annotations;
using Tauron.AkkaHost;
using Tauron.Application.CommonUI.Model;

namespace Tauron.Application.CommonUI;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IActorApplicationBuilder RegisterModelActor<TActor>(this IActorApplicationBuilder builder)
        where TActor : ActorModel
        => builder.StartActors((system, registry, resolver) 
            => registry.Register<TActor>(system.ActorOf(resolver.Props<TActor>(), typeof(TActor).Name)));
}