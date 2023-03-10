using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager.Platforms;

[SupportedOSPlatform("windows")]
public sealed partial class Win32Collector : ICollector
{
    private readonly IActorRef _owner;
    private readonly ILogger _logger;

    private readonly ManagementEventWatcher _watcher = new(
        new WqlEventQuery
        {
            EventClassName = "Win32_ProcessStartTrace",
        });

    public Win32Collector(IActorRef owner, ILogger logger)
    {
        _owner = owner;
        _logger = logger;
        
        Process.GetProcesses().Foreach(owner.Tell);
        
        _watcher.EventArrived += WatcherOnEventArrived;
        _watcher.Start();
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error on Get Process from Event")]
    private partial void ErrorOnRecieveProcess(Exception e);
    
    private void WatcherOnEventArrived(object sender, EventArrivedEventArgs e)
    {
        try
        {
            PropertyData? propertyData = e.NewEvent.Properties.OfType<PropertyData>()
                .FirstOrDefault(d => string.Equals(d.Name, "ProcessID", StringComparison.Ordinal));
            if(propertyData is null) return;

            using var process = Process.GetProcessById((int)(uint)propertyData.Value);

            _owner.Tell(process);
        }
        catch (Exception exception)
        {
            if(exception is ArgumentException or InvalidOperationException) return;

            ErrorOnRecieveProcess(exception);
        }
    }

    public void Dispose()
    {
        _watcher.Stop();
        _watcher.Dispose();
    }
}