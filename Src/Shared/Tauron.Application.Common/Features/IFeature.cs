using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Features;

public interface IFeature : IResourceHolder
{
    IEnumerable<string> Identify();
}

public interface IFeature<TState> : IFeature
{
    void Init(IFeatureActor<TState> actor);
}

[PublicAPI]
public abstract class ActorFeatureBase<TState> : IFeature<TState>, IFeatureActor<TState>
{
    private IFeatureActor<TState> _actor = null!;

    public IActorContext Context { get; private set; } = null!;

    public virtual IEnumerable<string> Identify()
    {
        yield return GetType().Name;
    }

    public void Init(IFeatureActor<TState> actor)
    {
        Context = actor.Context;
        _actor = actor;
        Timers = actor.Timers;
        Config();
    }

    void IDisposable.Dispose()
    {
        _actor.Dispose();
        GC.SuppressFinalize(this);
    }

    void IResourceHolder.AddResource(IDisposable res) => _actor.AddResource(res);

    void IResourceHolder.RemoveResource(IDisposable res) => _actor.RemoveResource(res);

    public bool CallSingleHandler
    {
        get => _actor.CallSingleHandler;
        set => _actor.CallSingleHandler = value;
    }

    public IObservable<IActorContext> Start => _actor.Start;

    public IObservable<IActorContext> Stop => _actor.Stop;

    public TState CurrentState => _actor.CurrentState;

    public IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match)
        => _actor.WaitForSignal(timeout, match);

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
        => _actor.Receive(handler);

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
        => _actor.Receive(handler);

    public void Receive<TEvent>(
        Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler,
        Func<Exception, bool> errorHandler) => _actor.Receive(handler, errorHandler);

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
        => _actor.Receive(handler);

    public void UpdateState(TState state) => _actor.UpdateState(state);

    public void TellSelf(object msg) => _actor.TellSelf(msg);

    public ILoggingAdapter Log => _actor.Log;

    public IObservable<TEvent> Receive<TEvent>() => _actor.Receive<TEvent>();

    public IActorRef Self => _actor.Self;

    public IActorRef Parent => _actor.Parent;

    public IActorRef Sender => _actor.Sender;

    IUntypedActorContext IFeatureActor<TState>.Context => _actor.Context;

    public SupervisorStrategy? SupervisorStrategy
    {
        get => _actor.SupervisorStrategy;
        set => _actor.SupervisorStrategy = value;
    }

    public IDisposable Subscribe(IObserver<TState> observer) => _actor.Subscribe(observer);

    public ITimerScheduler Timers { get; set; } = null!;

    protected virtual void Config() => ConfigImpl();

    protected abstract void ConfigImpl();

    protected IObservable<TType> SyncActor<TType>(TType element)
        => Observable.Return(element, ActorScheduler.From(Self));

    protected IObservable<TType> SyncActor<TType>(IObservable<TType> toSync)
        => toSync.ObserveOn(ActorScheduler.From(Self));

    protected IObservable<StatePair<TType, TState>> UpdateAndSyncActor<TType>(StatePair<TType, TState> element)
        => from toUpdate in Observable.Return(element, ActorScheduler.From(Self))
           select toUpdate with { State = CurrentState };

    protected IObservable<StatePair<TType, TState>> UpdateAndSyncActor<TType>(IObservable<StatePair<TType, TState>> toSync)
        => from toUpdate in toSync.ObserveOn(ActorScheduler.From(Self))
           select toUpdate with { State = CurrentState };

    protected IObservable<StatePair<TType, TState>> UpdateAndSyncActor<TType>(TType element)
        => from syncElement in Observable.Return(element, ActorScheduler.From(Self))
           let toUpdate = new StatePair<TType, TState>(syncElement, CurrentState, Timers, Context, Sender, Parent, Self)
           select toUpdate with { State = CurrentState };

    protected IObservable<StatePair<TType, TState>> UpdateAndSyncActor<TType>(IObservable<TType> toSync)
        => from element in toSync.ObserveOn(ActorScheduler.From(Self))
           let toUpdate = new StatePair<TType, TState>(element, CurrentState, Timers, Context, Sender, Parent, Self)
           select toUpdate with { State = CurrentState };
}