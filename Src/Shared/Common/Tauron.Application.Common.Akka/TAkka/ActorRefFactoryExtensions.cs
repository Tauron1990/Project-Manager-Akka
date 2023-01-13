using System.Linq.Expressions;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public static class ActorRefFactoryExtensions
{
    public static IActorRef GetOrAdd<TActor>(this IActorContext context, string? name)
        where TActor : ActorBase, new()
        => GetOrAdd(context, name, Props.Create<TActor>());

    public static IActorRef GetOrAdd(this IActorContext context, string? name, Props props)
    {
        IActorRef? child = context.Child(name);

        return child.Equals(ActorRefs.Nobody) ? context.ActorOf(props, name) : child;
    }

    public static IActorRef ActorOf<TActor>(
        this IActorRefFactory fac, Expression<Func<TActor>> creator,
        string? name = null) where TActor : ActorBase => fac.ActorOf(Props.Create(creator), name);

    //public static IActorRef ActorOf<TActor>(this IActorRefFactory fac, string? name = null) where TActor : ActorBase
    //    => fac.ActorOf(Props.Create<TActor>(), name);
}