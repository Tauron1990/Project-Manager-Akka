using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Configuration;

[PublicAPI]
public interface IRootStore : IDisposable
{
    void Dispatch(object action);

    IObservable<object> ObserveAction();

    IObservable<TAction> ObserveAction<TAction>()
        where TAction : class;

    IRootStoreState<TState> ForState<TState>()
        where TState : new();
}

[PublicAPI]
public interface IRootStoreState<out TState>
{
    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector)
        where TAction : class;

    IObservable<TState> Select();

    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);
}