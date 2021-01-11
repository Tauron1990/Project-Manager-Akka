using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public interface IObservableActor : IDisposable
    {
        IObservable<IActorContext> Start { get; }

        IObservable<IActorContext> Stop { get; }

        ILoggingAdapter Log { get; }
        IActorRef Self { get; }
        IActorRef Parent { get; }
        IActorRef? Sender { get; }
        void AddResource(IDisposable res);
        void RemoveResources(IDisposable res);
        void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler);
        IObservable<TEvent> Receive<TEvent>();
        void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler);
        void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler);
        void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler);
        void Receive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler);
    }

    [PublicAPI]
    public class ObservableActor : ActorBase, IObservableActor
    {
        public static IActorContext ExposedContext => ActorBase.Context;
        
        private readonly Dictionary<Type, object> _selectors = new();
        private readonly CompositeDisposable _resources = new();
        private readonly Subject<object> _receiver = new();
        private readonly BehaviorSubject<IActorContext?> _start = new(null);
        private readonly BehaviorSubject<IActorContext?> _stop = new(null);

        private bool _isReceived;

        public IObservable<IActorContext> Start => _start.NotNull();
        public IObservable<IActorContext> Stop => _stop.NotNull();
        public ILoggingAdapter Log { get; } = ActorBase.Context.GetLogger();
        
        public new IActorRef Self { get; }
        public IActorRef Parent { get; }
        public new IActorRef Sender => Context.Sender;
        public new static IUntypedActorContext Context => (IUntypedActorContext)ActorBase.Context;

        public ObservableActor()
        {
            Self = base.Self;
            Parent = ActorBase.Context.Parent;

            _resources.Add(_receiver);
            _resources.Add(_start);
            _resources.Add(_stop);
        }

        public virtual void Dispose()
        {
            _resources.Dispose();
            GC.SuppressFinalize(this);
        }
        
        public void AddResource(IDisposable res) => _resources.Add(res);

        public void RemoveResources(IDisposable res) => _resources.Remove(res);

        protected override bool AroundReceive(Receive receive, object message)
        {
            switch (message)
            {
                case TransmitAction act:
                    return act.Runner();
                case Status.Failure failure:
                    if (OnError(failure))
                        throw failure.Cause;
                    else
                        return true;
                default:
                    return base.AroundReceive(receive, message);
            }
        }

        public override void AroundPreStart()
        {
            _start.OnNext(Context);
            base.AroundPreStart();
        }

        public override void AroundPostStop()
        {
            _stop.OnNext(Context);
            _start.OnCompleted();
            _receiver.OnCompleted();
            base.AroundPostStop();
            _stop.OnCompleted();
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

        protected override bool Receive(object message)
        {
            _isReceived = false;

            _receiver.OnNext(message);

            return _isReceived;
        }

        public void TellSelf(object msg) => _receiver.OnNext(msg);

        protected virtual bool OnError(Status.Failure failure) => ThrowError(failure.Cause);

        protected IObservable<TEvent> GetSelector<TEvent>()
        {
            if (!_selectors.TryGetValue(typeof(TEvent), out var selector))
            {
                selector = _receiver
                          .Where(m => m is TEvent)
                          .Select(m =>
                                  {
                                      _isReceived = true;
                                      return (TEvent) m;
                                  })
                          .Isonlate();

                _selectors[typeof(TEvent)] = selector;
            }

            return (IObservable<TEvent>) selector;
        }

        public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler) 
            => AddResource(new ObservableInvoker<TEvent, Unit>(handler, ThrowError, GetSelector<TEvent>()).Construct());

        public IObservable<TEvent> Receive<TEvent>() => GetSelector<TEvent>();

        public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, ThrowError, GetSelector<TEvent>()).Construct());

        public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler) 
            => AddResource(new ObservableInvoker<TEvent, Unit>(handler, errorHandler, GetSelector<TEvent>()).Construct());

        public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, errorHandler, GetSelector<TEvent>()).Construct());


        public void Receive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler) 
            => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, GetSelector<TEvent>(), true).Construct());

        public bool ThrowError(Exception e)
        {
            Log.Error(e, "Error on Process Event");
            Self.Tell(new Status.Failure(e));
            return true;
        }

        public bool DefaultError(Exception e)
        {
            Log.Error(e, "Error on Process Event");
            return false;
        }

        private sealed class ObservableInvoker<TEvent, TResult> : IDisposable
        {
            private readonly IObservable<TEvent> _selector;

            private readonly Func<IObservable<TEvent>, IDisposable> _factory;
            private IDisposable? _subscription;

            public ObservableInvoker(Func<IObservable<TEvent>, IObservable<TResult>> factory, Func<Exception, bool> errorHandler, IObservable<TEvent> selector)
            {
                _factory = o => factory(o.AsObservable()).Subscribe(_ => { }, e =>
                                                                              {
                                                                                  if (errorHandler(e)) 
                                                                                      Init();
                                                                              });
                _selector = selector;

                Init();
            }

            public ObservableInvoker(Func<IObservable<TEvent>, IDisposable> factory, IObservable<TEvent> selector, bool isSafe)
            {
                _factory = isSafe ? observable => factory(observable.Do(_ => { }, _ => Init())) : factory;
                _selector = selector;

                Init();
            }

            void IDisposable.Dispose() => _subscription?.Dispose();

            public IDisposable Construct() => this;

            private void Init() => _subscription = _factory(_selector);
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
    }
}