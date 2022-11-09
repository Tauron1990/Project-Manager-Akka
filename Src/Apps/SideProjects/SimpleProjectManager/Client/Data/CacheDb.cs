﻿using System.Text.Json;
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
            (bool sucess, string? message) = await db.DeleteTimeoutElement(key);

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
        await db.DeleteElement(key);
    }

    public async ValueTask<(CacheTimeoutId? id, CacheDataId? Key, DateTime Time)> GetNextTimeout()
    {
        try
        {
            DatabaseConnection db = GetDatabseConnection();
            var all = await db.GetTimeoutElements();

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

            await UpdateTimeout(key);

            await db.UpdateData(cacheData);
        }
        catch (Exception e)
        {
            _eventAggregator.PublishError(e);
        }
    }

    public async ValueTask<string?> ReNewAndGet(CacheDataId key)
    {
        DatabaseConnection db = GetDatabseConnection();
        CacheData? result = await db.GetCacheEntry(key);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if(result is null) return null;

        await UpdateTimeout(key);

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
        CacheTimeout? timeout = await db.GetTimeout(id);
        await db.UpdateTimeout(
            timeout is null
                ? new CacheTimeout(id, key, GetTimeout())
                : timeout with { Timeout = GetTimeout() });
    }

    private sealed record InternalResult(bool Sucess, string? Message);

    private sealed class DatabaseConnection
    {
        private readonly IJSRuntime _reference;

        public DatabaseConnection(IJSRuntime reference)
            => _reference = reference;

        public async Task UpdateData(CacheData data)
        {
            await _reference.InvokeVoidAsync("window.Database.saveData", data.Id.ToString(), data);
        }

        public async Task DeleteElement(CacheDataId key)
            => await _reference.InvokeVoidAsync("window.Database.deleteElement", key.ToString(), CacheTimeoutId.FromCacheId(key).ToString());

        public async Task<CacheTimeout[]> GetTimeoutElements()
            => await _reference.InvokeAsync<CacheTimeout[]>("window.Database.getAllTimeoutElements");

        public async Task<InternalResult> DeleteTimeoutElement(CacheTimeoutId id)
            => await _reference.InvokeAsync<InternalResult>("window.Database.deleteTimeoutElement", id.ToString());

        public async Task<CacheTimeout?> GetTimeout(CacheTimeoutId id)
        {
            var result = await _reference.InvokeAsync<string>("window.Database.getTimeout", id.ToString());

            return string.IsNullOrWhiteSpace(result) ? null : JsonSerializer.Deserialize<CacheTimeout>(result);
        }

        public async Task UpdateTimeout(CacheTimeout timeout)
            => await _reference.InvokeVoidAsync("window.Database.updateTimeout", timeout.Id.ToString(), timeout);

        public async Task<CacheData?> GetCacheEntry(CacheDataId id)
        {
            #if DEBUG
            await Task.CompletedTask;

            return null;
            #else
            var result = await _reference.InvokeAsync<string>("window.Database.getCacheEntry", id.ToString());

            return string.IsNullOrWhiteSpace(result) ? null : JsonSerializer.Deserialize<CacheData>(result);
            #endif
        }
    }
}