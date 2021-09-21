using System;
using System.Reactive;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public static class EventActorExtensions
    {
        public static IEventActor CreateEventActor(this IActorRefFactory system)
            => CreateEventActor(system, null);

        public static IEventActor CreatSelfKillingEventActor(this IActorRefFactory system)
            => CreatSelfKillingEventActor(system, null);

        public static IEventActor CreateEventActor(this IActorRefFactory system, string? name)
            => CreateEventActor<Unit>(system, name, null);

        public static IEventActor CreatSelfKillingEventActor(this IActorRefFactory system, string? name)
            => CreatSelfKillingEventActor<Unit>(system, name, null);

        public static IEventActor CreateEventActor<TPayload>(this IActorRefFactory system, string? name, Action<TPayload>? handler)
            => EventActor.Create(system, name, handler);

        public static IEventActor CreatSelfKillingEventActor<TPayload>(this IActorRefFactory system, string? name, Action<TPayload>? handler)
            => EventActor.CreateSelfKilling(system, name, handler);

        public static IEventActor GetOrCreateEventActor(this IUntypedActorContext system, string name)
        {
            var child = system.Child(name);

            return child.Equals(ActorRefs.Nobody)
                ? EventActor.Create(system, name)
                : EventActor.From(child);
        }

        public static IEventActor GetOrCreateSelfKillingEventActor(this IUntypedActorContext system, string name)
        {
            var child = system.Child(name);

            return child.Equals(ActorRefs.Nobody)
                ? EventActor.CreateSelfKilling(system, name)
                : EventActor.From(child);
        }
    }
}