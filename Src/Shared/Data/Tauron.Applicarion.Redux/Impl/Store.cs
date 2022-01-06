using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Tauron.Applicarion.Redux.Impl;

internal sealed class Store<TState> : IStore<TState>
{
    private readonly TState _initialState;
    private readonly IScheduler _scheduler;
    private readonly SerialDisposable _currentPipeline = new();
    
    private readonly BehaviorSubject<TState> _currentState;
    private readonly Subject<object> _dispatcher = new();
    private readonly Subject<DispatchedAction<TState>> _dispatched = new();

    private readonly List<IMiddleware<TState>> _middlewares = new();
    private readonly List<On<TState>> _reducers = new();

    private readonly IObservable<object> _actionsStream;

    public Store(TState initialState, IScheduler scheduler)
    {
        _initialState = initialState;
        _scheduler = scheduler;
        _currentState = new BehaviorSubject<TState>(initialState);

        var disposer = new CompositeDisposable();
        var connect = _dispatcher.Synchronize().Finally(() => disposer.Dispose()).Publish();
        
        disposer.Add(connect.Connect());
        disposer.Add(_dispatched.Select(a => a.State).Subscribe(_currentState));
        disposer.Add(_currentPipeline);
        
        _actionsStream = connect.AsObservable();
        RebuildPipeline();
    }

    public void Dispose()
    {
        _currentState.OnCompleted();
        _dispatched.OnCompleted();
        _dispatcher.OnCompleted();

        _currentState.Dispose();
        _dispatched.Dispose();
        _dispatcher.Dispose();
    }

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _currentState.ObserveOn(_scheduler).Select(selector).DistinctUntilChanged();

    public IObservable<TState> Select()
        => _currentState.ObserveOn(_scheduler).DistinctUntilChanged();

    public IObservable<ActionState<TState, TAction>> ObservAction<TAction>()
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

    public void Dispatch(object action)
        => _dispatcher.OnNext(action);

    public void RegisterMiddlewares(IEnumerable<IMiddleware<TState>> middlewares)
    {
        foreach (var middleware in middlewares)
        {
            middleware.Initialize(this);
            _middlewares.Add(middleware);
        }
        RebuildPipeline();
    }

    public void RegisterReducers(IEnumerable<On<TState>> reducers)
        => _reducers.AddRange(reducers);

    public void RegisterEffects(IEnumerable<Effect<TState>> effects)
    {
        foreach (var effect in effects) 
            effect.CreateEffect(this).Retry().NotNull().Subscribe(_dispatcher);
    }

    public void RegisterMiddlewares(params IMiddleware<TState>[] middlewares)
        => RegisterMiddlewares(middlewares.AsEnumerable());

    public void RegisterReducers(params On<TState>[] reducers)
        => RegisterReducers(reducers.AsEnumerable());

    public void RegisterEffects(params Effect<TState>[] effects)
        => RegisterEffects(effects.AsEnumerable());

    private void RebuildPipeline()
    {
        var nextDispatcher = _middlewares
           .Aggregate<IMiddleware<TState>, DispatchNext<TState>>(
                RunActualReducers,
                (current, middleware) => pprovider => middleware.Connect(pprovider, current));

        _currentPipeline.Disposable = nextDispatcher(
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