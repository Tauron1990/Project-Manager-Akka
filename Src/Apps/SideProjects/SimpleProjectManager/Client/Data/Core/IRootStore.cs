using ReduxSimple;

namespace SimpleProjectManager.Client.Data.Core;

public interface IRootStore
{
    void Dispatch(object action);

    IObservable<object> ObserveAction();

    IObservable<T> ObserveAction<T>();

    IRootStoreState<TState> ForState<TState>()
        where TState : new();
}

public interface IRootStoreState<out TState>
{
    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TAction, TState, TResult> resultSelector);

    IObservable<TState> Select();

    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);

    IObservable<TResult> Select<TResult>(ISelectorWithoutProps<TState, TResult> selector);

    IObservable<TResult> Select<TProps, TResult>(ISelectorWithProps<TState, TProps, TResult> selector, TProps props);
}