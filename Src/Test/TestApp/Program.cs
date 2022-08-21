using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Shared.Data.States;
using Stl.Fusion;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

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
        await Task.Delay(1000, t);

        return _counter;
    }

    public Task<string?> Increment(IncrementCommand command, CancellationToken t)
    {
        Interlocked.Increment(ref _counter);
        using (Computed.Invalidate())
            _ = FakeServer(default);

        return Task.FromResult<string?>(string.Empty);
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
        => configuration.FromCacheAndServer(_fakeServer.FakeServer, (_, i) => new TestCounter(Count: i))
           .ApplyRequests(f => f.AddRequest<IncrementCommand>(_fakeServer.Increment, (counter, _) => new TestCounter(counter.Count + 1)));

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
        coll.AddTransient<ICacheDb, FakeCahce>();
        coll.AddTransient<IErrorHandler, FakeError>();
        coll.AddStoreConfiguration();
        coll.AddFusion().AddComputeService<IFakeServer, FakeServerImpl>();

        using var serviceProvider = coll.BuildServiceProvider();
        var stateFactory = serviceProvider.GetService<IStateFactory>();
        var configuration = serviceProvider.GetRequiredService<IStoreConfiguration>();

        var test = CreateState<CounterState>();
        var store = configuration.Build();
        
        test.ActualCount.Subscribe(Console.WriteLine);
        
        store.Dispatch(new IncrementCommand());

        Console.ReadKey();
        
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