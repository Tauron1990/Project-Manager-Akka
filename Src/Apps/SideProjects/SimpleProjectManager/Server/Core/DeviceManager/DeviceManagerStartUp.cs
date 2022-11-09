using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
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
    {
        Props? singleTon = ClusterSingletonManager.Props(ServerDeviceManagerFeature.Create(_deviceEventHandler), ClusterSingletonManagerSettings.Create(_system));
        _system.ActorOf(singleTon, DeviceInformations.ManagerName);
    }
}