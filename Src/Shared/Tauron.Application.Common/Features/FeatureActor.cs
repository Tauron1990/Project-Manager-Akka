using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Application.AkkNode.Services.Features
{
    [PublicAPI]
    public interface IFeatureActorBase : IObservableActor, IWithTimers
    {
        ILoggingAdapter Log { get; }
        IObservable<TEvent> WhenReceive<TEvent>();

        IActorRef Self { get; }

        IActorRef Parent { get; }

        IActorRef? Sender { get; }

        IUntypedActorContext Context { get; }

        void Become(IEnumerable<IFeature> futures);

        void BecomeStacked(IEnumerable<IFeature> futures);

        void UnbecomeStacked();
    }

    [PublicAPI]
    public interface IFeatureActor : IFeatureActorBase
    {
        void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler);
        void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler);
        void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);
        void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler);
        void WhenReceive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler);
    }

    [PublicAPI]
    public interface IFeatureActor<TState> : IFeatureActorBase, IObservable<TState>
    {
        TState CurrentState { get; }

        void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler);
        void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler);
        void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);
        void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler);
    }

    public sealed record StatePair<TEvent, TState>(TEvent Event, TState State, ITimerScheduler Timers)
    {
        public StatePair<TEvent, TNew> Convert<TNew>(Func<TState, TNew> converter)
            => new(Event, converter(State), Timers);
    }

    [PublicAPI]
    public abstract class FeatureActorBase<TFeatured, TState> : ObservableActor, IFeatureActor<TState>, IFeatureActor
        where TFeatured : FeatureActorBase<TFeatured, TState>, new()
    {
        private BehaviorSubject<TState>? _currentState;

        private BehaviorSubject<TState> CurrentState
        {
            get
            {
                if (_currentState == null)
                    throw new InvalidOperationException("The Actor was Not Initialized Propertly");

                return _currentState;
            }
        }

        protected static Props Create(Func<TState> initialState, Action<ActorBuilder<TState>> builder)
            => Props.Create(typeof(ActorFactory), builder, initialState);

        protected static Props Create(TState initialState, Func<IEnumerable<Action<ActorBuilder<TState>>>> builder)
            => Create(() => initialState, actorBuilder => builder().Foreach(f => f(actorBuilder)));

        protected static Props Create(TState initialState, Action<ActorBuilder<TState>> builder)
            => Create(() => initialState, builder);

        TState IFeatureActor<TState>.CurrentState => CurrentState.Value;

        protected FeatureActorBase()
        {
            Log = base.Log;
            Self = base.Self;
            Parent = Context.Parent;
        }

        public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))));

        public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
        {
            IDisposable CreateHandler(IObservable<TEvent> observable) 
                => handler(observable.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers)))
               .SubscribeWithStatus(CurrentState.OnNext);

            Receive<TEvent>(obs => CreateHandler(obs));
        }

        public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))), errorHandler);

        public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))));

        private void InitialState(TState initial)
            => _currentState = new BehaviorSubject<TState>(initial);

        private void RegisterFeature(IFeature feature)
            => feature.Init(this);

        public IDisposable Subscribe(IObserver<TState> observer)
            => CurrentState.Subscribe(observer);

        public void Become(IEnumerable<IFeature> futures) => BecomeStacked(() => futures.Foreach(f => f.Init(this)));

        public void BecomeStacked(IEnumerable<IFeature> futures) => BecomeStacked(() => futures.Foreach(f => f.Init(this)));
        void IFeatureActorBase.UnbecomeStacked() => UnbecomeStacked();

        private sealed class ActorFactory : IIndirectActorProducer
        {
            private readonly Action<ActorBuilder<TState>> _builder;
            private readonly Func<TState> _initialState;

            public ActorFactory(Action<ActorBuilder<TState>> builder, Func<TState> initialState)
            {
                _builder = builder;
                _initialState = initialState;
            }

            public ActorBase Produce()
            {
                var fut = new TFeatured();
                fut.InitialState(_initialState());
                _builder(new ActorBuilder<TState>(fut.RegisterFeature));
                return fut;
            }

            public void Release(ActorBase actor) { }

            public Type ActorType { get; } = typeof(TFeatured);
        }

        [PublicAPI]
        public static class Make
        {
            public static Action<ActorBuilder<TState>> Feature(Action<IFeatureActor<TState>> initializer) 
                => b => b.WithFeature(new DelegatingFeature(initializer));
        }

        [PublicAPI]
        public static class Simple
        {
            public static IFeature Feature(Action<IFeatureActor<TState>> initializer)
                => new DelegatingFeature(initializer);
        }

        private class DelegatingFeature : ActorFeatureBase<TState>
        {
            private readonly Action<IFeatureActor<TState>> _initializer;

            public DelegatingFeature(Action<IFeatureActor<TState>> initializer) => _initializer = initializer;

            protected override void Init(IFeatureActor<TState> actorBase) => _initializer(actorBase);
        }

        public ITimerScheduler Timers { get; set; } = null!;
    }

    public sealed record EmptyState;

    [PublicAPI]
    public sealed class ActorBuilder<TState>
    {
        private readonly Action<IFeature> _registrar;

        public ActorBuilder(Action<IFeature> registrar) => _registrar = registrar;

        public ActorBuilder<TState> WithFeature(IFeature feature)
        {
            _registrar(feature);
            return this;
        }

        public ActorBuilder<TState> WithFeature(IStatedFeature<TState> feature)
            => WithFeature((IFeature) feature);

        public ActorBuilder<TState> WithFeature<TNewState>(IStatedFeature<TNewState> feature, Func<TState, TNewState> convert, Func<TNewState, TState> convertBack)
            => WithFeature(new ConvertingFeature<TNewState, TState>(feature, convert, convertBack));

        private sealed class ConvertingFeature<TTarget, TOriginal> : IStatedFeature<TTarget>
        {
            private readonly IStatedFeature<TTarget> _feature;
            private readonly Func<TOriginal, TTarget> _convert;
            private readonly Func<TTarget, TOriginal> _convertBack;

            public ConvertingFeature(IStatedFeature<TTarget> feature, Func<TOriginal, TTarget> convert, Func<TTarget, TOriginal> convertBack)
            {
                _feature = feature;
                _convert = convert;
                _convertBack = convertBack;
            }

            public void Init(IFeatureActorBase actorBase) 
                => _feature.Init(new StateDelegator<TTarget,TOriginal>((IFeatureActor<TOriginal>)actorBase, _convert, _convertBack));
        }

        private sealed class StateDelegator<TTarget, TOriginal> : IFeatureActor<TTarget>
        {
            private readonly IFeatureActor<TOriginal> _original;
            private readonly Func<TOriginal, TTarget> _convert;
            private readonly Func<TTarget, TOriginal> _convertBack;

            public StateDelegator(IFeatureActor<TOriginal> original, Func<TOriginal, TTarget> convert, Func<TTarget, TOriginal> convertBack)
            {
                _original = original;
                _convert = convert;
                _convertBack = convertBack;
            }
            
            public IObservable<TEvent> Receive<TEvent>() 
                => _original.WhenReceive<TEvent>();

            public IActorRef Self
                => _original.Self;

            public IActorRef Parent
                => _original.Parent;

            public IActorRef? Sender
                => _original.Sender;

            public IUntypedActorContext Context
                => _original.Context;

            public void Become(IEnumerable<IFeature> futures) => _original.Become(futures);

            public void BecomeStacked(IEnumerable<IFeature> futures) => _original.BecomeStacked(futures);

            public void UnbecomeStacked() => _original.UnbecomeStacked();

            public ILoggingAdapter Log
                => _original.Log;

            public IDisposable Subscribe(IObserver<TTarget> observer) => _original.Select(_convert).Subscribe(observer);

            public TTarget CurrentState => _convert(_original.CurrentState);

            public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler)
                => _original.WhenReceive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))));

            public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<TTarget>> handler)
                => _original.WhenReceive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))).Select(_convertBack));

            public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
                => _original.WhenReceive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))), errorHandler);

            public void WhenReceive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IDisposable> handler)
                => _original.WhenReceive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))));


            public IActorDsl Exposed
                => _original.Exposed;

            public void AddResource(IDisposable res) 
                => _original.AddResource(res);

            public void RemoveResources(IDisposable res) 
                => _original.RemoveResources(res);

            public ITimerScheduler Timers
            {
                get => _original.Timers;
                set => _original.Timers = value;
            }
        }
    }

}