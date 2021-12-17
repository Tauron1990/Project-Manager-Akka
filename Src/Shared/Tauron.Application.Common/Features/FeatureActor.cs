using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Event;
using JetBrains.Annotations;
using IResourceHolder = Tauron.TAkka.IResourceHolder;
using ObservableActor = Tauron.TAkka.ObservableActor;

namespace Tauron.Features;

[PublicAPI]
public interface IFeatureActor<TState> : IResourceHolder, IObservable<TState>, IWithTimers
{
    IObservable<IActorContext> Start { get; }

    IObservable<IActorContext> Stop { get; }

    TState CurrentState { get; }

    ILoggingAdapter Log { get; }

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

[PublicAPI, DebuggerStepThrough]
public sealed record StatePair<TEvent, TState>(TEvent Event, TState State, ITimerScheduler Timers, IActorContext Context, IActorRef Sender, IActorRef Parent, IActorRef Self)
    : IObservable<StatePair<TEvent, TState>>
{
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

    IDisposable IObservable<StatePair<TEvent, TState>>.Subscribe(IObserver<StatePair<TEvent, TState>> observer)
        => Observable.Return(this).Subscribe(observer);
}

[PublicAPI, DebuggerStepThrough]
public abstract class FeatureActorBase<TFeatured, TState> : ObservableActor, IFeatureActor<TState>
    where TFeatured : FeatureActorBase<TFeatured, TState>, new()
{
    private readonly HashSet<string> _featureIds = new();
    private BehaviorSubject<TState>? _currentState;
    private IActorRef _parent = ActorRefs.Nobody;

    private IActorRef _self = ActorRefs.Nobody;

    private BehaviorSubject<TState> CurrentState
    {
        get
        {
            if (_currentState == null)
                throw new InvalidOperationException("The Actor was Not Initialized Propertly");

            return _currentState;
        }
    }

    IActorRef IFeatureActor<TState>.Self => _self;

    IActorRef IFeatureActor<TState>.Parent => _parent;

    public ITimerScheduler Timers { get; set; } = null!;

    TState IFeatureActor<TState>.CurrentState => CurrentState.Value;

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
        => Receive<TEvent>(
            obs
                => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))));

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
    {
        IDisposable CreateHandler(IObservable<TEvent> observable)
            => handler(observable.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self)))
               .SubscribeWithStatus(UpdateState);

        Receive<TEvent>(obs => CreateHandler(obs));
    }

    public void Receive<TEvent>(
        Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler,
        Func<Exception, bool> errorHandler)
        => Receive<TEvent>(
            obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))),
            errorHandler);

    public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
        => Receive<TEvent>(
            obs
                => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))));

    public void UpdateState(TState state)
    {
        if (InternalCurrentActorCellKeeper.Current is null)
            throw new NotSupportedException("There is no active ActorContext, this is most likely due to use of async operations from within this actor.");

        CurrentState.OnNext(state);
    }

    IUntypedActorContext IFeatureActor<TState>.Context => Context;
    SupervisorStrategy? IFeatureActor<TState>.SupervisorStrategy { get; set; }

    public IDisposable Subscribe(IObserver<TState> observer)
        => CurrentState.DistinctUntilChanged().Subscribe(observer);

    protected internal static Props Create(
        Func<IUntypedActorContext, TState> initialState,
        Action<ActorBuilder<TState>> builder)
        => Props.Create(typeof(ActorFactory), builder, initialState);

    protected static Props Create(TState initialState, Func<IEnumerable<Action<ActorBuilder<TState>>>> builder)
        => Create(_ => initialState, actorBuilder => builder().Foreach(action => action(actorBuilder)));

    protected static Props Create(TState initialState, Action<ActorBuilder<TState>> builder)
        => Create(_ => initialState, builder);

    private void InitialState(TState initial)
    {
        _self = Self;
        _parent = Parent;
        _currentState = new BehaviorSubject<TState>(initial);
    }

    private void RegisterFeature(IFeature<TState> feature)
    {
        if (feature.Identify().Any(id => !_featureIds.Add(id)))
            throw new InvalidOperationException("Duplicate Feature Added");

        feature.Init(this);
    }

    protected override SupervisorStrategy SupervisorStrategy()
        => ((IFeatureActor<TState>)this).SupervisorStrategy ?? base.SupervisorStrategy();
        
    [DebuggerStepThrough]
    private sealed class ActorFactory : IIndirectActorProducer
    {
        private readonly Action<ActorBuilder<TState>> _builder;
        private readonly Func<IUntypedActorContext, TState> _initialState;

        #pragma warning disable GU0073
        public ActorFactory(Action<ActorBuilder<TState>> builder, Func<IUntypedActorContext, TState> initialState)
            #pragma warning restore GU0073
        {
            _builder = builder;
            _initialState = initialState;
        }

        public ActorBase Produce()
        {
            var fut = new TFeatured();
            fut.InitialState(_initialState(Context));
            _builder(new ActorBuilder<TState>(fut.RegisterFeature));

            return fut;
        }

        public void Release(ActorBase actor) { }

        public Type ActorType { get; } = typeof(TFeatured);
    }

    [PublicAPI, DebuggerStepThrough]
    public static class Make
    {
        public static Action<ActorBuilder<TState>> Feature(Action<IFeatureActor<TState>> initializer, params string[] ids)
            => actorBuilder => actorBuilder.WithFeature(new DelegatingFeature(initializer, ids));
    }

    [PublicAPI, DebuggerStepThrough]
    public static class Simple
    {
        public static IFeature<TState> Feature(Action<IFeatureActor<TState>> initializer, params string[] ids)
            => new DelegatingFeature(initializer, ids);
    }

    [DebuggerStepThrough]
    private class DelegatingFeature : IFeature<TState>
    {
        private readonly IEnumerable<string> _ids;
        private readonly Action<IFeatureActor<TState>> _initializer;
        private IFeatureActor<TState>? _actor;

        internal DelegatingFeature(Action<IFeatureActor<TState>> initializer, IEnumerable<string> ids)
        {
            _initializer = initializer;
            _ids = ids;
        }

        public IEnumerable<string> Identify() => _ids;

        public void Init(IFeatureActor<TState> actor)
        {
            _actor = actor;
            _initializer(actor);
        }

        void IDisposable.Dispose() => _actor?.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _actor?.AddResource(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _actor?.RemoveResource(res);
    }
}

