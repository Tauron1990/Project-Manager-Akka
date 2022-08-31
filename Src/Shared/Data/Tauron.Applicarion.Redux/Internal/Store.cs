using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Tauron.Applicarion.Redux.Extensions;

namespace Tauron.Applicarion.Redux.Internal;

internal sealed class Store<TState> : IReduxStore<TState>
{
    private readonly TState _initialState;
    
    private readonly CompositeDisposable _subscriptions = new();
    private readonly BehaviorSubject<TState> _state;
    private readonly Subject<object> _dispactcher = new();
    private readonly Subject<DispatchedAction<TState>> _actions = new();
    private readonly List<On<TState>> _reducers = new();
    private readonly Dictionary<Type, IMiddleware<TState>> _middlewares = new();

    public Store(TState initialState, IScheduler scheduler)
    {
        _state = new BehaviorSubject<TState>(initialState);
        _initialState = initialState;

        var toAction = _dispactcher.ObserveOn(scheduler);
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            toAction = _dispactcher.Synchronize();
        
        _subscriptions.Add(toAction.Select(action => new DispatchedAction<TState>(CurrentState, action)).Subscribe(_actions));
        _subscriptions.Add
        (
            _actions.Select(
                    a => _middlewares.Aggregate(
                        Observable.Return(a),
                        (observable, middleware) => middleware.Value.Connect(observable)))
               .Switch()
               .Select(
                    da => _reducers
                       .Where(on => da.GetType().IsAssignableTo(on.ActionType))
                       .Aggregate(
                            (da.State, da.Action),
                            ((tuple, on) => (on.Mutator(tuple.State, tuple.Action), tuple.Action))))
               .Select(a => a.State)
               .NotNull()
               .Subscribe(_state)
        );
        
        MutateCallbackPlugin.Install(this);
    }

    public IObservable<TAction> ObservAction<TAction>()
        => _actions.Select(da => da.Action).NotNull().OfType<TAction>();

    public void Dispatch(object action)
        => _dispactcher.OnNext(action);

    public void Dispose()
    {
        _subscriptions.Dispose();
        _state.Dispose();
        _actions.Dispose();
        _reducers.Clear();
        _middlewares.Clear();
    }

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _state.Select(selector);

    public IObservable<TState> Select()
        => _state.AsObservable();

    public IObservable<ActionState<TState, TAction>> ObservActionState<TAction>()
        => _actions.Select(da => da.Action).NotNull().OfType<TAction>().Select(a => new ActionState<TState, TAction>(CurrentState, a));

    public IObservable<TResult> ObservAction<TAction, TResult>(Func<TState, TAction, TResult> selector)
        => _actions.Select(da => da.Action).NotNull().OfType<TAction>().Select(a => selector(CurrentState, a));

    public TState CurrentState => _state.Value;
    public void Reset()
        => _state.OnNext(_initialState);

    public void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares)
    {
        foreach (var middleware in middlewares)
        {
            _middlewares.Add(middleware.GetType(), middleware);
            middleware.Initialize(this);
        }
    }

    public TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory) where TMiddleware : IMiddleware<TState>
    {
        if(_middlewares.TryGetValue(typeof(TMiddleware), out var middleware) && middleware is TMiddleware casted) return casted;

        var value = factory();
        _middlewares.Add(typeof(TMiddleware), value);
        value.Initialize(this);

        return value;
    }

    public void RegisterReducers(IEnumerable<On<TState>> reducers)
        => _reducers.AddRange(reducers);

    public void RegisterEffects(IEnumerable<Effect<TState>> effects)
    {
        foreach (var effect in effects)
            effect.CreateEffect(this).Retry().NotNull().Subscribe(_dispactcher.OnNext);
    }

    public void RegisterMiddlewares(params IMiddleware<TState>[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());

    public void RegisterReducers(params On<TState>[] reducers)
        => _reducers.AddRange(reducers);

    public void RegisterEffects(params Effect<TState>[] effects)
        => RegisterEffects(effects.AsEnumerable());

    public IObservable<object> ObserveAction()
        => _actions.Select(da => da.Action).NotNull().AsObservable();

    public IObservable<TAction> ObserveAction<TAction>() where TAction : class
        => _actions.Select(da => da.Action).NotNull().OfType<TAction>();

    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector) where TAction : class
        => from dispatched in _actions
           let action = dispatched.Action as TAction
           where action is not null
           select resultSelector(action, dispatched.State);
}

/*internal sealed class StoreOld<TState> : IReduxStore<TState>
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
                (current, middleware) => pprovider => middleware.Connect(pprovider));

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
}*/