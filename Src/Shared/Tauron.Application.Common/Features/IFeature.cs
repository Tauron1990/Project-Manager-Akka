using System;
using System.Collections.Generic;
using System.Reactive;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;
using Tauron.Akka;

namespace Tauron.Features
{
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
        public bool CallSingleHandler
        {
            get => _actor.CallSingleHandler;
            set => _actor.CallSingleHandler = value;
        }

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

        public IObservable<IActorContext> Start => _actor.Start;

        public IObservable<IActorContext> Stop => _actor.Stop;

        public TState CurrentState => _actor.CurrentState;

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler)
            => _actor.Receive(handler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<TState>> handler)
            => _actor.Receive(handler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IObservable<Unit>> handler,
            Func<Exception, bool> errorHandler) => _actor.Receive(handler, errorHandler);

        public void Receive<TEvent>(Func<IObservable<StatePair<TEvent, TState>>, IDisposable> handler)
            => _actor.Receive(handler);

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

        protected virtual void Config() => ConfigImpl();

        protected abstract void ConfigImpl();

        public ITimerScheduler Timers { get; set; } = null!;

        void IDisposable.Dispose() => _actor.Dispose();

        void IResourceHolder.AddResource(IDisposable res) => _actor.AddResource(res);

        void IResourceHolder.RemoveResources(IDisposable res) => _actor.RemoveResources(res);
    }
}