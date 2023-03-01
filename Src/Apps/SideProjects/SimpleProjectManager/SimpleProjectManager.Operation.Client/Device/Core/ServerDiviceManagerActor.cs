using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using Tauron;
using Tauron.Features;


namespace SimpleProjectManager.Operation.Client.Device.Core;

public sealed class ServerDiviceManagerActor : ActorFeatureBase<ServerDiviceManagerActor.State>
{
    public readonly record struct State(IActorRef Supervisor, ImmutableList<IActorRef> Manager, int ActorCount);

    public static IPreparedFeature New(IActorRef supervisor)
        => Feature.Create(() => new ServerDiviceManagerActor(), _ => new State(supervisor, ImmutableList<IActorRef>.Empty, 0));
    
    private ServerDiviceManagerActor()
    {

    }

    protected override void ConfigImpl()
    {
        CallSingleHandler = true;
        
        ReceiveState<ClusterActorDiscoveryMessage.ActorUp>(ActorUp);
        ReceiveState<ClusterActorDiscoveryMessage.ActorDown>(ActorDown);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });

        Receive<object>(msg => CurrentState.Manager.Foreach(actor => actor.Forward(msg)));
    }
    
    private State ActorDown(StatePair<ClusterActorDiscoveryMessage.ActorDown, State> evt)
    {
        (ClusterActorDiscoveryMessage.ActorDown down, State state) = evt;
        
        if(state.ActorCount == 0)
            return state;

        State newData = state with
        {
            ActorCount = state.ActorCount - 1,
            Manager = state.Manager.Remove(down.Actor),
        };

        if(newData.ActorCount == 0)
            newData.Supervisor.Tell(new DeviceServerOffline());
        
        return newData;
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