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
    private readonly Action<Exception> _onError;

    private readonly CompositeDisposable _subscriptions = new();
    private readonly BehaviorSubject<TState> _state;
    private readonly Subject<object> _dispactcher = new();
    private readonly Subject<DispatchedAction<TState>> _actions = new();
    private readonly List<On<TState>> _reducers = new();
    private readonly HashSet<Type> _registratedActions = new();

    public Store(TState initialState, IScheduler scheduler, Action<Exception> onError)
    {
        _state = new BehaviorSubject<TState>(initialState);
        _initialState = initialState;
        _onError = onError;

        var toAction = _dispactcher.ObserveOn(scheduler);
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            toAction = _dispactcher.Synchronize();
        
        _subscriptions.Add(toAction.Select(action => new DispatchedAction<TState>(CurrentState, action)).Subscribe(_actions));
        _subscriptions.Add
        (
            _actions
               .Select(
                    da => _reducers
                       .Where(on => da.GetType().IsAssignableTo(on.ActionType))
                       .Aggregate(
                            (da.State, da.Action),
                            ((tuple, on) => (on.Mutator(tuple.State, tuple.Action), tuple.Action))))
               .Select(a => a.State)
               .NotNull()
               .Do(static _ => { }, _onError)
               .Retry()
               .Subscribe(_state)
        );
        
        MutateCallbackPlugin.Install(this);
    }

    public bool CanProcess(Type type)
        => _registratedActions.Contains(type);

    public IObservable<TAction> ObservAction<TAction>() where TAction : class
        => _actions.Select(da => da.Action).NotNull().OfType<TAction>();

    public void Dispatch(object action)
        => _dispactcher.OnNext(action);

    public void Dispose()
    {
        _subscriptions.Dispose();
        _state.Dispose();
        _actions.Dispose();
        _reducers.Clear();
    }

    public bool CanProcess<TAction>()
        => _registratedActions.Contains(typeof(TAction));

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

    public void RegisterReducers(IEnumerable<On<TState>> reducers)
    {
        foreach (var reducer in reducers)
        {
            _reducers.Add(reducer);
            _registratedActions.Add(reducer.ActionType);
        }
    }

    public void RegisterEffects(IEnumerable<Effect<TState>> effects)
    {
        foreach (var effect in effects)
            effect.CreateEffect(this).Do(
                static _ => { },
                _onError).Retry().NotNull().Subscribe(_dispactcher.OnNext);
    }

    public void RegisterReducers(params On<TState>[] reducers)
        => RegisterReducers(reducers.AsEnumerable());

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