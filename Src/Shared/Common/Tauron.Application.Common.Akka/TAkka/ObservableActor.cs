using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Application;

namespace Tauron.TAkka;

[PublicAPI]
public class ObservableActor : ActorBase, IObservableActor
{
    private readonly record struct StackFrame(Subject<object> Receiver, CompositeDisposable Subscriptions, Dictionary<Type, object> Selectors) : IDisposable
    {
        public void Dispose()
        {
            Subscriptions.Dispose();
            Receiver.Dispose();
        }
    }
    
    private sealed class FrameStack : IDisposable
    {
        private readonly Stack<StackFrame> _frames = new();
        private bool _isReceived;
        
        internal bool CallSingleHandler { get; set; }

        internal FrameStack() => _frames.Push(new StackFrame(new Subject<object>(), new CompositeDisposable(), new Dictionary<Type, object>()));

        internal void AddSubscription(IDisposable subscription) => _frames.Peek().Subscriptions.Add(subscription);

        internal bool Dispatch(object msg)
        {
            _isReceived = false;
            
            _frames.Peek().Receiver.OnNext(msg);
            
            return _isReceived;
        }
        
        internal void DispatchSelf(object msg) => _frames.Peek().Receiver.OnNext(msg);

        internal IObservable<TEvent> GetSelector<TEvent>()
        {
            var (receiver, _, selectors) = _frames.Peek();
            
            if(selectors.TryGetValue(typeof(TEvent), out object? selector)) return (IObservable<TEvent>)selector;

            selector = receiver
                .Where(msg => msg is TEvent && (!CallSingleHandler || !_isReceived))
                .Select(
                    msg =>
                    {
                        _isReceived = true;

                        return (TEvent)msg;
                    })
                .Isonlate();

            selectors[typeof(TEvent)] = selector;

            return (IObservable<TEvent>)selector;
        }

        internal bool HasSelector<TEvent>()
            => _frames.Peek().Selectors.ContainsKey(typeof(TEvent));
        
        internal void ReplaceFrame()
        {
            SendCompled();
            _frames.Pop().Dispose();
            _frames.Push(new StackFrame(new Subject<object>(), new CompositeDisposable(), new Dictionary<Type, object>()));
        }

        internal void PushNewFrame()
            => _frames.Push(new StackFrame(new Subject<object>(), new CompositeDisposable(), new Dictionary<Type, object>()));

        internal void PopFrame()
        {
            if(_frames.Count == 1)
                ReplaceFrame();
            else
            {
                SendCompled();
                _frames.Pop();
            }
        }

        internal void SendCompled() => _frames.Peek().Receiver.OnCompleted();

        public void Dispose()
        {
            foreach (StackFrame frame in _frames)
            {
                frame.Dispose();
            }
            
            _frames.Clear();
        }
    }

    private readonly List<ISignal> _currentWaiting = new();
    private readonly CompositeDisposable _resources = new();
    private readonly FrameStack _frameStack = new();
    
    private readonly BehaviorSubject<IActorContext?> _start = new(value: null);
    private readonly BehaviorSubject<IActorContext?> _stop = new(value: null);
    

    public ObservableActor()
    {
        Self = base.Self;
        Parent = ActorBase.Context.Parent;

        _resources.Add(_frameStack);
        _resources.Add(_start);
        _resources.Add(_stop);

        Log = TauronEnviroment.GetLogger(GetType());
    }

    public bool CallSingleHandler
    {
        get => _frameStack.CallSingleHandler;
        set => _frameStack.CallSingleHandler = value;
    }

    public static IActorContext ExposedContext => ActorBase.Context;

    public IObservable<IActorContext> Start => _start.NotNull();
    public IObservable<IActorContext> Stop => _stop.NotNull();
    public ILogger Log { get; }

    public virtual void Dispose()
    {
        _resources.Dispose();
        GC.SuppressFinalize(this);
    }

    public void AddResource(IDisposable res) => _resources.Add(res);

