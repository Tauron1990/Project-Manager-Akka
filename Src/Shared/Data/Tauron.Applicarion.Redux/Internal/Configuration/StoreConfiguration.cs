using System.Reactive.Concurrency;
using Stl.Fusion;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace Tauron.Applicarion.Redux.Internal.Configuration;

public sealed class StoreConfiguration : IStoreConfiguration
{
    private readonly List<IConfiguredState> _configuredStates = new();
    private readonly IErrorHandler _errorHandler;
    private readonly List<object> _finisher = new();
    private readonly StateDb _stateDb;
    private readonly IStateFactory _stateFactory;

    public StoreConfiguration(IStateFactory stateFactory, IErrorHandler errorHandler, StateDb stateDb)
    {
        _stateFactory = stateFactory;
        _errorHandler = errorHandler;
        _stateDb = stateDb;
    }

    public IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator) where TState : class, new()
    {
        _configuredStates.Add(configurator(new SourceConfiguration<TState>(_stateFactory, _errorHandler, _stateDb)));

        return this;
    }

    public IStoreConfiguration RegisterForFhinising(object toRegister)
    {
        _finisher.Add(toRegister);

        return this;
    }

    public IRootStore Build(IScheduler? scheduler = null)
    {
        Console.WriteLine($"ReduxStore Configuration: {_configuredStates.Count}");

        var store = new RootStore(
            scheduler ?? Scheduler.Default,
            _configuredStates,
            _errorHandler.StoreError);

        foreach (IConfiguredState configuredState in _configuredStates)
            configuredState.PostBuild(store);

        foreach (object toCall in _finisher)
            switch (toCall)
            {
                case IProvideRootStore toProvideStore:
                    toProvideStore.StoreCreated(store);

                    break;
            }

        return store;
    }
}