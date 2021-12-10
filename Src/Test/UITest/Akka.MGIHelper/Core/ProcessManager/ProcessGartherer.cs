using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using Akka.Actor;
using Akka.Event;

namespace Akka.MGIHelper.Core.ProcessManager;

public sealed class ProcessGartherer : IDisposable
{
    private readonly IActorRef _owner;
    private readonly ILoggingAdapter _log;

    private readonly ManagementEventWatcher _watcher = new(
        new WqlEventQuery
        {
            EventClassName = "Win32_ProcessStartTrace"
        });

    public ProcessGartherer(IActorRef owner, ILoggingAdapter log)
    {
        _owner = owner;
        _log = log;
        _watcher.EventArrived += WatcherOnEventArrived;
        _watcher.Start();
    }

    private void WatcherOnEventArrived(object sender, EventArrivedEventArgs e)
    {
        var propertyData = e.NewEvent.Properties.OfType<PropertyData>().FirstOrDefault(d => d.Name == "ProcessID");
        if(propertyData is null) return;

        try
        {
            using var process = Process.GetProcessById((int)(uint)propertyData.Value);
            
            _owner.Tell(process);
        }
        catch (Exception exception)
        {
            if(exception is ArgumentException or InvalidOperationException) return;
            
            _log.Error(exception, "Error on Get Process from Event");
        }
    }

    public void Dispose()
    {
        _watcher.Stop();
        _watcher.Dispose();
    }
}