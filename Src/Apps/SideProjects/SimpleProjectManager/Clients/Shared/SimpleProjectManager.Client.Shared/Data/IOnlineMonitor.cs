using System;
using System.Threading.Tasks;

namespace SimpleProjectManager.Client.Shared.Data;

public interface IOnlineMonitor
{
    IObservable<bool> Online { get; }

    Task<bool> IsOnline();
}