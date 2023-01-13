using Akka.Actor;
using Akka.Streams;
using Stl.Fusion;
using Tauron.Application.Akka.Redux.Configuration;
using Tauron.Application.Akka.Redux.Extensions.Cache;

namespace Tauron.Application.Akka.Redux.Internal.Configuration;

public sealed class StoreConfiguration : IStoreConfiguration
{
    private readonly List<IConfiguredState> _configuredStates = new();
    private readonly IErrorHandler _errorHandler;
    private readonly List<object> _finisher = new();

    private readonly IMaterializer _materializer;
    private readonly StateDb _stateDb;
    private readonly IStateFactory _stateFactory;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public StoreConfiguration(IStateFactory stateFactory, IErrorHandler errorHandler, StateDb stateDb, ActorSystem actorSystem)
    {
        _stateFactory = stateFactory;
        _errorHandler = errorHandler;
        _stateDb = stateDb;

        _materializer = ActorMaterializer.Create(actorSystem);
    }

    public IStoreConfiguration NewState<TState>(Func<ISourceConfiguration<TState>, IConfiguredState> configurator) where TState : class, new()
    {
        _configuredStates.Add(configurator(new SourceConfiguration<TState>(_stateFactory, _errorHandler, _stateDb, _materializer)));

        return this;
    }

    public IStoreConfiguration RegisterForFhinising(object toRegister)
    {
        _finisher.Add(toRegister);

        return this;
    }

    public IRootStore Build()
    {
        Console.WriteLine($"ReduxStore Configuration: {_configuredStates.Count}");

        var store = new RootStore(
            _materializer,
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