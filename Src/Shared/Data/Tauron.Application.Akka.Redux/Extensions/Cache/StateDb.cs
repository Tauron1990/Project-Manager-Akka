using System.Text.Json;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Extensions.Cache;

[PublicAPI]
public sealed class StateDb
{
    private readonly ICacheDb _db;
    private readonly Action<Exception> _errorHandler;
    private readonly TimeoutManager _timeoutManager;

    public StateDb(ICacheDb db, TimeoutManager timeoutManager, IErrorHandler errorHandler)
    {
        _db = db;
        _timeoutManager = timeoutManager;
        _errorHandler = errorHandler.StateDbError;

        timeoutManager.Start();
    }

    public async Task Set<TState>(TState state)
    {
        try
        {
            await _db.TryAddOrUpdateElement(
                CacheDataId.FromType(typeof(TState)),
                JsonSerializer.Serialize(state));
        }
        catch (Exception e)
        {
            _errorHandler(e);
        }
    }

    public async Task<TState?> Get<TState>()
    {
        string? data = await _timeoutManager.FetchAndReNew(CacheDataId.FromType(typeof(TState)));

        if(string.IsNullOrWhiteSpace(data))
            return default;

        try
        {
            return JsonSerializer.Deserialize<TState>(data);
        }
        catch (Exception e)
        {
            _errorHandler(e);

            return default;
        }
    }
}