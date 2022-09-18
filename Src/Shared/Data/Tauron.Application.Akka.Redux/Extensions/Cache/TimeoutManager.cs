namespace Tauron.Application.Akka.Redux.Extensions.Cache;

public sealed class TimeoutManager
{
    private readonly ICacheDb _db;
    private readonly Action<Exception> _errorHandler;
    private readonly object _lock = new();

    private bool _skipStart;
    private Task? _curentlyRunning;
    private CancellationTokenSource? _cancellation;
    private CacheDataId? _currentKey;
    
    public TimeoutManager(ICacheDb db, IErrorHandler errorHandler)
    {
        _db = db;
        _errorHandler = errorHandler.TimeoutError;
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
            _errorHandler(e);
        }

        if (_currentKey == key) return data;

        lock (_lock)
            _cancellation?.Cancel();
        Start();

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
            _errorHandler(e);   
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