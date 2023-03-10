using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Actor.Internal;
using JetBrains.Annotations;
using Tauron.TAkka;

namespace Tauron.Features;

[PublicAPI]
[DebuggerStepThrough]
public abstract class FeatureActorBase<TFeatured, TState> : ObservableActor, IFeatureActor<TState>
    where TFeatured : FeatureActorBase<TFeatured, TState>, new()
{
    private readonly HashSet<string> _featureIds = new(StringComparer.Ordinal);
    private readonly List<IFeature> _features = new();
    private BehaviorSubject<TState>? _currentState;
    private IActorRef _parent = ActorRefs.Nobody;

    private IActorRef _self = ActorRefs.Nobody;

    private BehaviorSubject<TState> CurrentState
    {
        get
        {
            if(_currentState is null)
                throw new InvalidOperationException("The Actor was Not Initialized Propertly");

            return _currentState;
        }
    }

    IActorRef IFeatureActor<TState>.Self => _self;

    IActorRef IFeatureActor<TState>.Parent => _parent;

    public ITimerScheduler Timers { get; set; } = null!;

    TState IFeatureActor<TState>.CurrentState => CurrentState.Value;

    public void Observ<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
        => Receive<TEvent>(
            obs
                => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))));

    public void Observ<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
    {
        IDisposable CreateHandler(IObservable<TEvent> observable)
            => handler(observable.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self)))
               .SubscribeWithStatus(UpdateState);

        Receive<TEvent>(obs => CreateHandler(obs));
    }

    public void Observ<TEvent>(
        Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler,
        Func<Exception, bool> errorHandler)
        => Receive<TEvent>(
            obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))),
            errorHandler);

    public void Observ<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
        => Receive<TEvent>(
            obs
                => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers, Context, Sender, Parent, Self))));

    public void UpdateState(TState state)
    {
        if(InternalCurrentActorCellKeeper.Current is null)
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


        System.Reactive.Disposables.Disposable.Create(
            _currentState,
                currentState =>
            {
                if(currentState.Value is IDisposable disposable)
                    disposable.Dispose();
            })
            .DisposeWith(this);
        _currentState.DisposeWith(this);
    }

    private void RegisterFeature(IFeature<TState> feature)
    {
        if(feature.Identify().Any(id => !_featureIds.Add(id)))
            throw new InvalidOperationException("Duplicate Feature Added");

        feature.Init(this);
        _features.Add(feature);
    }

    protected override SupervisorStrategy SupervisorStrategy()
        => ((IFeatureActor<TState>)this).SupervisorStrategy ?? base.SupervisorStrategy();

    protected override void PreStart()
    {
        foreach (IFeature feature in _features)
            feature.PreStart();

        base.PreStart();
    }

    protected override void PostStop()
    {
        foreach (IFeature feature in _features)
            feature.PostStop();
        
        base.PostStop();
    }

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

    [DebuggerStepThrough]
    private class DelegatingFeature : IFeature<TState>
    {
        private readonly IEnumerable<string> _ids;
        private readonly Func<IFeatureActor<TState>, IFeature<TState>> _initializer;
        private IFeatureActor<TState>? _actor;
        private IFeature<TState>? _feature;

        internal DelegatingFeature(Func<IFeatureActor<TState>, IFeature<TState>> initializer, IEnumerable<string> ids)
        {
            _initializer = initializer;
            _ids = ids;
        }

        public IEnumerable<string> Identify() => _ids;

        public void Init(IFeatureActor<TState> actor)
        {
            _actor = actor;
            _feature = _initializer(actor);
        }

        public void PostStop()
            => _feature?.PostStop();

        public void PreStart()
            => _feature?.PreStart();

        void IDisposable.Dispose() => _actor?.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _actor?.AddResource(res);

        void IResourceHolder.RemoveResource(IDisposable res) => _actor?.RemoveResource(res);
    }

    [PublicAPI]
    [DebuggerStepThrough]
    #pragma warning disable MA0018
    public static class Make
    {
        public static Action<ActorBuilder<TState>> Feature(Func<IFeatureActor<TState>, IFeature<TState>> initializer, params string[] ids)
            => actorBuilder => actorBuilder.WithFeature(new DelegatingFeature(initializer, ids));
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public static class Simple
    {
        public static IFeature<TState> Feature(Func<IFeatureActor<TState>, IFeature<TState>> initializer, params string[] ids)
            => new DelegatingFeature(initializer, ids);
    }
    #pragma warning restore MA0018
}