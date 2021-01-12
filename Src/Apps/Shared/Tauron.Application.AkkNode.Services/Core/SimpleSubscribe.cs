using System;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Features;

namespace Tauron.Application.AkkNode.Services.Core
{

    [PublicAPI]
    public static class SimpleSubscribe
    {
        public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, bool killOnFirstResponse = false)
        {
            var eventActor = EventActor.Create(actor, null, killOnFirstResponse);
            eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));
            return eventActor;
        }

        public static IEventActor SubscribeToEvent<TEvent>(this IActorRefFactory actor, IActorRef target, Action<TEvent> handler, bool killOnFirstResponse = false)
        {
            var eventActor = EventActor.Create(actor, handler, killOnFirstResponse);
            eventActor.Send(target, new EventSubscribe(true, typeof(TEvent)));
            return eventActor;
        }

        public static EventSubscribtion SubscribeToEvent<TEvent>(this IActorRef eventSource)
        {
            eventSource.Tell(new EventSubscribe(true, typeof(TEvent)));
            return new EventSubscribtion(typeof(TEvent), eventSource);
        }
    }

    [PublicAPI]
    public sealed class EventSubscribtion : IDisposable
    {
        public static EventSubscribtion Empty { get; } = new(typeof(Type), ActorRefs.Nobody);

        private readonly Type _event;
        private readonly IActorRef _eventSource;

        public EventSubscribtion(Type @event, IActorRef eventSource)
        {
            _event = @event;
            _eventSource = eventSource;
        }

        public void Dispose()
        {
            if(_eventSource.IsNobody()) return;
            _eventSource.Tell(new EventUnSubscribe(_event));
        }
    }
}