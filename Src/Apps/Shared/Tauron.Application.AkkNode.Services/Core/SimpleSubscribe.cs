﻿using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.TAkka;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.Core;

[PublicAPI]
public static class SimpleSubscribe
{
    public static IEventActor SubscribeToSelfKillingEvent<TEvent>(this IActorRefFactory actor, IActorRef target)
    {
        var eventActor = EventActor.CreateSelfKilling(actor, null);
        eventActor.Send(target, new EventSubscribe(Watch: true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToSelfKillingEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler)
    {
        var eventActor = EventActor.CreateSelfKilling(actor, null, handler);
        eventActor.Send(target, new EventSubscribe(Watch: true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target)
    {
        var eventActor = EventActor.Create(actor, null);
        eventActor.Send(target, new EventSubscribe(Watch: true, typeof(TEvent)));

        return eventActor;
    }

    public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler, bool killOnFirstResponse = false)
    {
        var eventActor = EventActor.Create(actor, null, handler);
        eventActor.Send(target, new EventSubscribe(Watch: true, typeof(TEvent)));

        return eventActor;
    }

    public static EventSubscribtion SubscribeToEvent<TEvent>(this IActorRef eventSource)
    {
        eventSource.Tell(new EventSubscribe(Watch: true, typeof(TEvent)));

        return new EventSubscribtion(typeof(TEvent), eventSource);
    }
}

[PublicAPI]
public sealed class EventSubscribtion : IDisposable
{
    private readonly Type _event;
    private readonly IActorRef _eventSource;

    public EventSubscribtion(Type @event, IActorRef eventSource)
    {
        _event = @event;
        _eventSource = eventSource;
    }

    public static EventSubscribtion Empty { get; } = new(typeof(Type), ActorRefs.Nobody);

    public void Dispose()
    {
        if (_eventSource.IsNobody()) return;

        _eventSource.Tell(new EventUnSubscribe(_event));
    }
}