    public void RemoveResource(IDisposable res) => _resources.Remove(res);

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler)
        => _frameStack.AddSubscription(new ObservableInvoker<TEvent, Unit>(handler, ThrowError, _frameStack.GetSelector<TEvent>()).Construct());

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler)
        => _frameStack.AddSubscription(new ObservableInvoker<TEvent, TEvent>(handler, ThrowError, _frameStack.GetSelector<TEvent>()).Construct());

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
        => _frameStack.AddSubscription(new ObservableInvoker<TEvent, Unit>(handler, errorHandler, _frameStack.GetSelector<TEvent>()).Construct());

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler)
        => _frameStack.AddSubscription(new ObservableInvoker<TEvent, TEvent>(handler, errorHandler, _frameStack.GetSelector<TEvent>()).Construct());


    public void Receive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler)
        => _frameStack.AddSubscription(new ObservableInvoker<TEvent, TEvent>(handler, _frameStack.GetSelector<TEvent>(), isSafe: true).Construct());

    public void Become(Action configure)
    {
        _frameStack.ReplaceFrame();
        configure();
    }

    public void BecomeStacked(Action configure)
    {
        _frameStack.PushNewFrame();
        configure();
    }

    public void UnbecomeStacked() => _frameStack.PopFrame();

    protected IObservable<TType> SyncActor<TType>(TType element)
        => Observable.Return(element, ActorScheduler.From(Self));

    protected IObservable<TType> SyncActor<TType>(IObservable<TType> toSync)
        => toSync.ObserveOn(ActorScheduler.From(Self));

    protected override bool AroundReceive(Receive receive, object message)
    {
        bool RunDefault()
        {
            var signaled = false;
            foreach (ISignal signal in _currentWaiting.Where(signal => signal.Match(message)))
            {
                signal.Signal(message);
                signaled = true;
            }

            return signaled || base.AroundReceive(receive, message);
        }

        switch (message)
        {
            case TransmitAction act:
                return act.Runner();
            case Status.Failure when _frameStack.HasSelector<Status.Failure>() || _currentWaiting.Any(signal => signal.Match(message)):
                return RunDefault();
            case Status.Failure failure:
                if(OnError(failure))
                    throw failure.Cause;

                return true;
            case Status.Success when _frameStack.HasSelector<Status.Success>() || _currentWaiting.Any(signal => signal.Match(message)):
                return RunDefault();
            case AddSignal add:
                _currentWaiting.Add(add.Signal);

                return true;
            case SignalTimeOut timeOut:
                timeOut.Signal.Cancel();
                _currentWaiting.Remove(timeOut.Signal);

                return true;
            default:
                return RunDefault();
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
        _frameStack.SendCompled();
        base.AroundPostStop();
        _stop.OnCompleted();
    }

    protected override void Unhandled(object message)
    {
        if(message is Status status)
        {
            if(status is Status.Failure failure)
                ObservableActorLogger.UnhandledException(Log, failure.Cause);
        }
        else
        {
            base.Unhandled(message);
        }
    }

    protected override bool Receive(object message) 
        => _frameStack.Dispatch(message);

    public void TellSelf(object msg) => _frameStack.DispatchSelf(msg);

    protected virtual bool OnError(Status.Failure failure) => ThrowError(failure.Cause);

    public IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match)
    {
        var source = new TaskCompletionSource<TSignal>(TaskCreationOptions.RunContinuationsAsynchronously);
        var signal = new ConcrretSignal<TSignal>(source, match);
        Self.Tell(new AddSignal(signal));
        Task.Delay(timeout).PipeTo(Self, success: () => new SignalTimeOut(signal));

        return source.Task.ToObservable();
    }

    public bool ThrowError(Exception exception)
    {
        ObservableActorLogger.EventProcessError(Log, exception);
        Self.Tell(new Status.Failure(exception));

        return true;
    }

    public bool DefaultError(Exception exception)
    {
        ObservableActorLogger.EventProcessError(Log, exception);

        return false;
    }

    private interface ISignal
    {
        bool Match(object obj);

        void Signal(object obj);

        void Cancel();
    }

    private sealed class ConcrretSignal<TType> : ISignal
    {
        private readonly Predicate<TType> _predicate;
        private readonly TaskCompletionSource<TType> _source;

        internal ConcrretSignal(TaskCompletionSource<TType> source, Predicate<TType> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public bool Match(object obj) => !_source.Task.IsCompleted && obj is TType signal && _predicate(signal);

        public void Signal(object obj) => _source.TrySetResult((TType)obj);

        public void Cancel() => _source.TrySetException(new TimeoutException());
    }

    //private sealed class RemoveSignal
    //{
    //    public ISignal Signal { get; }

    //    public RemoveSignal(ISignal signal) => Signal = signal;
    //}

    private sealed class AddSignal
    {
        internal AddSignal(ISignal signal) => Signal = signal;

        internal ISignal Signal { get; }
    }

    private sealed class SignalTimeOut
    {
        internal SignalTimeOut(ISignal signal) => Signal = signal;
        internal ISignal Signal { get; }
    }

    private sealed class ObservableInvoker<TEvent, TResult> : IDisposable
    {
        private readonly Func<IObservable<TEvent>, IDisposable> _factory;
        private readonly IObservable<TEvent> _selector;
        private IDisposable? _subscription;

        internal ObservableInvoker(Func<IObservable<TEvent>, IObservable<TResult>> factory, Func<Exception, bool> errorHandler, IObservable<TEvent> selector)
        {
            _factory = observable => factory(observable.AsObservable()).Subscribe(
                           _ => { },
                           exception =>
                           {
                               if(errorHandler(exception))
                                   Init();
                           });
            _selector = selector;

            Init();
        }

        internal ObservableInvoker(Func<IObservable<TEvent>, IDisposable> factory, IObservable<TEvent> selector, bool isSafe)
        {
            _factory = isSafe ? observable => factory(observable.Do(_ => { }, _ => Init())) : factory;
            _selector = selector;

            Init();
        }

        void IDisposable.Dispose() => _subscription?.Dispose();

        internal IDisposable Construct() => this;

        private void Init() => _subscription = _factory(_selector);
    }

    public record TransmitAction(Func<bool> Runner)
    {
        public TransmitAction(Action action)
            : this(
                () =>
                {
                    action();

                    return true;
                }) { }
    }
    #pragma warning disable AV1010
    public new static IUntypedActorContext Context => (IUntypedActorContext)ActorBase.Context;

    public new IActorRef Self { get; }
    public IActorRef Parent { get; }
    public new IActorRef Sender => Context.Sender;

    #pragma warning restore AV1010
}