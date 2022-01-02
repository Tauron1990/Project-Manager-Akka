using System.Reactive.Disposables;
using SimpleProjectManager.Client.Data.Cache;
using Stl.Fusion;
using Tauron.Application;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class StoreConfiguration : IStoreConfiguration
{
    private readonly IStateFactory _stateFactory;
    private readonly IEventAggregator _aggregator;
    private readonly CompositeDisposable _disposer;
    private readonly StateDb _stateDb;
    
    private readonly Dictionary<Type, Guid> _guidMapping = new();
    private readonly List<IConfiguredState> _configuredStates = new();

    public StoreConfiguration(IStateFactory stateFactory, IEventAggregator aggregator, CompositeDisposable disposer, StateDb stateDb)
    {
        _stateFactory = stateFactory;
        _aggregator = aggregator;
        _disposer = disposer;
        _stateDb = stateDb;
    }
    
    public IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator) where TState : class, new()
    {
        var guid = Guid.NewGuid();
        _guidMapping.Add(typeof(TState), guid);
        
        _configuredStates.Add(configurator(new SourceConfiguration<TState>(_stateFactory, _aggregator, guid, _disposer, _stateDb)));
        
        return this;
    }

    public IRootStore Build()
    {
        var store = new RootStore(_configuredStates);

        foreach (var configuredState in _configuredStates) 
            configuredState.PostBuild(store);

        return store;
    }
}