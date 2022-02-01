using System.Collections.Immutable;
using System.Reactive.Concurrency;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class RootStore : IRootStore
{
    private readonly IReduxStore<MultiState> _reduxStore = new Store<MultiState>(new MultiState(ImmutableDictionary<Guid, StateData>.Empty), Scheduler.Default);
    private readonly Dictionary<Type, Guid> _guidMapping = new();

    internal IActionDispatcher ActionDispatcher => _reduxStore;
    
    public RootStore(List<IConfiguredState> configuredStates, Action<IReduxStore<MultiState>> config)
    {
        config(_reduxStore);
        
        foreach (var configuredState in configuredStates) 
            configuredState.RunConfig(_reduxStore, (type, guid) => _guidMapping.Add(type, guid));
    }

    public void Dispatch(object action)
        => _reduxStore.Dispatch(action);

    public IObservable<object> ObserveAction()
        => _reduxStore.ObserveAction();

    public IObservable<T> ObserveAction<T>() where T : class
        => _reduxStore.ObserveAction<T>();

    public IRootStoreState<TState> ForState<TState>() 
        where TState : new()
    {
        if (_guidMapping.TryGetValue(typeof(TState), out var id))
            return new RootStoreState<TState>(_reduxStore, state => state.GetState<TState>(id));

        throw new KeyNotFoundException("The Requested State are not Found");
    }

    public void Dispose()
        => _reduxStore.Dispose();
}

public sealed class RootStoreState<TState> : IRootStoreState<TState>
{
    private readonly IReduxStore<MultiState> _reduxStore;
    private readonly Func<MultiState, TState> _stateSelector;

    public RootStoreState(IReduxStore<MultiState> reduxStore, Func<MultiState, TState> stateSelector)
    {
        _reduxStore = reduxStore;
        _stateSelector = stateSelector;
    }
    
    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector) where TAction : class
        => _reduxStore.ObserveAction<TAction, TResult>((action, state) => resultSelector(action, _stateSelector(state)));

    public IObservable<TState> Select()
        => _reduxStore.Select(_stateSelector);

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _reduxStore.Select(s => selector(_stateSelector(s)));
}