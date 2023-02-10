using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public partial class DeviceManagerStartUp
{
    private readonly DeviceEventHandler _deviceEventHandler;
    private readonly ILogger<DeviceManagerStartUp> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ActorSystem _system;

    public DeviceManagerStartUp(
        ActorSystem system, 
        DeviceEventHandler deviceEventHandler, 
        ILogger<DeviceManagerStartUp> logger,
        IHostApplicationLifetime lifetime)
    {
        _system = system;
        _deviceEventHandler = deviceEventHandler;
        _logger = logger;
        _lifetime = lifetime;
    }

    public void Run()
    {
        Task.Run(MoitorMemory, _lifetime.ApplicationStopping)
            .Ignore();
        
        _system.ActorOf(
            ServerDeviceManagerFeature.Create(_deviceEventHandler),
            DeviceInformations.ManagerName);
    }

    [LoggerMessage(Level = LogLevel.Error, EventId = 1, Message = "Error on Monitor Memeory")]
    private partial void ErrorMonitor(Exception ex);

    [LoggerMessage(Level = LogLevel.Critical, EventId = 2, Message = "Memory Limit Exeeded. Shutdown")]
    private partial void MemoryLimitShutdown();
    
    private async Task MoitorMemory()
    {
        try
        {

            long max = 3221225472; // 3GB
            
            while (!_lifetime.ApplicationStopping.IsCancellationRequested)
            {
                await Task.Delay(2000, _lifetime.ApplicationStopping).ConfigureAwait(false);

                if(GC.GetTotalMemory(forceFullCollection: false) > max)
                {
                    MemoryLimitShutdown();
                    _lifetime.StopApplication();
                }
            }
        }
        catch (Exception e)
        {
            if(e is OperationCanceledException) return;
            
            ErrorMonitor(e);
        }
    }
}