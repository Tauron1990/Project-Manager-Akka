using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

public interface IActionDispatcher
{
    bool CanProcess<TAction>();

    bool CanProcess(Type type);

    IObservable<TAction> ObservAction<TAction>()
        where TAction : class;

    void Dispatch(object action);
}

[PublicAPI]
public interface IRootStore : IActionDispatcher, IDisposable
{
    IObservable<object> ObserveActions();

    IRootStoreState<TState> ForState<TState>()
        where TState : new();

    void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares);

    TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware;

    void RegisterMiddlewares(params IMiddleware[] middlewares);
}

[PublicAPI]
public interface IRootStoreState<out TState>
{
    IObservable<TResult> ObserveAction<TAction, TResult>(Func<TState, TAction, TResult> resultSelector)
        where TAction : class;

    IObservable<TState> Select();

    IObservable<TResult> Select<TResult>(Func<TState, TResult> selector);
}