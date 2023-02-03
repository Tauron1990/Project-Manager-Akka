using Akka.Actor;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Server.Core.DeviceManager.Events;

namespace SimpleProjectManager.Server.Core.DeviceManager;

public class DeviceManagerStartUp
{
    private readonly DeviceEventHandler _deviceEventHandler;
    private readonly ActorSystem _system;

    public DeviceManagerStartUp(ActorSystem system, DeviceEventHandler deviceEventHandler)
    {
        _system = system;
        _deviceEventHandler = deviceEventHandler;
    }

    public void Run()
        => _system.ActorOf(
            ServerDeviceManagerFeature.Create(_deviceEventHandler), 
            DeviceInformations.ManagerName);
}