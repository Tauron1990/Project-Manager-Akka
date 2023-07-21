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
    private readonly List<ISignal> _currentWaiting = new();
    private readonly Subject<object> _receiver = new();
    private readonly CompositeDisposable _resources = new();

    private readonly Dictionary<Type, object> _selectors = new();
    private readonly BehaviorSubject<IActorContext?> _start = new(value: null);
    private readonly BehaviorSubject<IActorContext?> _stop = new(value: null);

    private bool _isReceived;

    public ObservableActor()
    {
        Self = base.Self;
        Parent = ActorBase.Context.Parent;

        _resources.Add(_receiver);
        _resources.Add(_start);
        _resources.Add(_stop);

        Log = TauronEnviroment.GetLogger(GetType());
    }

    public bool CallSingleHandler { get; set; }

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
        => AddResource(new ObservableInvoker<TEvent, Unit>(handler, ThrowError, GetSelector<TEvent>()).Construct());

    public IObservable<TEvent> Receive<TEvent>() => GetSelector<TEvent>();

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler)
        => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, ThrowError, GetSelector<TEvent>()).Construct());

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<Unit>> handler, Func<Exception, bool> errorHandler)
        => AddResource(new ObservableInvoker<TEvent, Unit>(handler, errorHandler, GetSelector<TEvent>()).Construct());

    public void Receive<TEvent>(Func<IObservable<TEvent>, IObservable<TEvent>> handler, Func<Exception, bool> errorHandler)
        => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, errorHandler, GetSelector<TEvent>()).Construct());


    public void Receive<TEvent>(Func<IObservable<TEvent>, IDisposable> handler)
        => AddResource(new ObservableInvoker<TEvent, TEvent>(handler, GetSelector<TEvent>(), isSafe: true).Construct());

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
            case Status.Failure when _selectors.ContainsKey(typeof(Status.Failure)) || _currentWaiting.Any(signal => signal.Match(message)):
                return RunDefault();
            case Status.Failure failure:
                if(OnError(failure))
                    throw failure.Cause;

                return true;
            case Status.Success when _selectors.ContainsKey(typeof(Status.Success)) || _currentWaiting.Any(signal => signal.Match(message)):
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
        _receiver.OnCompleted();
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
    {
        _isReceived = false;

        _receiver.OnNext(message);

        return _isReceived;
    }

    public void TellSelf(object msg) => _receiver.OnNext(msg);

    protected virtual bool OnError(Status.Failure failure) => ThrowError(failure.Cause);

    public IObservable<TSignal> WaitForSignal<TSignal>(TimeSpan timeout, Predicate<TSignal> match)
    {
        var source = new TaskCompletionSource<TSignal>(TaskCreationOptions.RunContinuationsAsynchronously);
        var signal = new ConcrretSignal<TSignal>(source, match);
        Self.Tell(new AddSignal(signal));
        Task.Delay(timeout)
            .PipeTo(Self, success: () => new SignalTimeOut(signal))
            .LogTaskError($"Timeout Pipe Error {GetType()}", Log);

        return source.Task.ToObservable();
    }

    protected IObservable<TEvent> GetSelector<TEvent>()
    {
        if(_selectors.TryGetValue(typeof(TEvent), out object? selector)) return (IObservable<TEvent>)selector;

        selector = _receiver
           .Where(msg => msg is TEvent && (!CallSingleHandler || !_isReceived))
           .Select(
                msg =>
                {
                    _isReceived = true;

                    return (TEvent)msg;
                })
           .Isonlate();

        _selectors[typeof(TEvent)] = selector;

        return (IObservable<TEvent>)selector;
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