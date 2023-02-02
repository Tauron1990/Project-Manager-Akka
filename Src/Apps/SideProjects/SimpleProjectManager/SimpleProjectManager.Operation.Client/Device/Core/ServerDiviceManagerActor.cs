using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Devices;

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed class ServerDiviceManagerActor : ReceiveActor
{
    private int _actorCount;
    
    public ServerDiviceManagerActor()
    {
        Receive<ClusterActorDiscoveryMessage.ActorUp>(
            _ =>
            {
                _actorCount++;
                
                if(_actorCount == 1)
                    Context.Parent.Tell(DeviceServerOnline.Instance);
            });

        Receive<ClusterActorDiscoveryMessage.ActorDown>(
            _ =>
            {
                _actorCount--;
                if(_actorCount == 0)
                    Context.Parent.Tell(DeviceServerOffline.Instance);
            });
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });
    }
    
    protected override void PreStart()
    {
        Context.Parent.Tell(DeviceServerOffline.Instance);
        ClusterActorDiscovery.Get(Context.System).MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(DeviceManagerMessages.DeviceDataId));
    }

    protected override void PostStop()
        => ClusterActorDiscovery.Get(Context.System).UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(DeviceManagerMessages.DeviceDataId));
}