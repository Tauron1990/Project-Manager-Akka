using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed class ServerDiviceManagerActor : ReceiveActor
{
    private readonly List<IActorRef> _manager = new();
    private int _actorCount;

    public ServerDiviceManagerActor()
    {
        Receive<ClusterActorDiscoveryMessage.ActorUp>(
            up =>
            {
                _actorCount++;
                _manager.Add(up.Actor);
                
                if(_actorCount == 1)
                    Context.Parent.Tell(DeviceServerOnline.Instance);
            });

        Receive<ClusterActorDiscoveryMessage.ActorDown>(
            down =>
            {
                _actorCount--;
                _manager.Remove(down.Actor);
                
                if(_actorCount == 0)
                    Context.Parent.Tell(DeviceServerOffline.Instance);
            });
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });

        ReceiveAny(msg => _manager.Foreach(actor => actor.Forward(msg)));
    }

    protected override void PreStart()
    {
        Context.Parent.Tell(DeviceServerOffline.Instance);
        ClusterActorDiscovery.Get(Context.System).MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(DeviceManagerMessages.DeviceDataId));
    }

    protected override void PostStop()
        => ClusterActorDiscovery.Get(Context.System).UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(DeviceManagerMessages.DeviceDataId));
}