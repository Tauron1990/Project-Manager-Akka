using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux.Internal;

public sealed class RootStore : IRootStore
{
    private readonly Subject<object> _actions = new();
    private readonly Dictionary<Type, MiddlewareRegistration> _middlewares = new();
    private readonly Action<Exception> _onError;
    private readonly IScheduler _scheduler;
    private readonly Dictionary<Type, IActionDispatcher> _stores = new();
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

        foreach (IConfiguredState state in configuredStates)
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
        if(_stores.TryGetValue(typeof(TState), out IActionDispatcher? store) && store is IRootStoreState<TState> reduxStore)
            return reduxStore;

        var storeState = new RootStoreState<TState>(new Store<TState>(new TState(), _scheduler, _onError));
        _stores[typeof(TState)] = storeState;

        return storeState;
    }

    public void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares)
    {
        foreach (IMiddleware middleware in middlewares)
        {
            var reg = new MiddlewareRegistration(middleware, _onError);
            _middlewares.Add(middleware.GetType(), reg);
            reg.Connect(this);
        }
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory) where TMiddleware : IMiddleware
    {
        if(_middlewares.TryGetValue(typeof(TMiddleware), out MiddlewareRegistration? registration) && registration.Middleware is TMiddleware typed) return typed;

        registration = new MiddlewareRegistration(factory(), _onError);
        _middlewares.Add(typeof(TMiddleware), registration);
        registration.Connect(this);

        return (TMiddleware)registration.Middleware;
    }

    public void RegisterMiddlewares(params IMiddleware[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());

    private sealed class MiddlewareRegistration : IDisposable
    {
        private readonly Action<Exception> _error;
        private IDisposable _subscriptions = Disposable.Empty;

        public MiddlewareRegistration(IMiddleware middleware, Action<Exception> error)
        {
            _error = error;
            Middleware = middleware;
        }

        public IMiddleware Middleware { get; }

        public void Dispose()
            => _subscriptions.Dispose();

        public void Connect(IRootStore store)
        {
            Middleware.Init(store);
            _subscriptions = Middleware
               .Connect(store)
               .RetryWhen(
                    exSource => exSource.Select(
                        ex =>
                        {
                            _error(ex);

                            return Unit.Default;
                        }))
               .Subscribe(store.Dispatch);
        }
    }
}