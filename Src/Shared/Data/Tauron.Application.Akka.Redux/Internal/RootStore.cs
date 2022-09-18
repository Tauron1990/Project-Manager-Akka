using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Application.Akka.Redux.Configuration;

namespace Tauron.Application.Akka.Redux.Internal;

public sealed class RootStore : IRootStore
{
    private readonly IScheduler _scheduler;
    private readonly Action<Exception> _onError;
    private readonly Dictionary<Type, IActionDispatcher> _stores = new();
    private readonly Dictionary<Type, MiddlewareRegistration> _middlewares = new();
    private readonly Subject<object> _actions = new();
    private readonly IDisposable _subscription;

    public RootStore(IScheduler scheduler, List<IConfiguredState> configuredStates, Action<Exception> onError)
    {
        _scheduler = scheduler;
        _onError = onError;

        _subscription =
        (
            from action in _actions
            from dispatcher in from dis in _stores.Values
                               where dis.CanProcess(action.GetType())
                               select dis
            select (dispatcher, action)
        ).Subscribe(p => p.dispatcher.Dispatch(p.action));

        foreach (var state in configuredStates)
            state.RunConfig(this);
    }

    public bool CanProcess<TAction>()
        => _stores.Values.Any(d => d.CanProcess<TAction>());

    public bool CanProcess(Type type)
        => _stores.Values.Any(d => d.CanProcess(type));

    public IObservable<TAction> ObservAction<TAction>() where TAction : class
        => _actions.OfType<TAction>();

    public void Dispatch(object action)
        => _actions.OnNext(action);

    public void Dispose()
    {
        _actions.Dispose();
        _subscription.Dispose();
        _stores.Clear();
        _middlewares.Values.Foreach(m => m.Dispose());
        _middlewares.Clear();
    }

    public IObservable<object> ObserveActions()
        => _actions.AsObservable();

    public IRootStoreState<TState> ForState<TState>() where TState : new()
    {
        if(_stores.TryGetValue(typeof(TState), out var store) && store is IReduxStore<TState> reduxStore)
            return new RootStoreState<TState>(reduxStore);

        var storeState = new RootStoreState<TState>(new Store<TState>(new TState(), _scheduler, _onError));
        _stores[typeof(TState)] = storeState;

        return storeState;
    }

    public void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares)
    {
        foreach (var middleware in middlewares)
        {
            var reg = new MiddlewareRegistration(middleware);
            _middlewares.Add(middleware.GetType(), reg);
            reg.Connect(this);
        }
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory) where TMiddleware : IMiddleware
    {
        if(_middlewares.TryGetValue(typeof(TMiddleware), out var registration) && registration.Middleware is TMiddleware typed) return typed;

        registration = new MiddlewareRegistration(factory());
        _middlewares.Add(typeof(TMiddleware), registration);
        registration.Connect(this);
        
        return (TMiddleware)registration.Middleware;
    }

    public void RegisterMiddlewares(params IMiddleware[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());
    
    private sealed class MiddlewareRegistration : IDisposable
    {
        private IDisposable _subscriptions = Disposable.Empty;
        public IMiddleware Middleware { get; }

        public MiddlewareRegistration(IMiddleware middleware)
        {
            Middleware = middleware;
        }

        public void Connect(IRootStore store)
        {
            Middleware.Init(store);
            _subscriptions = Middleware
               .Connect(store)
               .Retry()
               .Subscribe(store.Dispatch);
        }

        public void Dispose()
            => _subscriptions.Dispose();
    }
}