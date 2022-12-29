using System.Diagnostics;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.TAkka;

namespace Tauron.Features;

[PublicAPI]
[DebuggerStepThrough]
public static class FeatureActorExtensions
{
    public static IObservable<StatePair<TEvent, TState>> SyncState<TEvent, TState>(
        this IObservable<StatePair<TEvent, TState>> observable, IFeatureActor<TState> actor)
    {
        return observable.ObserveOn(ActorScheduler.From(actor.Self))
           .Select(evt => evt with { State = actor.CurrentState });
    }

    public static IActorRef ActorOf(this IActorRefFactory factory, string? name, params IPreparedFeature[] features)
        => factory.ActorOf(GenericActor.Create(features), name);

    public static IActorRef ActorOf(this IActorRefFactory factory, params IPreparedFeature[] features)
        => ActorOf(factory, null, features);
}