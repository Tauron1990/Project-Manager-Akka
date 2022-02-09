using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;
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

internal sealed class FakeOnlineModitor : IOnlineMonitor
{
    public IObservable<bool> Online { get; } = Observable.Return(true);
    public Task<bool> IsOnline()
        => Task.FromResult(true);
}

#pragma warning disable EX002

internal sealed class FakeJobDatabaseService : IJobDatabaseService
{
    public Task<JobInfo[]> GetActiveJobs(CancellationToken token)
        => Task.FromResult(Array.Empty<JobInfo>());

    public Task<SortOrder[]> GetSortOrders(CancellationToken token)
        => Task.FromResult(Array.Empty<SortOrder>());

    public Task<JobData> GetJobData(ProjectId id, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<long> CountActiveJobs(CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> DeleteJob(ProjectId id, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> CreateJob(CreateProjectCommand command, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> UpdateJobData(UpdateProjectCommand command, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token)
        => throw new InvalidOperationException();
}

internal sealed class FakeErrorService : ICriticalErrorService
{
    public Task<long> CountErrors(CancellationToken token)
        => Task.FromResult(0L);

    public Task<CriticalError[]> GetErrors(CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> DisableError(string id, CancellationToken token)
        => throw new InvalidOperationException();

    public Task WriteError(CriticalError error, CancellationToken token)
        => throw new InvalidOperationException();
}

internal sealed class FakeFileService : IJobFileService
{
    public Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<DatabaseFile[]> GetAllFiles(CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> CommitFiles(FileList files, CancellationToken token)
        => throw new InvalidOperationException();

    public Task<string> DeleteFiles(FileList files, CancellationToken token)
        => throw new InvalidOperationException();
}

static class Program
{
    static async Task Main()
    {
        var coll = new ServiceCollection();
        coll.AddTransient<IErrorHandler, TestErrorHandler>();
        coll.AddSingleton<ICacheDb, TestCache>();

        coll.AddSingleton<IOnlineMonitor, FakeOnlineModitor>();
        coll.AddTransient<IJobDatabaseService, FakeJobDatabaseService>();
        coll.AddTransient<ICriticalErrorService, FakeErrorService>();
        coll.AddTransient<IJobFileService, FakeFileService>();

        coll.AddSingleton<GlobalState>();
        coll.AddStoreConfiguration();

        coll.RegisterModule<CommonModule>();
        
        await using var prov = coll.BuildServiceProvider();
        var store = prov.GetRequiredService<GlobalState>();
        
        await Task.Delay(3000);

        Console.WriteLine();
        Console.WriteLine("Fertig...");
        Console.ReadKey();
    }
}