using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Application;

namespace Tauron;

[PublicAPI]
public abstract class DisposeableBase : IDisposable
{
    private int _isDisposed;
    private Action? _tracker;

    public bool IsDisposed => _isDisposed == 1;


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void ThrowDispose()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(GetType));
    }

    public void TrackDispose(Action action) => _tracker = _tracker.Combine(action);

    public void RemoveTrackDispose(Action action) => _tracker = _tracker.Remove(action);

    protected void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
            return;

        DisposeCore(disposing);

        try
        {
            _tracker?.Invoke();
        }
        catch (Exception e)
        {
            TauronEnviroment.GetLogger(GetType()).LogWarning(e, "Error on Execute Dispose Tracker");
        }
        finally
        {
            _tracker = null;
        }
    }

    protected abstract void DisposeCore(bool disposing);
}