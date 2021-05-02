using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features
{
    public sealed class SubscribeFeature : IFeature<SubscribeFeature.State>
    {
        IEnumerable<string> IFeature.Identify()
        {
            yield return nameof(SubscribeFeature);
        }

        void IFeature<State>.Init(IFeatureActor<State> actor)
        {
            actor.Receive<Terminated>(obs => obs.ToUnit());
            actor.Receive<KeyHint>(obs => obs.Select(data => data.State.Update(data.Event.Key, refs => refs.Remove(data.Event.Target))));

            actor.Receive<EventSubscribe>(
                obs => obs.Where(_ => !actor.Sender.IsNobody())
                          .Do(m => m.Event.Watch.WhenTrue(()
                                                              => actor.Context.WatchWith(actor.Sender, new KeyHint(actor.Sender!, m.Event.Event))))
                          .Select(m =>
                                  {
                                      var ((_, @event), state, _) = m;

                                      actor.TellSelf(new InternalEventSubscription(actor.Sender, @event));
                                      return state.Update(@event, refs => refs.Add(actor.Sender));
                                  }));

            actor.Receive<EventUnSubscribe>(
                obs => obs.Where(_ => !actor.Sender.IsNobody())
                          .Select(m =>
                                  {
                                      actor.Context.Unwatch(actor.Sender!);
                                      var (eventUnSubscribe, state, _) = m;
                                      return state.Update(eventUnSubscribe.Event, refs => refs.Remove(actor.Sender));
                                  }));

            actor.Receive<SendEvent>(
                obs => obs.ToUnit(m =>
                                  {
                                      var ((@event, eventType), state, _) = m;

                                      if (state.Subscriptions.TryGetValue(eventType, out var intrests))
                                          intrests.ForEach(r => r.Tell(@event));
                                  }));
        }

        [PublicAPI]
        public static IPreparedFeature New()
            => Feature.Create(() => new SubscribeFeature(),
                new State(ImmutableDictionary<Type, ImmutableList<IActorRef>>.Empty));

        public sealed record State(ImmutableDictionary<Type, ImmutableList<IActorRef>> Subscriptions)
        {
            public State Update(Type type, Func<ImmutableList<IActorRef>, ImmutableList<IActorRef>> listUpdate)
            {
                if (!Subscriptions.TryGetValue(type, out var list))
                    return this with
                    {
                        Subscriptions = Subscriptions.SetItem(type, listUpdate(ImmutableList<IActorRef>.Empty))
                    };

                list = listUpdate(list);
                if (list.IsEmpty)
                    return this with {Subscriptions = Subscriptions.Remove(type)};

                return this with {Subscriptions = Subscriptions.SetItem(type, list)};
            }
        }

        private sealed record KeyHint(IActorRef Target, Type Key);

        public sealed record InternalEventSubscription(IActorRef Intrest, Type Type);
    }

    [PublicAPI]
    public sealed record EventUnSubscribe(Type Event);

    [PublicAPI]
    public sealed record EventSubscribe(bool Watch, Type Event);

    public sealed record SendEvent(object Event, Type EventType)
    {
        [PublicAPI]
        public static SendEvent Create<TType>(TType evt)
            where TType : notnull
            => new(evt, typeof(TType));
    }
}