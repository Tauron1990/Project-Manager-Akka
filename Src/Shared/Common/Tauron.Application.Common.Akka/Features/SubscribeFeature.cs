using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

public sealed class SubscribeFeature : IFeature<SubscribeFeature.State>
{
    IEnumerable<string> IFeature.Identify()
    {
        yield return nameof(SubscribeFeature);
    }

    public void PostStop()
    {
        
    }

    public void PreStart()
    {
    }

    void IFeature<State>.Init(IFeatureActor<State> actor)
    {
        actor.Receive<Terminated>(obs => obs.ToUnit());
        actor.Receive<KeyHint>(obs => obs.Select(data => data.State.Update(data.Event.Key, refs => refs.Remove(data.Event.Target))));

        actor.Receive<EventSubscribe>(
            obs => obs.Where(_ => !actor.Sender.IsNobody())
               .Do(
                    statePair => statePair.Event.Watch.WhenTrue(
                        ()
                            => actor.Context.WatchWith(actor.Sender, new KeyHint(actor.Sender, statePair.Event.Event))))
               .Select(
                    statePair =>
                    {
                        var ((_, @event), state, _) = statePair;

                        actor.TellSelf(new InternalEventSubscription(actor.Sender, @event));

                        return state.Update(@event, refs => refs.Add(actor.Sender));
                    }));

        actor.Receive<EventUnSubscribe>(
            obs => obs.Where(_ => !actor.Sender.IsNobody())
               .Select(
                    statePair =>
                    {
                        actor.Context.Unwatch(actor.Sender);
                        var (eventUnSubscribe, state, _) = statePair;

                        return state.Update(eventUnSubscribe.Event, refs => refs.Remove(actor.Sender));
                    }));

        actor.Receive<SendEvent>(
            obs => obs.ToUnit(
                statePair =>
                {
                    var ((@event, eventType), state, _) = statePair;

                    if (state.Subscriptions.TryGetValue(eventType, out var intrests))
                        intrests.ForEach(actorRef => actorRef.Tell(@event));
                }));
    }

    public void Dispose() { }

    public void AddResource(IDisposable res) => throw new NotSupportedException("ResourceHolding not Supported");

    public void RemoveResource(IDisposable res) => throw new NotSupportedException("ResourceHolding not Supported");

    [PublicAPI]
    public static IPreparedFeature New()
        => Feature.Create(
            () => new SubscribeFeature(),
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
                return this with { Subscriptions = Subscriptions.Remove(type) };

            return this with { Subscriptions = Subscriptions.SetItem(type, list) };
        }
    }

    private sealed record KeyHint(IActorRef Target, Type Key);

    public sealed record InternalEventSubscription(IActorRef Intrest, Type Type);
}

[PublicAPI]
public sealed record EventUnSubscribe(Type Event);

[PublicAPI]
#pragma warning disable AV1564
public sealed record EventSubscribe(bool Watch, Type Event);
#pragma warning restore AV1564

public sealed record SendEvent(object Event, Type EventType)
{
    [PublicAPI]
    public static SendEvent Create<TType>(TType evt)
        where TType : notnull
        => new(evt, typeof(TType));
}