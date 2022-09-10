namespace Tauron.Applicarion.Redux.Internal;

public sealed class RootStoreState<TState> : IRootStoreState<TState>, IActionDispatcher, IInternalRootStoreState<TState>
{
    private readonly IReduxStore<TState> _store;

    public RootStoreState(IReduxStore<TState> store)
        => _store = store;

    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TState, TAction, TResult> resultSelector) where TAction : class
        => _store.ObservAction(resultSelector);

    public IObservable<TState> Select()
        => _store.Select();

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => _store.Select(selector);

    public bool CanProcess<TAction>()
        => _store.CanProcess<TAction>();

    public bool CanProcess(Type type)
        => _store.CanProcess(type);

    public IObservable<TAction> ObservAction<TAction>() where TAction : class
        => _store.ObservAction<TAction>();

    public void Dispatch(object action)
        => _store.Dispatch(action);

    public IReduxStore<TState> Store => _store;
}