public sealed record EmptyState
{
    public static readonly EmptyState Inst = new();
}

[PublicAPI]
[DebuggerStepThrough]
public sealed class ActorBuilder<TState>
{
    private readonly Action<IFeature<TState>> _registrar;

    public ActorBuilder(Action<IFeature<TState>> registrar) => _registrar = registrar;

    #pragma warning disable AV1551
    public ActorBuilder<TState> WithFeature(IFeature<TState> feature)
        #pragma warning restore AV1551
    {
        _registrar(feature);

        return this;
    }

    public ActorBuilder<TState> WithFeatures(IEnumerable<IFeature<TState>> features)
    {
        foreach (var feature in features)
            _registrar(feature);

        return this;
    }

    public ActorBuilder<TState> WithFeature<TNewState>(
        IFeature<TNewState> feature, Func<TState, TNewState> convert,
        Func<TState, TNewState, TState> convertBack)
        => WithFeature(new ConvertingFeature<TNewState, TState>(feature, convert, convertBack));

    [DebuggerStepThrough]
    internal class ConvertingFeature<TTarget, TOriginal> : IFeature<TOriginal>
    {
        private readonly Func<TOriginal, TTarget> _convert;
        private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;
        private readonly IFeature<TTarget> _feature;

        internal ConvertingFeature(
            IFeature<TTarget> feature, Func<TOriginal, TTarget> convert,
            Func<TOriginal, TTarget, TOriginal> convertBack)
        {
            _feature = feature;
            _convert = convert;
            _convertBack = convertBack;
        }

        public IEnumerable<string> Identify() => _feature.Identify();

        public virtual void Init(IFeatureActor<TOriginal> actor)
            => _feature.Init(new StateDelegator<TTarget, TOriginal>(actor, _convert, _convertBack));

        void IDisposable.Dispose() => _feature.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _feature.AddResource(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _feature.RemoveResource(res);
    }

    [DebuggerStepThrough]
    private sealed class StateDelegator<TTarget, TOriginal> : IFeatureActor<TTarget>
    {
        private readonly Func<TOriginal, TTarget> _convert;
        private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;
        private readonly IFeatureActor<TOriginal> _original;

        internal StateDelegator(
            IFeatureActor<TOriginal> original, Func<TOriginal, TTarget> convert,
            Func<TOriginal, TTarget, TOriginal> convertBack)
        {
            _original = original;
            _convert = convert;
            _convertBack = convertBack;
        }

        #pragma warning disable AV1551
        public IObservable<TEvent> Receive<TEvent>()
            #pragma warning restore AV1551
            => _original.Receive<TEvent>();

        public IActorRef Self
            => _original.Self;

        public IActorRef Parent
            => _original.Parent;

        public IActorRef Sender
            => _original.Sender;

        public IUntypedActorContext Context
            => _original.Context;

        bool IFeatureActor<TTarget>.CallSingleHandler
        {
            get => _original.CallSingleHandler;
            set => _original.CallSingleHandler = value;
        }

        public SupervisorStrategy? SupervisorStrategy
        {
            get => _original.SupervisorStrategy;
            set => _original.SupervisorStrategy = value;
        }

        public void UpdateState(TTarget state)
            => _original.UpdateState(_convertBack(_original.CurrentState, state));

        public void TellSelf(object msg) => _original.TellSelf(msg);

        public ILoggingAdapter Log
            => _original.Log;

        public IDisposable Subscribe(IObserver<TTarget> observer) => _original.Select(_convert).Subscribe(observer);

        public IObservable<IActorContext> Start => _original.Start;
        public IObservable<IActorContext> Stop => _original.Stop;
        public TTarget CurrentState => _convert(_original.CurrentState);

        public IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match) => _original.WaitForSignal(timeout, match);

        public ITimerScheduler Timers
        {
            get => _original.Timers;
            set => _original.Timers = value;
        }

        public void Dispose() => _original.Dispose();

        public void AddResource(IDisposable res) => _original.AddResource(res);

        public void RemoveResource(IDisposable res) => _original.RemoveResource(res);

        #pragma warning disable AV1551
        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))));

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<TTarget>> handler)
            => _original.Receive<TEvent>(
                obs
                    => handler(obs.Select(statePair => statePair.Convert(_convert)))
                       .Select(state => _convertBack(_original.CurrentState, state)));

        public void Receive<TEvent>(
            Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler,
            Func<Exception, bool> errorHandler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))), errorHandler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IDisposable> handler)
            => _original.Receive<TEvent>(obs => handler(obs.Select(statePair => statePair.Convert(_convert))));
        #pragma warning restore AV1551
    }
}