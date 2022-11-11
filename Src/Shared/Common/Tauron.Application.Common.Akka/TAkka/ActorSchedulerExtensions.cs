using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public static class ActorSchedulerExtensions
{
    public static IObservable<TType> ObserveOnSelf<TType>(this IObservable<TType> observable)
        => observable.ObserveOn(ActorScheduler.CurrentSelf);

    public static IObservable<TType> ObserveOn<TType>(this IObservable<TType> observable, IActorRef target)
        => observable.ObserveOn(ActorScheduler.From(target));
}