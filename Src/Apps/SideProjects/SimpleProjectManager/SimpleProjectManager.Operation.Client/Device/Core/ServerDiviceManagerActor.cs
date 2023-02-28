using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using Tauron;

namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed class ServerDiviceManagerActor : ReceiveActor
{
    private readonly IActorRef _supervisor;
    private readonly List<IActorRef> _manager = new();
    private int _actorCount;

    public ServerDiviceManagerActor(IActorRef supervisor)
    {
        _supervisor = supervisor;
        Receive<ClusterActorDiscoveryMessage.ActorUp>(ActorUp);

        Receive<ClusterActorDiscoveryMessage.ActorDown>(ActorDown);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });

        ReceiveAny(msg => _manager.Foreach(actor => actor.Forward(msg)));
    }

    private void ActorDown(ClusterActorDiscoveryMessage.ActorDown down)
    {
        if(_actorCount == 0)
            return;

        _actorCount--;
        _manager.Remove(down.Actor);

        if(_actorCount == 0)
            _supervisor.Tell(new DeviceServerOffline());
    }

    private void ActorUp(ClusterActorDiscoveryMessage.ActorUp up)
    {
        _actorCount++;
        _manager.Add(up.Actor);

        if(_actorCount == 1)
            _supervisor.Tell(new DeviceServerOnline());
    }

    protected override void PreStart()
    {
        _supervisor.Tell(new DeviceServerOffline());
        ClusterActorDiscovery.Get(Context.System).MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(DeviceManagerMessages.DeviceDataId));
    }

    protected override void PostStop()
        => ClusterActorDiscovery.Get(Context.System).UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(DeviceManagerMessages.DeviceDataId));
}