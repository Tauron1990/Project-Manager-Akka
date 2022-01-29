using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Applicarion.Redux.Extensions.Cache;

namespace TestApp;

internal sealed class TestCache : ICacheDb
{
    private readonly ConcurrentDictionary<CacheTimeoutId, CacheTimeout> _timeouts = new();
    private readonly ConcurrentDictionary<CacheDataId, CacheData> _datas = new();

    public ValueTask DeleteElement(CacheTimeoutId key)
    {
        _timeouts.TryRemove(key, out _);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteElement(CacheDataId key)
    {
        _datas.Remove(key, out _);
        return ValueTask.CompletedTask;
    }

    public ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        var date = DateTime.UtcNow;
        var result = (from timeout in _timeouts
                      where timeout.Value.Timeout < date
                      orderby timeout.Value.Timeout descending
                      select timeout.Value).FirstOrDefault();

        return (result is null 
            ? ValueTask.FromResult<(CacheTimeoutId?, CacheDataId?, DateTime)>(default)! 
            : ValueTask.FromResult((result.Id, result.DataKey, result.Timeout)))!;
    }

    public ValueTask UpdateTimeout(CacheDataId key)
    {
        var id = CacheTimeoutId.FromCacheId(key);
        _timeouts.AddOrUpdate(
            id,
            timeoutId => new CacheTimeout(timeoutId, key, DateTime.UtcNow + TimeSpan.FromDays(7)),
            (_, d) => d with { Timeout = DateTime.UtcNow + TimeSpan.FromDays(7) });
        return ValueTask.CompletedTask;
    }

    public async ValueTask TryAddOrUpdateElement(CacheDataId key, string data)
    {
        await UpdateTimeout(key);
        _datas.AddOrUpdate(
            key,
            id => new CacheData(id, data),
            (_, value) => value with { Data = data });
    }

    public async ValueTask<string?> ReNewAndGet(CacheDataId key)
    {
        if (!_datas.TryGetValue(key, out var data)) return null;

        await UpdateTimeout(key);

        return data.Data;

    }
}

internal sealed class TestErrorHandler : IErrorHandler
{
    public void RequestError(string error)
    {
        Console.WriteLine();
        
        var org = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ForegroundColor = org;
        
        Console.WriteLine();
    }

    public void RequestError(Exception error)
        => RequestError(error.ToString());

    public void StateDbError(Exception error)
        => RequestError(error);

    public void TimeoutError(Exception error)
        => RequestError(error);
}

public sealed record TestState(int Counter)
{
    public TestState() : this(0) {}
}

public sealed record IncremntAction;



static class Program
{
    static async Task Main()
    {
        var coll = new ServiceCollection();
        coll.AddTransient<IErrorHandler, TestErrorHandler>();
        coll.AddSingleton<ICacheDb, TestCache>();
        coll.AddRootStore(CreateTestStore);
        
        await using var prov = coll.BuildServiceProvider();
        var store = prov.GetRequiredService<IRootStore>();
        using var _ = store.ForState<TestState>().Select(state => state.Counter).Subscribe(Console.WriteLine);
        
        await Task.Delay(3000);
        store.Dispatch(new IncremntAction());

        Console.WriteLine();
        Console.WriteLine("Fertig...");
        Console.ReadKey();
    }

    private static void CreateTestStore(IStoreConfiguration config)
    {
        config.NewState<TestState>(
            source => source
               .FromCacheAndServer(TestRequest)
               .ApplyReducer(f => f.On<IncremntAction>(ts => ts with { Counter = ts.Counter + 1 }))
               .AndFinish());
    }

    private static async Task<TestState> TestRequest(CancellationToken token)
    {
        await Task.Delay(2000, token);

        return new TestState(5);
    }
}