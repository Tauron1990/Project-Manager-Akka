using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Features;
using Tauron.TAkka;

namespace Tauron.Application.AkkaNode.Services.Core;

[PublicAPI]
public static class SimpleSubscribe
{
    public static IEventActor SubscribeToSelfKillingEvent<TEvent>(this IActorRefFactory actor, IActorRef target)
    {
        IEventActor eventActor = EventActor.CreateSelfKilling(actor, null);
        eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToSelfKillingEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler)
    {
        IEventActor eventActor = EventActor.CreateSelfKilling(actor, null, handler);
        eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target)
    {
        IEventActor eventActor = EventActor.Create(actor, null);
        eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler, bool killOnFirstResponse = false)
    {
        IEventActor eventActor = EventActor.Create(actor, null, handler);
        eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));

        return eventActor;
    }

    public static EventSubscribtion SubscribeToEvent<TEvent>(this IActorRef eventSource)
    {
        eventSource.Tell(new EventSubscribe(true, typeof(TEvent)));

        return new EventSubscribtion(typeof(TEvent), eventSource);
    }
}