using ReduxSimple;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class RootStore : IRootStore
{
    private readonly ReduxStore<MultiState> _reduxStore = new(Array.Empty<On<MultiState>>(), new MultiState());
    private readonly Dictionary<Type, Guid> _guidMapping = new();

    public RootStore(List<IConfiguredState> configuredStates)
    {
        foreach (var configuredState in configuredStates) 
            configuredState.RunConfig(_reduxStore, (type, guid) => _guidMapping.Add(type, guid));
    }

    public void Dispatch(object action)
        => _reduxStore.Dispatch(action);

    public IObservable<object> ObserveAction()
        => _reduxStore.ObserveAction();

    public IObservable<T> ObserveAction<T>()
        => _reduxStore.ObserveAction<T>();

    public IRootStoreState<TState> ForState<TState>() 
        where TState : new()
    {
        if (_guidMapping.TryGetValue(typeof(TState), out var id))
            return new RootStoreState<TState>(_reduxStore, state => state.GetState<TState>(id));

        throw new KeyNotFoundException("The Requested State are not Found");
    }
}

public sealed class RootStoreState<TState> : IRootStoreState<TState>
{
    private readonly ReduxStore<MultiState> _reduxStore;
    private readonly Func<MultiState, TState> _stateSelector;

    public RootStoreState(ReduxStore<MultiState> reduxStore, Func<MultiState, TState> stateSelector)
    {
        _reduxStore = reduxStore;
        _stateSelector = stateSelector;
    }
    
    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector)
        => _reduxStore.ObserveAction<TAction, TResult>((action, state) => resultSelector(action, _stateSelector(state)));

    public IObservable<TState> Select()
        => _reduxStore.Select(_stateSelector);

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _reduxStore.Select(s => selector(_stateSelector(s)));

    public IObservable<TResult> Select<TResult>(ISelectorWithoutProps<TState, TResult> selector)
        => selector.Apply(_reduxStore.Select(_stateSelector));

    public IObservable<TResult> Select<TProps, TResult>(ISelectorWithProps<TState, TProps, TResult> selector, TProps props)
        => selector.Apply(_reduxStore.Select(_stateSelector), props);
}