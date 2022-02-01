using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Extensions.Internal;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class StoreConfiguration : IStoreConfiguration
{
    private readonly IStateFactory _stateFactory;
    private readonly IErrorHandler _errorHandler;
    private readonly StateDb _stateDb;
    
    private readonly List<IConfiguredState> _configuredStates = new();
    private readonly List<Action<IReduxStore<MultiState>>> _config = new();
    private readonly List<object> _finisher = new();

    public StoreConfiguration(IStateFactory stateFactory, IErrorHandler errorHandler, StateDb stateDb)
    {
        _stateFactory = stateFactory;
        _errorHandler = errorHandler;
        _stateDb = stateDb;
    }
    
    public IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator) where TState : class, new()
    {
        var guid = Guid.NewGuid();
        
        _configuredStates.Add(configurator(new SourceConfiguration<TState>(_config, _stateFactory, _errorHandler, guid, _stateDb)));
        
        return this;
    }

    public IStoreConfiguration RegisterForFhinising(object toRegister)
    {
        _finisher.Add(toRegister);
        return this;
    }

    public IRootStore Build()
    {
        var store = new RootStore(
            _configuredStates,
            reduxStore =>
            {
                foreach (var action in _config)
                    action(reduxStore);
            });

        foreach (var configuredState in _configuredStates) 
            configuredState.PostBuild(store);

        foreach (var toCall in _finisher)
        {
            switch (toCall)
            {
                case IProvideActionDispatcher toProvideActionDispatcher:
                    toProvideActionDispatcher.StoreCreated(store.ActionDispatcher);
                    break;
                case IProvideRootStore toProvideStore:
                    toProvideStore.StoreCreated(store);
                    break;
            }
        }
        
        return store;
    }
}