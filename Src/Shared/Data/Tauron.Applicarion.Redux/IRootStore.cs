using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux;

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