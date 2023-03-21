using System.Collections.Immutable;
using Akka.Actor;
using Akka.Cluster.Utility;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using Tauron;
using Tauron.Features;

using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;


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
        
        ReceiveState<ActorUp>(ActorUp);
        ReceiveState<ActorDown>(ActorDown);
        
        Receive<DeviceServerOffline>(_ => {});
        Receive<DeviceServerOnline>(_ => { });

        Receive<object>(msg => CurrentState.Manager.Foreach(actor => actor.Forward(msg)));

        Start.Subscribe(StartActor);
        Stop.Subscribe(StopActor);
    }
    
    private static State ActorDown(StatePair<ActorDown, State> evt)
    {
        (ActorDown down, State state) = evt;
        
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

    private static State ActorUp(StatePair<ActorUp, State> evt)
    {
        (ActorUp up, State state) = evt;

        State newState = state with
        {
            ActorCount = state.ActorCount + 1,
            Manager = state.Manager.Add(up.Actor),
        };

        if(newState.ActorCount == 1)
            newState.Supervisor.Tell(new DeviceServerOnline());

        return newState;
    }

    private void StartActor(IActorContext context)
    {
        CurrentState.Supervisor.Tell(new DeviceServerOffline());
        ClusterActorDiscovery.Get(context.System).MonitorActor(new MonitorActor(DeviceManagerMessages.DeviceDataId));
    }


    private static void StopActor(IActorContext context)
        => ClusterActorDiscovery.Get(context.System).UnMonitorActor(new UnmonitorActor(DeviceManagerMessages.DeviceDataId));
    
}