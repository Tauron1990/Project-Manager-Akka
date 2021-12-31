using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.Cache;

public sealed class TimeoutManager
{
    private readonly CacheDb _db;
    private readonly IEventAggregator _aggregator;
    private readonly object _lock = new();

    private bool _skipStart;
    private Task? _curentlyRunning;
    private CancellationTokenSource? _cancellation;
    private CacheDataId? _currentKey;
    
    public TimeoutManager(CacheDb db, IEventAggregator aggregator)
    {
        _db = db;
        _aggregator = aggregator;
    }

    public void Start()
    {
        lock (_lock)
        {
            if(_curentlyRunning is not null) return;

            if (_skipStart)
            {
                _skipStart = false;
                return;
            }
            
            _cancellation = new CancellationTokenSource();
            _curentlyRunning = FetchAndDelete();
        }
    }

    public async Task<string?> FetchAndReNew(CacheDataId key)
    {
        string? data = null;

        try
        {
            data = await _db.ReNewAndGet(key);
        }
        catch (Exception e)
        {
            _aggregator.PublishError(e);
        }

        if (_currentKey != key)
        {
            lock (_lock)
                _cancellation?.Cancel();
            Start();
        }

        return data;
    }

    private async Task FetchAndDelete()
    {
        try
        {
            var (entryId, key, time) = await _db.GetNextTimeout();

            if (_cancellation?.Token.IsCancellationRequested == true) return;

            if (key is null)
            {
                if (entryId is not null)
                    await _db.DeleteElement(entryId);
                else
                {
                    lock (_lock)
                        _skipStart = true;
                }

                return;
            }

            _currentKey = key;

            var now = DateTime.UtcNow;
            if (now < time)
                await Task.Delay(time - now, _cancellation?.Token ?? CancellationToken.None);

            if (_cancellation?.Token.IsCancellationRequested == true) return;

            await _db.DeleteElement(key);
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _aggregator.PublishError(e);   
        }
        finally
        {
            _curentlyRunning = null;
            _cancellation?.Dispose();
            _cancellation = null;
            
            Start();
        }
    }
}