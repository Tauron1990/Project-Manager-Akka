using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class SourceConfiguration<TState> : ISourceConfiguration<TState>
    where TState : class, new()
{
    private readonly List<Action<IReduxStore<MultiState>>> _config;
    private readonly IStateFactory _stateFactory;
    private readonly IErrorHandler _errorHandler;
    private readonly Guid _guid;
    private readonly StateDb _stateDb;

    public SourceConfiguration(List<Action<IReduxStore<MultiState>>> config, IStateFactory stateFactory, IErrorHandler errorHandler, Guid guid, StateDb stateDb)
    {
        _config = config;
        _stateFactory = stateFactory;
        _errorHandler = errorHandler;
        _guid = guid;
        _stateDb = stateDb;
    }

    public IStateConfiguration<TState> FromInitial(TState? initial = default)
    {
        MultiState Patcher(MultiState toPatch)
            => toPatch.UpdateState(_guid, initial);
        
        if(initial != null)
            _config.Add(s => s.Dispatch(MutateCallback.Create<MultiState>(Patcher)));

        return new StateConfiguration<TState>(_guid, _errorHandler, _config, _stateFactory);
    }

    private static Reqester<MultiState> CreateRequester<TToPatch>(
        Guid stateId, Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
        => async (token, multiState) =>
           {
               var currentState = multiState.GetState<TState>(stateId);
               var request = await TimeoutToken.WithDefault(token, fetcher);

               return multiState.UpdateState(stateId, patcher(currentState, request));
           };

    public IStateConfiguration<TState> FromServer(Func<CancellationToken, Task<TState>> fetcher)
        => FromServer(fetcher, (_, s) => s);

    public IStateConfiguration<TState> FromServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
    {
        _config.Add(store => DynamicSource.FromRequest(_stateFactory, store, CreateRequester(_guid, fetcher, patcher)));

        return new StateConfiguration<TState>(_guid, _errorHandler, _config, _stateFactory);
    }

    public IStateConfiguration<TState> FromCacheAndServer(Func<CancellationToken, Task<TState>> fetcher)
        => FromCacheAndServer(fetcher, (_ , s) => s);

    public IStateConfiguration<TState> FromCacheAndServer<TToPatch>(Func<CancellationToken, Task<TToPatch>> fetcher, Func<TState, TToPatch, TState> patcher)
    {
        _config.Add(store => DynamicSource.FromCacheAndRequest(_stateFactory, store, _stateDb, CreateRequester(_guid, fetcher, patcher)));

        return new StateConfiguration<TState>(_guid, _errorHandler, _config, _stateFactory);
    }
}