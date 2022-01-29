using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Application;
using Tauron.Application.Blazor;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data.Core;

public sealed class CacheDb : ICacheDb
{
    private readonly IndexedDbService<CacheDataId> _dataDb;
    private readonly IndexedDbService<CacheTimeoutId> _timeoutDb;
    private readonly IEventAggregator _eventAggregator;

    public CacheDb(IndexedDbService<CacheDataId> dataDb, IndexedDbService<CacheTimeoutId> timeoutDb, IEventAggregator eventAggregator)
    {
        _dataDb = dataDb;
        _timeoutDb = timeoutDb;
        _eventAggregator = eventAggregator;
    }

    public async ValueTask DeleteElement(CacheTimeoutId key)
    {
        try
        {
            await _timeoutDb.DeleteKeyAsync(key);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }
    
    public async ValueTask DeleteElement(CacheDataId key)
    {
        var all = await _timeoutDb.GetAllAsync<CacheTimeout>();
        var toDelete = all.FirstOrDefault(d => d.DataKey == key);
        if (toDelete != null)
            await _timeoutDb.DeleteKeyAsync(toDelete.Id);

        await _dataDb.DeleteKeyAsync(key);
    }
    
    public async ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        try
        {
            var all = await _timeoutDb.GetAllAsync<CacheTimeout>();

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
        var id = CacheTimeoutId.FromCacheId(key);
        var timeout = await _timeoutDb.GetValueAsync<CacheTimeout>(id);
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
        var result = await _dataDb.GetValueAsync<CacheData>(key);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (result is null) return null;

        await UpdateTimeout(key);

        return result.Data;
    }
} 