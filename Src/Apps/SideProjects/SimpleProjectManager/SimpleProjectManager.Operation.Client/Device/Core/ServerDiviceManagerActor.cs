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

        Start.Subscribe(PreStart);
        Stop.Subscribe(PostStop);
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

    private State ActorUp(StatePair<ClusterActorDiscoveryMessage.ActorUp, State> up)
    {
        State newState = up.State with
        {
            ActorCount = up.State.ActorCount + 1,
            Manager = up.State.Manager.Add(up.Event.Actor),
        };
        
        if(newState.ActorCount == 1)
            newState.Supervisor.Tell(new DeviceServerOnline());

        return newState;
    }

    private void PreStart(IActorContext context)
    {
        CurrentState.Supervisor.Tell(new DeviceServerOffline());
        ClusterActorDiscovery.Get(Context.System).MonitorActor(new ClusterActorDiscoveryMessage.MonitorActor(DeviceManagerMessages.DeviceDataId));
    }
    

    private void PostStop(IActorContext context)
        => ClusterActorDiscovery.Get(Context.System).UnMonitorActor(new ClusterActorDiscoveryMessage.UnmonitorActor(DeviceManagerMessages.DeviceDataId));
    
}