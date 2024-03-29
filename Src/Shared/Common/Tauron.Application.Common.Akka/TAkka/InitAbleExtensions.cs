﻿using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public static class InitableExtensions
{
    public static Task<TResult> Ask<TResult>(this IInitableActorRef model, object message, TimeSpan? timeout = null)
        => model.Actor.Ask<TResult>(message, timeout);

    public static void Tell(this IInitableActorRef model, object msg)
        => Tell(model, msg, ActorRefs.NoSender);

    public static void Tell(this IInitableActorRef model, object msg, IActorRef sender)
        => model.Actor.Tell(msg, sender);
}