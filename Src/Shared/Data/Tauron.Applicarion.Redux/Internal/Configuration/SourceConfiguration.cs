using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class SourceConfiguration<TState> : ISourceConfiguration<TState>
    where TState : class, new()
{
    private readonly List<Action<IRootStore, IReduxStore<TState>>> _config = new();
    private readonly IErrorHandler _errorHandler;
    private readonly StateDb _stateDb;
    private readonly IStateFactory _stateFactory;

    public SourceConfiguration(IStateFactory stateFactory, IErrorHandler errorHandler, StateDb stateDb)
    {
        _stateFactory = stateFactory;
        _errorHandler = errorHandler;
        _stateDb = stateDb;
    }

    public IStateConfiguration<TState> FromInitial(TState? initial = default)
    {
        if(initial != null)
            _config.Add((s, _) => s.Dispatch(MutateCallback.Create(initial)));

        return new StateConfiguration<TState>(_errorHandler, _config, _stateFactory);
    }

    public IStateConfiguration<TState> FromServer(Func<CancellationToken, Task<TState>> fetcher)
        => FromServer(fetcher, (_, s) => s);

    public IStateConfiguration<TState> FromServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
    {
        _config.Add((_, store) => DynamicSource.FromRequest(_stateFactory, store, CreateRequester(fetcher, patcher)));

        return new StateConfiguration<TState>(_errorHandler, _config, _stateFactory);
    }

    public IStateConfiguration<TState> FromCacheAndServer(Func<CancellationToken, Task<TState>> fetcher)
        => FromCacheAndServer(fetcher, (_, s) => s);

    public IStateConfiguration<TState> FromCacheAndServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
    {
        _config.Add((_, store) => DynamicSource.FromCacheAndRequest(_stateFactory, store, _stateDb, CreateRequester(fetcher, patcher)));

        return new StateConfiguration<TState>(_errorHandler, _config, _stateFactory);
    }

    private static Reqester<TState> CreateRequester<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
        => async (token, currentState) =>
           {
               TToPatch request = await TimeoutToken.WithDefault(token, fetcher).ConfigureAwait(false);

               return patcher(currentState, request);
           };
}