using Microsoft.JSInterop;
using Tauron.Application;
using Tauron.Application.Blazor;
using Tavenem.Blazor.IndexedDB;

namespace SimpleProjectManager.Client.Data.Cache;

public sealed class CacheDb
{
    private readonly IndexedDbService<string> _dataDb;
    private readonly IndexedDbService<int> _timeoutDb;
    private readonly IEventAggregator _eventAggregator;

    public CacheDb(IndexedDbService<string> dataDb, IndexedDbService<int> timeoutDb, IEventAggregator eventAggregator)
    {
        _dataDb = dataDb;
        _timeoutDb = timeoutDb;
        _eventAggregator = eventAggregator;
    }

    public async Task DeleteElement(string key)
    {
        var all = await _timeoutDb.GetAllAsync<CacheTimeout>();
        var toDelete = all.FirstOrDefault(d => d.DataKey == key);
        if (toDelete != null)
            await _timeoutDb.DeleteKeyAsync(toDelete.Id);

        await _dataDb.DeleteKeyAsync(key);
    }
    
    public async Task<(string? Key, DateTime Time)> GetNextTimeout()
    {
        try
        {
            var all = await _timeoutDb.GetAllAsync<CacheTimeout>();

            var entry = (from cacheTimeout in all
                         orderby cacheTimeout.Timeout
                         select cacheTimeout)
               .FirstOrDefault();

            return entry is null ? default : (entry.DataKey, entry.Timeout);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);

            return default;
        }
    }
}