using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Tauron.Applicarion.Redux.Extensions;

namespace Tauron.Applicarion.Redux.Internal;

internal sealed class Store<TState> : IReduxStore<TState>
{
    private readonly TState _initialState;
    private readonly IScheduler _scheduler;
    
    private readonly SerialDisposable _currentPipeline = new();
    private readonly CompositeDisposable _subscriptions = new();
    
    private readonly BehaviorSubject<TState> _currentState;
    private readonly Subject<object> _dispatcher = new();
    private readonly Subject<DispatchedAction<TState>> _dispatched = new();

    private readonly Dictionary<Type, IMiddleware<TState>> _middlewares = new();
    private readonly List<On<TState>> _reducers = new();

    private readonly IObservable<object> _actionsStream;

    public Store(TState initialState, IScheduler scheduler)
    {
        _initialState = initialState;
        _scheduler = scheduler;
        _currentState = new BehaviorSubject<TState>(initialState);
        
        _subscriptions.Add(_dispatched.Select(a => a.State).Subscribe(_currentState));
        _subscriptions.Add(_currentPipeline);
        
        _actionsStream = _dispatcher.Synchronize();
        
        RebuildPipeline();
        MutateCallbackPlugin.Install(this);
    }

    public void Dispose()
    {
        _currentState.OnCompleted();
        _dispatched.OnCompleted();
        _dispatcher.OnCompleted();

        _subscriptions.Dispose();
        _currentState.Dispose();
        _dispatched.Dispose();
        _dispatcher.Dispose();
    }

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _currentState.ObserveOn(_scheduler).Select(selector).DistinctUntilChanged();

    public IObservable<TState> Select()
        => _currentState.ObserveOn(_scheduler).DistinctUntilChanged();

    public IObservable<ActionState<TState, TAction>> ObservActionState<TAction>()
        => from actionDipatched in _dispatched.ObserveOn(_scheduler)
           where actionDipatched.Action is TAction
           select new ActionState<TState, TAction>(actionDipatched.State, ((TAction)actionDipatched.Action!)!);

    public IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector)
        => from actionDispatched in _dispatched.ObserveOn(_scheduler)
           where actionDispatched.Action is TAction
           select selector(actionDispatched.State, ((TAction)actionDispatched.Action!)!);

    public TState CurrentState => _currentState.Value;

    public void Reset()
        => _currentState.OnNext(_initialState);

    IObservable<TAction> IActionDispatcher.ObservAction<TAction>()
        => _actionsStream.OfType<TAction>();
        

    public void Dispatch(object action)
        => _dispatcher.OnNext(action);

    public void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares)
    {
        foreach (var middleware in middlewares)
        {
            middleware.Initialize(this);
            _middlewares.Add(middleware.GetType(), middleware);
        }
        RebuildPipeline();
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware<TState>
    {
        if (_middlewares.TryGetValue(typeof(TMiddleware), out var middleware))
            return (TMiddleware)middleware;

        var newInst = factory();
        RegisterMiddlewares(newInst);

        return newInst;
    }

    public void RegisterReducers(IEnumerable<On<TState>> reducers)
        => _reducers.AddRange(reducers);

    public void RegisterEffects(IEnumerable<Effect<TState>> effects)
    {
        foreach (var effect in effects) 
            effect.CreateEffect(this).Retry().NotNull().Subscribe(_dispatcher.OnNext);
    }

    public void RegisterMiddlewares(params IMiddleware<TState>[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());

    public void RegisterReducers(params On<TState>[] reducers)
        => RegisterReducers(reducers.AsEnumerable());

    public void RegisterEffects(params Effect<TState>[] effects)
        => RegisterEffects(effects.AsEnumerable());

    public IObservable<object> ObserveAction()
        => _dispatched.Select(da => da.Action).NotNull();

    public IObservable<TAction> ObserveAction<TAction>() where TAction : class
        => _dispatched.Select(da => da.Action as TAction).NotNull();

    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector)
        where TAction : class
        => from dispatchedAction in _dispatched
           let action = dispatchedAction.Action as TAction
           where action is not null
           select resultSelector(action, dispatchedAction.State);

    private void RebuildPipeline()
    {
        var nextDispatcher = _middlewares.Values
           .Aggregate<IMiddleware<TState>, DispatchNext<TState>>(
                RunActualReducers,
                (current, middleware) => pprovider => middleware.Connect(pprovider, current));

        _currentPipeline.Disposable = nextDispatcher
            (
                _actionsStream
                   .Select(action => new DispatchedAction<TState>(CurrentState, action))
            )
           .Retry()
           .Subscribe(_dispatched);
    }

    private IObservable<DispatchedAction<TState>> RunActualReducers(IObservable<DispatchedAction<TState>> actionStream)
    {
        DispatchedAction<TState> RunReducer(DispatchedAction<TState> dispatchedAction)
        {
            if (_reducers.Count == 0) return dispatchedAction;

            var (current, action) = dispatchedAction;
            var actionType = action!.GetType();
            
            return new DispatchedAction<TState>(
                _reducers.Aggregate(
                current,
                (state, on) => actionType.IsAssignableTo(on.ActionType) ? on.Mutator(state, action) : state),
                action);
        }

        return from dispatchedAction in actionStream
               where dispatchedAction.Action is not null
               select RunReducer(dispatchedAction);
    }
}