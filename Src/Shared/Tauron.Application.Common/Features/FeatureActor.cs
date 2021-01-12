using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Features
{
    [PublicAPI]
    public interface IFeatureActor<TState> : IObservable<TState>
    {
        TState CurrentState { get; }

        void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler);
        void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler);
        void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);
        void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler);

        void TellSelf(object msg);

        ILoggingAdapter Log { get; }
        IObservable<TEvent> Receive<TEvent>();

        IActorRef Self { get; }

        IActorRef Parent { get; }

        IActorRef Sender { get; }

        IUntypedActorContext Context { get; }

        SupervisorStrategy? SupervisorStrategy { get; set; }
    }

    public sealed record StatePair<TEvent, TState>(TEvent Event, TState State, ITimerScheduler Timers)
    {
        public StatePair<TEvent, TNew> Convert<TNew>(Func<TState, TNew> converter)
            => new(Event, converter(State), Timers);
    }

    [PublicAPI]
    public abstract class FeatureActorBase<TFeatured, TState> : ObservableActor, IFeatureActor<TState>
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
        
        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))));

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
        {
            IDisposable CreateHandler(IObservable<TEvent> observable) 
                => handler(observable.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers)))
               .SubscribeWithStatus(CurrentState.OnNext);

            Receive<TEvent>(obs => CreateHandler(obs));
        }

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))), errorHandler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
            => Receive<TEvent>(obs => handler(obs.Select(evt => new StatePair<TEvent, TState>(evt, CurrentState.Value, Timers))));

        IUntypedActorContext IFeatureActor<TState>.Context => Context;
        SupervisorStrategy? IFeatureActor<TState>.SupervisorStrategy { get; set; }

        private void InitialState(TState initial)
            => _currentState = new BehaviorSubject<TState>(initial);

        private readonly HashSet<string> _featureIds = new();
        private void RegisterFeature(IFeature<TState> feature)
        {
            if (feature.Identify().Any(id => !_featureIds.Add(id)))
                throw new InvalidOperationException("Duplicate Feature Added");

            feature.Init(this);
        }

        public IDisposable Subscribe(IObserver<TState> observer)
            => CurrentState.Subscribe(observer);

        protected override SupervisorStrategy SupervisorStrategy() => ((IFeatureActor<TState>)this).SupervisorStrategy ?? base.SupervisorStrategy();

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
            public static Action<ActorBuilder<TState>> Feature(Action<IFeatureActor<TState>> initializer, params string[] ids) 
                => b => b.WithFeature(new DelegatingFeature(initializer, ids));
        }

        [PublicAPI]
        public static class Simple
        {
            public static IFeature<TState> Feature(Action<IFeatureActor<TState>> initializer, params string[] ids)
                => new DelegatingFeature(initializer, ids);
        }

        private class DelegatingFeature : IFeature<TState>
        {
            private readonly Action<IFeatureActor<TState>> _initializer;
            private readonly IEnumerable<string> _ids;

            public DelegatingFeature(Action<IFeatureActor<TState>> initializer, IEnumerable<string> ids)
            {
                _initializer = initializer;
                _ids = ids;
            }

            public IEnumerable<string> Identify() => _ids;

            public void Init(IFeatureActor<TState> actor) => _initializer(actor);
        }

        public ITimerScheduler Timers { get; set; } = null!;
    }

    public sealed record EmptyState
    {
        public static readonly EmptyState Inst = new();
    }

    [PublicAPI]
    public sealed class ActorBuilder<TState>
    {
        private readonly Action<IFeature<TState>> _registrar;

        public ActorBuilder(Action<IFeature<TState>> registrar) => _registrar = registrar;

        public ActorBuilder<TState> WithFeature(IFeature<TState> feature)
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

        public ActorBuilder<TState> WithFeature<TNewState>(IFeature<TNewState> feature, Func<TState, TNewState> convert, Func<TState, TNewState, TState> convertBack)
            => WithFeature(new ConvertingFeature<TNewState, TState>(feature, convert, convertBack));

        internal class ConvertingFeature<TTarget, TOriginal> : IFeature<TOriginal>
        {
            private readonly IFeature<TTarget> _feature;
            private readonly Func<TOriginal, TTarget> _convert;
            private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;

            public ConvertingFeature(IFeature<TTarget> feature, Func<TOriginal, TTarget> convert, Func<TOriginal, TTarget, TOriginal> convertBack)
            {
                _feature = feature;
                _convert = convert;
                _convertBack = convertBack;
            }

            public IEnumerable<string> Identify() => _feature.Identify();

            public virtual void Init(IFeatureActor<TOriginal> actor) 
                => _feature.Init(new StateDelegator<TTarget,TOriginal>(actor, _convert, _convertBack));
        }

        private sealed class StateDelegator<TTarget, TOriginal> : IFeatureActor<TTarget>
        {
            private readonly IFeatureActor<TOriginal> _original;
            private readonly Func<TOriginal, TTarget> _convert;
            private readonly Func<TOriginal, TTarget, TOriginal> _convertBack;

            public StateDelegator(IFeatureActor<TOriginal> original, Func<TOriginal, TTarget> convert, Func<TOriginal, TTarget, TOriginal> convertBack)
            {
                _original = original;
                _convert = convert;
                _convertBack = convertBack;
            }
            
            public IObservable<TEvent> Receive<TEvent>() 
                => _original.Receive<TEvent>();

            public IActorRef Self
                => _original.Self;

            public IActorRef Parent
                => _original.Parent;

            public IActorRef Sender
                => _original.Sender;

            public IUntypedActorContext Context
                => _original.Context;

            public SupervisorStrategy? SupervisorStrategy
            {
                get => _original.SupervisorStrategy;
                set => _original.SupervisorStrategy = value;
            }

            public void TellSelf(object msg) => _original.TellSelf(msg);

            public ILoggingAdapter Log
                => _original.Log;

            public IDisposable Subscribe(IObserver<TTarget> observer) => _original.Select(_convert).Subscribe(observer);

            public TTarget CurrentState => _convert(_original.CurrentState);

            public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler)
                => _original.Receive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))));

            public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<TTarget>> handler)
                => _original.Receive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))).Select(s => _convertBack(_original.CurrentState, s)));

            public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
                => _original.Receive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))), errorHandler);

            public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TTarget>>, IDisposable> handler)
                => _original.Receive<TEvent>(obs => handler(obs.Select(d => d.Convert(_convert))));

        }
    }

}