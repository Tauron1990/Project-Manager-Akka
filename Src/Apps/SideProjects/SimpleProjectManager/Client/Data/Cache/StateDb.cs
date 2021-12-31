using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.Cache;

public sealed class StateDb
{
    private readonly CacheDb _db;
    private readonly TimeoutManager _timeoutManager;
    private readonly IEventAggregator _aggregator;

    public StateDb(CacheDb db, TimeoutManager timeoutManager, IEventAggregator aggregator)
    {
        _db = db;
        _timeoutManager = timeoutManager;
        _aggregator = aggregator;

        timeoutManager.Start();
    }

    public async Task Set<TState>(TState state)
    {
        try
        {
            await _db.TryAddOrUpdateElement(
                CacheDataId.FromType(typeof(TState)),
                System.Text.Json.JsonSerializer.Serialize(state));
        }
        catch (Exception e)
        {
            _aggregator.PublishError(e);
        }
    }
    
    public async Task<TState?> Get<TState>()
    {
        var data = await _timeoutManager.FetchAndReNew(CacheDataId.FromType(typeof(TState)));

        if (string.IsNullOrWhiteSpace(data))
            return default;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<TState>(data);
        }
        catch (Exception e)
        {
            _aggregator.PublishError(e);

            return default;
        }
    }
}