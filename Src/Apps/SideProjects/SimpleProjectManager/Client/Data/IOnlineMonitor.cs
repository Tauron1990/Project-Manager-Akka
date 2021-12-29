namespace SimpleProjectManager.Client.Data;

public interface IOnlineMonitor
{
    IObservable<bool> Online { get; }

    Task<bool> IsOnline();
}