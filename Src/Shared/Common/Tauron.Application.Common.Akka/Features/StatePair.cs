using System.Diagnostics;
using System.Reactive.Linq;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
[DebuggerStepThrough]
public sealed record StatePair<TEvent, TState>(TEvent Event, TState State, ITimerScheduler Timers, IActorContext Context, IActorRef Sender, IActorRef Parent, IActorRef Self)
    : IObservable<StatePair<TEvent, TState>>
{
    IDisposable IObservable<StatePair<TEvent, TState>>.Subscribe(IObserver<StatePair<TEvent, TState>> observer)
        => Observable.Return(this).Subscribe(observer);

    public StatePair<TEvent, TNew> Convert<TNew>(Func<TState, TNew> converter)
        => new(Event, converter(State), Timers, Context, Sender, Parent, Self);


    #pragma warning disable AV1551
    public void Deconstruct(out TEvent evt, out TState state)
        #pragma warning restore AV1551
    {
        evt = Event;
        state = State;
    }

    public StatePair<TNew, TState> NewEvent<TNew>(TNew evt)
        => NewEvent(evt, State);

    public StatePair<TNew, TState> NewEvent<TNew>(TNew evt, TState state)
        => new(evt, state, Timers, Context, Sender, Parent, Self);

    public void Deconstruct(out TEvent evt, out TState state, out ITimerScheduler scheduler)
    {
        evt = Event;
        state = State;
        scheduler = Timers;
    }
}