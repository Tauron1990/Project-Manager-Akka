using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public static class ObservableExtensions
{
    public static IObservable<TState> ToSelfWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Self.Tell(p.Event, p.Self))
           .Select(p => p.State);
    
    public static IObservable<TState> ToParentWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Parent.Tell(p.Event, p.Self))
           .Select(p => p.State);
    
    public static IObservable<TState> ToSenderWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Sender.Tell(p.Event, p.Self))
           .Select(p => p.State);
    
    public static IObservable<TState> ForewardToSelfWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Self.Forward(p.Event))
           .Select(p => p.State);
    
    public static IObservable<TState> ForewardToParentWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Parent.Forward(p.Event))
           .Select(p => p.State);
    
    public static IObservable<TState> ForewardToSenderWithState<TState, TEvent>(this IObservable<StatePair<TEvent, TState>> evt)
        => evt.Do(p => p.Sender.Forward(p.Event))
           .Select(p => p.State);

    public static IObservable<StatePair<TEvent, TState>> UpdateState<TEvent, TState>(this IObservable<StatePair<TEvent, TState>> obs)
        where TState : IApplyEvent<TState, TEvent>
        => obs.Select(p => p with { State = p.State.Apply(p.Event) });
}