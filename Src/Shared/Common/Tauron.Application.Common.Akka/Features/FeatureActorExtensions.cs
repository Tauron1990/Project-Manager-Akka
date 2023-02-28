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
        => ActorOf(factory, default, features);
    
    public static IActorRef ActorOf(this IActorRefFactory factory, string? name, IPreparedFeature feature)
        => factory.ActorOf(GenericActor.Create(feature), name);

    public static IActorRef ActorOf(this IActorRefFactory factory, IPreparedFeature feature)
        => ActorOf(factory, default(string), feature);
    
    public static IActorRef ActorOf(this IActorRefFactory factory, string? name, IPreparedFeature feature1, IPreparedFeature feature2)
        => factory.ActorOf(GenericActor.Create(feature1, feature2), name);

    public static IActorRef ActorOf(this IActorRefFactory factory, IPreparedFeature feature1, IPreparedFeature feature2)
        => ActorOf(factory, default(string), feature1, feature2);
    
    public static IActorRef ActorOf(this IActorRefFactory factory, string? name, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3)
        => factory.ActorOf(GenericActor.Create(feature1, feature2, feature3), name);

    public static IActorRef ActorOf(this IActorRefFactory factory, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3)
        => ActorOf(factory, default(string), feature1, feature2, feature3);
    
    public static IActorRef ActorOf(this IActorRefFactory factory, string? name, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3, IPreparedFeature feature4)
        => factory.ActorOf(GenericActor.Create(feature1, feature2, feature3, feature4), name);

    public static IActorRef ActorOf(this IActorRefFactory factory, IPreparedFeature feature1, IPreparedFeature feature2, IPreparedFeature feature3, IPreparedFeature feature4)
        => ActorOf(factory, default(string), feature1, feature2, feature3, feature4);

}