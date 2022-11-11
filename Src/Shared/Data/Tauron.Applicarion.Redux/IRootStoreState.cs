using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

[PublicAPI]
public interface IRootStoreState<out TState>
{
    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TState, TAction, TResult> resultSelector)
        where TAction : class;

    IObservable<TState> Select();

    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);
}