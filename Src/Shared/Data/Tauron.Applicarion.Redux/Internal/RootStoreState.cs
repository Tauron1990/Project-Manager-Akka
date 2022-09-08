namespace Tauron.Applicarion.Redux.Internal;

public sealed class RootStoreState<TState> : IRootStoreState<TState>
{
    private readonly IReduxStore<TState> _store;

    public RootStoreState(IReduxStore<TState> store)
        => _store = store;

    public IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector) where TAction : class
        => throw new NotImplementedException();

    public IObservable<TState> Select()
        => throw new NotImplementedException();

    public IObservable<TResult> Select<TResult>(Func<TState, TResult> selector)
        => throw new NotImplementedException();
}