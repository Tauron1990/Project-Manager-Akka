using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.States;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace TestApp;

public sealed record TestCounter(int Count)
{
    public TestCounter()
        : this(0) {}
}

public sealed class CounterState : StateBase<TestCounter>
{
    private static async Task<int> FakeServer(CancellationToken t)
    {
        await Task.Delay(3000, t);

        return 10;
    }

    public CounterState(IStateFactory stateFactory) : base(stateFactory) { }

    protected override IStateConfiguration<TestCounter> ConfigurateState(ISourceConfiguration<TestCounter> configuration)
        => configuration.FromCacheAndServer(FakeServer, (_, i) => new TestCounter(Count: i));

    public IObservable<int> ActualCount { get; private set; } = Observable.Empty<int>();

    protected override void PostConfiguration(IRootStoreState<TestCounter> state)
    {
        base.PostConfiguration(state);
        ActualCount = state.Select(tc => tc.Count);
    }
}

static class Program
{
    static Task Main()
    {
        var coll = new ServiceCollection();
        coll.AddFusion();
        coll.AddStoreConfiguration();

        using var serviceProvider = coll.BuildServiceProvider();
        var stateFactory = serviceProvider.GetService<IStateFactory>();
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();
        
        TState CreateState<TState>()
        {
            var state = ActivatorUtilities.CreateInstance<TState>(serviceProvider, stateFactory);

            if(state is IStoreInitializer baseState)
                baseState.RunConfig(configuration);

            return state;
        }
        
        return Task.CompletedTask;
    }
}