using Microsoft.JSInterop;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class CacheDb : ICacheDb
{
    private const string ScriptImport = "./Database/DatabaseContext";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _dbContext;
    private readonly IEventAggregator _eventAggregator;

    public CacheDb(IJSRuntime jsRuntime, IEventAggregator eventAggregator)
    {
        _jsRuntime = jsRuntime;
        _eventAggregator = eventAggregator;
    }

    private async ValueTask<IJSObjectReference> GetDatabseConnection()
        => _dbContext ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ScriptImport);

    public async ValueTask DeleteElement(CacheTimeoutId key)
    {
        try
        {
            var db = await GetDatabseConnection();
            var (sucess, message) = await db.InvokeAsync<InternalResult>("deleteTimeoutElement", key.ToString());

            if(sucess) return;

            _eventAggregator.PublishError(message);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }
    
    public async ValueTask DeleteElement(CacheDataId key)
    {
        var db = await GetDatabseConnection();
        await db.InvokeVoidAsync("deleteElement", key.ToString(), CacheTimeoutId.FromCacheId(key).ToString());
    }
    
    public async ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        try
        {
            var db = await GetDatabseConnection();
            var all = await db.InvokeAsync<CacheTimeout[]>("getAllTimeoutElements");

            var entry = (from cacheTimeout in all
                         orderby cacheTimeout.Timeout
                         select cacheTimeout)
               .FirstOrDefault();

            return entry is null ? default : (entry.Id, entry.DataKey, entry.Timeout);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);

            return default;
        }
    }

    private static DateTime GetTimeout()
        => DateTime.UtcNow + TimeSpan.FromDays(7);

    private async ValueTask UpdateTimeout(CacheDataId key)
    {
        var db = GetDatabseConnection();
        
        var id = CacheTimeoutId.FromCacheId(key);
        var timeout = await _timeoutDb.GetValueAsync<CacheTimeout>(id.ToString());
        await _timeoutDb.PutValueAsync(
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            timeout is null
                ? new CacheTimeout(id, key, GetTimeout())
                : timeout with { Timeout = GetTimeout() });
    }
    
    public async ValueTask TryAddOrUpdateElement(CacheDataId key, string data)
    {
        try
        {
            var cacheData = new CacheData(key, data);

            await UpdateTimeout(key);
            
            await _dataDb.PutValueAsync(cacheData);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }

    public async ValueTask<string?> ReNewAndGet(CacheDataId key)
    {
        var result = await _dataDb.GetValueAsync<CacheData>(key.ToString());

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (result is null) return null;

        await UpdateTimeout(key);

        return result.Data;
    }

    private sealed record InternalResult(bool Sucess, string? Message);
    
    private sealed class DatabaseConnection
    {
        private readonly IJSObjectReference _reference;

        public DatabaseConnection(IJSObjectReference reference)
            => _reference = reference;

        public async Task DeleteElement(CacheDataId key)
            => await _reference.InvokeVoidAsync("deleteElement", key.ToString(), CacheTimeoutId.FromCacheId(key).ToString());

        public async Task<CacheTimeout[]> GetTimeoutElements()
            => await _reference.InvokeAsync<CacheTimeout[]>("getAllTimeoutElements");
    }
} 