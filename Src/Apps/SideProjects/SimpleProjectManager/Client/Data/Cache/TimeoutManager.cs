namespace SimpleProjectManager.Client.Data.Cache;

public sealed class TimeoutManager
{
    private readonly CacheDb _db;
    private readonly object _lock = new();

    private Task? _curentlyRunning;
    private string _currentKey = string.Empty;
    
    public TimeoutManager(CacheDb db)
        => _db = db;

    public void Start()
    {
        lock (_lock)
        {
            if(_curentlyRunning is not null) return;

            _curentlyRunning = FetchAndDelete();
        }
    }

    public bool ReNew(string key)
    {
        lock (_lock)
        {
            if (_currentKey == key)
            {
                _currentKey = string.Empty;

                return true;
            }
        }

        return false;
    }
    
    private async Task FetchAndDelete()
    {
        var (key, time) = await _db.GetNextTimeout();
        if (string.IsNullOrWhiteSpace(key))
        {
            lock (_lock) _curentlyRunning = null;
            return;
        }

        var now = DateTime.UtcNow;
        if (now < time) 
            await Task.Delay(time - now);

        await _db.DeleteElement(key);

        lock (_lock) _curentlyRunning = FetchAndDelete();
    }
}