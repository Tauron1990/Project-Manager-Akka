using System.Reactive;
using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public interface IFeatureActor<TState> : IResourceHolder, IObservable<TState>, IWithTimers
{
    IObservable<IActorContext> Start { get; }

    IObservable<IActorContext> Stop { get; }

    TState CurrentState { get; }

    IActorRef Self { get; }

    IActorRef Parent { get; }

    IActorRef Sender { get; }

    IUntypedActorContext Context { get; }

    public bool CallSingleHandler { get; set; }

    SupervisorStrategy? SupervisorStrategy { get; set; }

    IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match);

    void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler);
    void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler);
    void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);
    void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler);

    void UpdateState(TState state);

    void TellSelf(object msg);
    IObservable<TEvent> Receive<TEvent>();
}