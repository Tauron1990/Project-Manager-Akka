using JetBrains.Annotations;

namespace Tauron.Applicarion.Redux.Extensions.Cache;

[PublicAPI]
public sealed class StateDb
{
    private readonly ICacheDb _db;
    private readonly TimeoutManager _timeoutManager;
    private readonly Action<Exception> _errorHandler;

    public StateDb(ICacheDb db, TimeoutManager timeoutManager, Action<Exception> errorHandler)
    {
        _db = db;
        _timeoutManager = timeoutManager;
        _errorHandler = errorHandler;

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
            _errorHandler(e);
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
            _errorHandler(e);

            return default;
        }
    }
}