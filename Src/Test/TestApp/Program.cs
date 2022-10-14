using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using LiteDB.Async;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Async;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Applicarion.Redux.Internal;
using TestApp.Test2;

namespace TestApp;

public sealed record IncrementCommand;

public interface IFakeServer
{
    [ComputeMethod]
    Task<int> FakeServer(CancellationToken t);

    Task<string?> Increment(IncrementCommand command, CancellationToken t);
}

public class FakeServerImpl : IFakeServer
{
    private int _counter = 1;

    public virtual async Task<int> FakeServer(CancellationToken t)
    {
        await Task.Delay(1000, t).ConfigureAwait(false);

        return _counter;
    }

    public async Task<string?> Increment(IncrementCommand command, CancellationToken t)
    {
        await Task.Delay(1500, t).ConfigureAwait(false);
        
        Interlocked.Increment(ref _counter);
        using (Computed.Invalidate())
            _ = FakeServer(default);

        return string.Empty;
    }
}

public sealed class FakeError : IErrorHandler
{
    public void RequestError(string error)
        => Console.WriteLine($"Error: {error}");

    public void RequestError(Exception error)
        => RequestError(error.ToString());

    public void StateDbError(Exception error)
        => Console.WriteLine($"State DB Error: {error}");

    public void TimeoutError(Exception error)
        => Console.WriteLine($"Timeout Error: {error}");

    public void StoreError(Exception error)
        => Console.WriteLine($"Store Error: {error}");
}

public sealed class FakeCahce : ICacheDb
{
    public ValueTask DeleteElement(CacheTimeoutId key)
        => default;

    public ValueTask DeleteElement(CacheDataId key)
        => default;

    public ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
        => default;

    public ValueTask TryAddOrUpdateElement(CacheDataId key, string data)
        => default;

    public ValueTask<string?> ReNewAndGet(CacheDataId key)
        => default;
}

public sealed record TestCounter(int Count)
{
    public TestCounter()
        : this(0) {}
}

public sealed class CounterState : StateBase<TestCounter>
{
    private readonly IFakeServer _fakeServer;
    public CounterState(IFakeServer fakeServer, IStateFactory stateFactory) : base(stateFactory)
        => _fakeServer = fakeServer;

    protected override IStateConfiguration<TestCounter> ConfigurateState(ISourceConfiguration<TestCounter> configuration)
        => configuration.FromCacheAndServer(_fakeServer.FakeServer, (d, i) => d with { Count = i })
           .ApplyRequests(f => f.AddRequest<IncrementCommand>(_fakeServer.Increment, (counter, _) => counter with{ Count = counter.Count + 1}));

    public IObservable<int> ActualCount { get; private set; } = Observable.Empty<int>();

    protected override void PostConfiguration(IRootStoreState<TestCounter> state)
    {
        base.PostConfiguration(state);
        ActualCount = state.Select(tc => tc.Count);
    }
}

static class Program
{
    public sealed class TestActor : ReceiveActor
    {
    }
    
    static async Task Main()
    {
        SerialTest.Run();
        
        using var system = (ExtendedActorSystem)ActorSystem.Create("Test");

        var actor = system.ActorOf<TestActor>("TestActor");
        var selector = new ActorSelection(system.Guardian, "TestActor");
        var result = await selector.ResolveOne(TimeSpan.FromSeconds(10));

        if(actor.Equals(result))
            Console.WriteLine("Selection Ok");
        
        await system.Terminate();
        Console.ReadKey();
        Debugger.Break();
        
        var coll = new ServiceCollection();
        coll.AddTransient<ICacheDb, FakeCahce>();
        coll.AddTransient<IErrorHandler, FakeError>();
        coll.AddStoreConfiguration();
        coll.AddFusion().AddComputeService<IFakeServer, FakeServerImpl>();

        await using var serviceProvider = coll.BuildServiceProvider();
        var stateFactory = serviceProvider.GetRequiredService<IStateFactory>();
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();

        var test = CreateState<CounterState>();
        var store = configuration.Build();
        
        test.ActualCount
           .DistinctUntilChanged()
           .Subscribe(d =>
                      {
                          if(d < 10)
                              store.Dispatch(new IncrementCommand());
                          Console.WriteLine(d);
                      });
        
        
        Console.ReadKey();
        
        TState CreateState<TState>()
        {
            var state = ActivatorUtilities.CreateInstance<TState>(serviceProvider, stateFactory);

            if(state is IStoreInitializer baseState)
                baseState.RunConfig(configuration);

            return state;
        }
    }
}