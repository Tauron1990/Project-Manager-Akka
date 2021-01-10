﻿using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IExpandedReceiveActor
    {
        IActorDsl Exposed { get; }

        void AddResource(IDisposable res);

        void RemoveResources(IDisposable res);
    }

    [PublicAPI]
    public class ExpandedReceiveActor : ReceiveActor, IActorDsl, IExpandedReceiveActor, IDisposable
    {
        private readonly Dictionary<Type, ObsSenderBase> _genericSource = new();
        private readonly CompositeDisposable _resources = new();

        private Action<Exception, IActorContext>? _onPostRestart;
        private Action<IActorContext>? _onPostStop;
        private Action<Exception, object, IActorContext>? _onPreRestart;
        private Action<IActorContext>? _onPreStart;
        private SupervisorStrategy? _strategy;

        public static IUntypedActorContext ExposedContext => Context;

        protected internal ILoggingAdapter Log { get; } = Context.GetLogger();

        public virtual void Dispose()
        {
            _resources.Dispose();
            GC.SuppressFinalize(this);
        }

        public IActorDsl Exposed => this;

        public void AddResource(IDisposable res) => _resources.Add(res);

        public void RemoveResources(IDisposable res) => _resources.Remove(res);

        protected override bool AroundReceive(Receive receive, object message)
        {
            switch (message)
            {
                case TransmitAction act:
                    return act.Runner();
                default:
                    if (_genericSource.TryGetValue(message.GetType(), out var sender))
                    {
                        sender.Run(message);
                        return true;
                    }

                    return base.AroundReceive(receive, message);
            }
        }

        protected override void Unhandled(object message)
        {
            if (message is Status status)
            {
                if(status is Status.Failure failure)
                    Log.Error(failure.Cause, "Unhandled Exception Received");
            }
            else
                base.Unhandled(message);
        }

        //public void WhenReceiveSafe<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler) 
        //    => AddResource(new ObservableInvoker<TEvent, Unit>(handler, DefaultError, this).Construct());

        //public void WhenReceiveSafe<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler) 
        //    => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, DefaultError, this).Construct());

        public void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler) 
            => AddResource(new ObservableInvoker<TEvent, Unit>(handler, DefaultError, this).Construct());

        public IObservable<TEvent> WhenReceive<TEvent>()
        {
            ObsSender<TEvent> typedsender;

            if (_genericSource.TryGetValue(typeof(TEvent), out var sender))
                typedsender = (ObsSender<TEvent>) sender;
            else
            {
                typedsender = new ObsSender<TEvent>();
                typedsender.Sender.DisposeWith(this);
                _genericSource[typeof(TEvent)] = typedsender;
            }

            return typedsender.Sender.AsObservable();
        }

        public void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, DefaultError, this).Construct());

        public void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler) 
            => AddResource(new ObservableInvoker<TEvent, Unit>(handler, errorHandler, this).Construct());

        public void WhenReceive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, errorHandler, this).Construct());

        //protected void WhenReceiveSafe<TEvent>(Func<IObservable<TEvent>, IDisposable> handler) 
        //    => AddResource(new ObservableInvoker<TEvent, Unit>(handler, this, true).Construct());

        public void WhenReceive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, this, true).Construct());

        public bool ThrowError(Exception e)
        {
            Log.Error(e, "Error on Process Event");
            throw e;
        }

        public bool DefaultError(Exception e)
        {
            Log.Error(e, "Error on Process Event");
            return true;
        }

        private sealed class ObservableInvoker<TEvent, TResult> : IDisposable
        {
            private readonly IActorDsl _dsl;

            private readonly Func<IObservable<TEvent>, IDisposable> _factory;
            private bool _needInit;
            private Subject<TEvent>? _source;
            private IDisposable? _subscription;

            public ObservableInvoker(Func<IObservable<TEvent>, IObservable<TResult>> factory, Func<Exception, bool> errorHandler, IActorDsl dsl)
            {
                _factory = o => factory(o.AsObservable()).Subscribe(_ => { }, e => _needInit = errorHandler(e));
                _dsl = dsl;

                Init();
            }

            public ObservableInvoker(Func<IObservable<TEvent>, IDisposable> factory, IActorDsl dsl, bool isSafe)
            {
                _factory = isSafe ? observable => factory(observable.Do(_ => { }, _ => _needInit = true)) : factory;
                _dsl = dsl;

                Init();
            }

            void IDisposable.Dispose()
            {
                _source?.Dispose();
                _subscription?.Dispose();
            }

            public IDisposable Construct()
            {
                _dsl.Receive<TEvent>(Runner);
                return this;
            }

            private void Runner(TEvent @event, IActorContext actorContext)
            {
                if (_needInit)
                    Init();
                _source?.OnNext(@event);
            }

            private void Init()
            {
                _subscription?.Dispose();
                _source?.Dispose();

                _source = new Subject<TEvent>();
                _subscription = _factory(_source.AsObservable());
            }
        }

        private abstract class ObsSenderBase
        {
            public abstract void Run(object msg);
        }

        private sealed class ObsSender<TType> : ObsSenderBase
        {
            public ObsSender() => Sender = new Subject<TType>();

            public Subject<TType> Sender { get; }

            public override void Run(object msg)
            {
                Sender.OnNext((TType) msg);
            }
        }

        public record TransmitAction(Func<bool> Runner)
        {
            public TransmitAction(Action action)
                : this(() =>
                       {
                           action();
                           return true;
                       }) { }
        }

        #region ActorDsl

        void IActorDsl.Receive<T>(Action<T, IActorContext> handler)
        {
            Receive<T>(m => handler(m, Context));
        }

        void IActorDsl.Receive<T>(Predicate<T> shouldHandle, Action<T, IActorContext> handler)
        {
            Receive(shouldHandle, obj => handler(obj, Context));
        }

        void IActorDsl.Receive<T>(Action<T, IActorContext> handler, Predicate<T> shouldHandle)
        {
            Receive(t => handler(t, Context), shouldHandle);
        }

        void IActorDsl.ReceiveAny(Action<object, IActorContext> handler)
        {
            ReceiveAny(m => handler(m, Context));
        }

        void IActorDsl.ReceiveAsync<T>(Func<T, IActorContext, Task> handler, Predicate<T> shouldHandle)
        {
            ReceiveAsync(m => handler(m, Context), shouldHandle);
        }

        void IActorDsl.ReceiveAsync<T>(Predicate<T> shouldHandle, Func<T, IActorContext, Task> handler)
        {
            ReceiveAsync(shouldHandle, arg => handler(arg, Context));
        }

        void IActorDsl.ReceiveAnyAsync(Func<object, IActorContext, Task> handler)
        {
            ReceiveAnyAsync(m => handler(m, Context));
        }

        void IActorDsl.DefaultPreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
        }

        void IActorDsl.DefaultPostRestart(Exception reason)
        {
            PostRestart(reason);
        }

        void IActorDsl.DefaultPreStart()
        {
            base.PreStart();
        }

        void IActorDsl.DefaultPostStop()
        {
            base.PostStop();
        }

        void IActorDsl.Become(Action<object, IActorContext> handler)
        {
            Become(o => handler(o, Context));
        }

        void IActorDsl.BecomeStacked(Action<object, IActorContext> handler)
        {
            BecomeStacked(o => handler(o, Context));
        }

        void IActorDsl.UnbecomeStacked()
        {
            UnbecomeStacked();
        }

        IActorRef IActorDsl.ActorOf(Action<IActorDsl> config, string name) => Context.ActorOf(config, name);

        protected event Action<Exception>? OnPostRestart;

        protected event Action<Exception, object>? OnPreRestart;

        protected event Action? OnPostStop;

        protected event Action? OnPreStart;

        Action<Exception, IActorContext>? IActorDsl.OnPostRestart
        {
            get => _onPostRestart;
            set => _onPostRestart = (Action<Exception, IActorContext>?) Delegate.Combine(_onPostRestart, value);
        }

        Action<Exception, object, IActorContext>? IActorDsl.OnPreRestart
        {
            get => _onPreRestart;
            set => _onPreRestart = (Action<Exception, object, IActorContext>?) Delegate.Combine(_onPostRestart, value);
        }

        Action<IActorContext>? IActorDsl.OnPostStop
        {
            get => _onPostStop;
            set => _onPostStop = (Action<IActorContext>?) Delegate.Combine(_onPostStop, value);
        }

        Action<IActorContext>? IActorDsl.OnPreStart
        {
            get => _onPreStart;
            set => _onPreStart = (Action<IActorContext>?) Delegate.Combine(_onPreStart, value);
        }

        SupervisorStrategy? IActorDsl.Strategy
        {
            get => _strategy;
            set => _strategy = value;
        }

        protected override void PostRestart(Exception reason)
        {
            _onPostRestart?.Invoke(reason, Context);
            OnPostRestart?.Invoke(reason);
            base.PostRestart(reason);
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _onPreRestart?.Invoke(reason, message, Context);
            OnPreRestart?.Invoke(reason, message);
            base.PreRestart(reason, message);
        }

        protected override void PostStop()
        {
            _onPostStop?.Invoke(Context);
            OnPostStop?.Invoke();

            base.PostStop();
        }

        protected override void PreStart()
        {
            _onPreStart?.Invoke(Context);
            OnPreStart?.Invoke();
            base.PreStart();
        }

        protected override SupervisorStrategy SupervisorStrategy() => _strategy ?? base.SupervisorStrategy();

        #endregion
    }
}