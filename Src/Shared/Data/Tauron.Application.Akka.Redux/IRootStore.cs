using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux;

[PublicAPI]
public interface IRootStore : IActionDispatcher, IDisposable
{
    IMaterializer Materializer { get; }

    Source<object, NotUsed> ObserveActions();

    IRootStoreState<TState> ForState<TState>()
        where TState : new();

    void RegisterMiddlewares(IEnumerable<IMiddleware> middlewares);

    TMiddleware GetMiddleware<TMiddleware>(Func<TMiddleware> factory)
        where TMiddleware : IMiddleware;

    void RegisterMiddlewares(params IMiddleware[] middlewares);
}