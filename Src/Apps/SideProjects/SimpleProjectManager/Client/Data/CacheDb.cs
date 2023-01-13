using System.Text.Json;
using Microsoft.JSInterop;
using Tauron.Applicarion.Redux.Extensions.Cache;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data;

public sealed class CacheDb : ICacheDb
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IJSRuntime _jsRuntime;
    private DatabaseConnection? _dbContext;

    public CacheDb(IJSRuntime jsRuntime, IEventAggregator eventAggregator)
    {
        _jsRuntime = jsRuntime;
        _eventAggregator = eventAggregator;
    }

    public async ValueTask DeleteElement(CacheTimeoutId key)
    {
        try
        {
            DatabaseConnection db = GetDatabseConnection();
            (bool sucess, string? message) = await db.DeleteTimeoutElement(key).ConfigureAwait(false);

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
        DatabaseConnection db = GetDatabseConnection();
        await db.DeleteElement(key).ConfigureAwait(false);
    }

    public async ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        try
        {
            DatabaseConnection db = GetDatabseConnection();
            var all = await db.GetTimeoutElements().ConfigureAwait(false);

            CacheTimeout? entry = (from cacheTimeout in all
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

    public async ValueTask TryAddOrUpdateElement(CacheDataId key, string data)
    {
        try
        {
            DatabaseConnection db = GetDatabseConnection();

            var cacheData = new CacheData(key, data);

            await UpdateTimeout(key).ConfigureAwait(false);

            await db.UpdateData(cacheData).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }

    public async ValueTask<string?> ReNewAndGet(CacheDataId key)
    {
        DatabaseConnection db = GetDatabseConnection();
        CacheData? result = await db.GetCacheEntry(key).ConfigureAwait(false);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if(result is null) return null;

        await UpdateTimeout(key).ConfigureAwait(false);

        return result.Data;
    }

    private DatabaseConnection GetDatabseConnection()
        => _dbContext ??= new DatabaseConnection(_jsRuntime);

    private static DateTime GetTimeout()
        => DateTime.UtcNow + TimeSpan.FromDays(7);

    private async ValueTask UpdateTimeout(CacheDataId key)
    {
        DatabaseConnection db = GetDatabseConnection();

        CacheTimeoutId id = CacheTimeoutId.FromCacheId(key);
        CacheTimeout? timeout = await db.GetTimeout(id).ConfigureAwait(false);
        await db.UpdateTimeout(
                timeout is null
                    ? new CacheTimeout(id, key, GetTimeout())
                    : timeout with { Timeout = GetTimeout() })
           .ConfigureAwait(false);
    }

    private sealed record InternalResult(bool Sucess, string? Message);

    private sealed class DatabaseConnection
    {
        private readonly IJSRuntime _reference;

        internal DatabaseConnection(IJSRuntime reference)
            => _reference = reference;

        internal async Task UpdateData(CacheData data)
            => await _reference.InvokeVoidAsync("window.Database.saveData", data.Id.ToString(), data).ConfigureAwait(false);

        internal async Task DeleteElement(CacheDataId key)
            => await _reference.InvokeVoidAsync("window.Database.deleteElement", key.ToString(), CacheTimeoutId.FromCacheId(key).ToString()).ConfigureAwait(false);

        internal async Task<CacheTimeout[]> GetTimeoutElements()
            => await _reference.InvokeAsync<CacheTimeout[]>("window.Database.getAllTimeoutElements").ConfigureAwait(false);

        internal async Task<InternalResult> DeleteTimeoutElement(CacheTimeoutId id)
            => await _reference.InvokeAsync<InternalResult>("window.Database.deleteTimeoutElement", id.ToString()).ConfigureAwait(false);

        internal async Task<CacheTimeout?> GetTimeout(CacheTimeoutId id)
        {
            string result = await _reference.InvokeAsync<string>("window.Database.getTimeout", id.ToString()).ConfigureAwait(false);

            return string.IsNullOrWhiteSpace(result) ? null : JsonSerializer.Deserialize<CacheTimeout>(result);
        }

        internal async Task UpdateTimeout(CacheTimeout timeout)
            => await _reference.InvokeVoidAsync("window.Database.updateTimeout", timeout.Id.ToString(), timeout).ConfigureAwait(false);

        internal async Task<CacheData?> GetCacheEntry(CacheDataId id)
        {
            #if DEBUG
            await Task.CompletedTask.ConfigureAwait(false);

            return null;
            #else
            var result = await _reference.InvokeAsync<string>("window.Database.getCacheEntry", id.ToString()).ConfigureAwait(false);

            return string.IsNullOrWhiteSpace(result) ? null : JsonSerializer.Deserialize<CacheData>(result);
            #endif
        }
    }
}