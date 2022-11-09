namespace Tauron.Applicarion.Redux.Internal;

public sealed class RootStoreState<TState> : IRootStoreState<TState>, IActionDispatcher, IInternalRootStoreState<TState>
{
    public RootStoreState(IReduxStore<TState> store)
        => Store = store;

    public bool CanProcess<TAction>()
        => Store.CanProcess<TAction>();

    public bool CanProcess(Type type)
        => Store.CanProcess(type);

    public IObservable<TAction> ObservAction<TAction>() where TAction : class
        => Store.ObservAction<TAction>();

    public void Dispatch(object action)
        => Store.Dispatch(action);

    public IReduxStore<TState> Store { get; }

    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TState, TAction, TResult> resultSelector) where TAction : class
        => Store.ObservAction(resultSelector);

    public IObservable<TState> Select()
        => Store.Select();

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => Store.Select(selector);
}