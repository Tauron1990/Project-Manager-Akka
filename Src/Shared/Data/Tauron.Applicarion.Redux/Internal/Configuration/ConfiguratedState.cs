using Tauron.Applicarion.Redux.Configuration;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class ConfiguratedState<TState> : IConfiguredState where TState : new()
{
    private readonly Action<IRootStore>? _onCreate;
    private readonly List<Action<IRootStore, IReduxStore<TState>>> _config;

    public ConfiguratedState(Action<IRootStore>? onCreate, List<Action<IRootStore, IReduxStore<TState>>> config)
    {
        _onCreate = onCreate;
        _config = config;
    }

    public void RunConfig(IRootStore store)
    {
        var impl = ((IInternalRootStoreState<TState>)store.ForState<TState>()).Store;
        _config.Foreach(c => c(store, impl));
    }

    public void PostBuild(IRootStore store)
        => _onCreate?.Invoke(store);
}