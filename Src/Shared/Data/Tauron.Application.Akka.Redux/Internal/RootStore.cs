using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using Tauron.Application.Akka.Redux.Configuration;

namespace Tauron.Application.Akka.Redux.Internal;

public sealed class RootStore : IRootStore
{
    private readonly Sink<object, NotUsed> _actions;

    private readonly SharedKillSwitch _close = KillSwitches.Shared("Cancel_Root_Store");
    private readonly Source<object, NotUsed> _dispatcher;
    private readonly ISourceQueueWithComplete<object> _forDispatching;
    private readonly Dictionary<Type, MiddlewareRegistration> _middlewares = new();
    private readonly Action<Exception> _onError;

    private readonly Dictionary<Type, IActionDispatcher> _stores = new();

    public RootStore(IMaterializer materializer, List<IConfiguredState> configuredStates, Action<Exception> onError)
    {
        Materializer = materializer;
        _onError = onError;

        var (actionCollector, toDispatch) = MergeHub.Source<object>().PreMaterialize(materializer);
        var (dispatcher, collector) = BroadcastHub.Sink<object>().PreMaterialize(materializer);

        toDispatch.Via(_close.Flow<object>()).RunWith(collector, materializer);
        _forDispatching = Source.Queue<object>(5, OverflowStrategy.Backpressure)
           .Via(_close.Flow<object>())
           .ToMaterialized(actionCollector, Keep.Left)
           .Run(materializer);

        _actions = actionCollector;
        _dispatcher = dispatcher;

        foreach (IConfiguredState state in configuredStates)
            state.RunConfig(this);
    }

    public bool CanProcess<TAction>()
        => _stores.Values.Any(d => d.CanProcess<TAction>());

    public bool CanProcess(Type type)
        => _stores.Values.Any(d => d.CanProcess(type));

    public Source<TAction, NotUsed> ObservAction<TAction>() where TAction : class
        => from action in _dispatcher
           where action is TAction
           select (TAction)action;

    public Task<IQueueOfferResult> Dispatch(object action)
        => _forDispatching.OfferAsync(action);

    public Sink<object, NotUsed> Dispatcher()
        => _actions;


    public void Dispose()
    {
        _forDispatching.Complete();
        _close.Shutdown();
        _stores.Clear();
        _middlewares.Clear();
    }


    public IMaterializer Materializer { get; }

    public Source<object, NotUsed> ObserveActions()
        => _dispatcher;

    public IRootStoreState<TState> ForState<TState>() where TState : new()
    {
        if(_stores.TryGetValue(typeof(TState), out IActionDispatcher? store) && store is IReduxStore<TState> reduxStore)
            return new RootStoreState<TState>(reduxStore);

        var storeState = new RootStoreState<TState>(new Store<TState>(new TState(), _onError, Materializer));
        _stores[typeof(TState)] = storeState;

        return storeState;
    }

    public void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares)
    {
        foreach (IMiddleware middleware in middlewares)
        {
            var reg = new MiddlewareRegistration(middleware, _close);
            _middlewares.Add(middleware.GetType(), reg);
            reg.Connect(this);
        }
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory) where TMiddleware : IMiddleware
    {
        if(_middlewares.TryGetValue(typeof(TMiddleware), out MiddlewareRegistration? registration) && registration.Middleware is TMiddleware typed) return typed;

        registration = new MiddlewareRegistration(factory(), _close);
        _middlewares.Add(typeof(TMiddleware), registration);
        registration.Connect(this);

        return (TMiddleware)registration.Middleware;
    }

    public void RegisterMiddlewares(params IMiddleware[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());

    private sealed class MiddlewareRegistration
    {
        private readonly SharedKillSwitch _killSwitch;

        internal MiddlewareRegistration(IMiddleware middleware, SharedKillSwitch killSwitch)
        {
            _killSwitch = killSwitch;
            Middleware = middleware;
        }

        internal IMiddleware Middleware { get; }

        internal void Connect(IRootStore store)
        {
            Middleware.Init(store);
            Middleware.Connect(store)
               .Via(_killSwitch.Flow<object>())
               .RunWith(store.Dispatcher(), store.Materializer);
        }
    }
}