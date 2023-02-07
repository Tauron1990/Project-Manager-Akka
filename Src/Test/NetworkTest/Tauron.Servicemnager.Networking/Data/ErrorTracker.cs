using System.Collections.Concurrent;

namespace Tauron.Servicemnager.Networking.Data;

public sealed class ErrorTracker : IDisposable
{
    private readonly string _operationName;
    private volatile ConcurrentQueue<Exception>? _exceptions = new();

    public ErrorTracker(string operationName) => _operationName = operationName;

    public void AddException(Exception ex)
    {
        var exList = _exceptions;
        if(exList is null) throw new ObjectDisposedException("Error Tracker is Disposed");
        
        exList.Enqueue(ex);
        if(exList.Count > 5)
        {
            var arr = exList.ToArray();
            if(arr.Length > 5)
                throw new AggregateException($"Mutible Error for Operation: {_operationName}", arr);
        }
        
        SheduleRemove();
    }
    
    private async void SheduleRemove()
    {
        await Task.Delay(5000).ConfigureAwait(false);
        
        var exlist = _exceptions;
        if(exlist is null) return;

        exlist.TryDequeue(out _);
    }
    
    public void Dispose() => Interlocked.Exchange(ref _exceptions, value: null);
}