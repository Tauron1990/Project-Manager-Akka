using Akka.Actor;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Client.Operations.Shared.Devices;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Device.Core;
using Tauron;
using Tauron.Features;
using Tauron.TAkka;

namespace SimpleProjectManager.Operation.Client.Device;

public class DeviceSuperviser : ActorFeatureBase<EmptyState>
{

    private DeviceSuperviser(IMachine machine,  OperationConfiguration configuration, ILoggerFactory loggerFactory)
    {
        IActorRef clusterManager = Context.ActorOf("ClusterManager", ClusterManagerActor.New());
        IActorRef server = Context.ActorOf("ServerDeviceManager", ServerDiviceManagerActor.New(Self));
        Context.ActorOf(() => new MachineManagerActor(server, machine, configuration, loggerFactory), "MachineManager");

        Receive<DeviceInformations>(info => clusterManager.Forward(info));
        Receive<DeviceServerOffline>(msg => Context.GetChildren().Foreach(actor => actor.Forward(msg)));
        Receive<DeviceServerOnline>(msg => Context.GetChildren()
                                       .Foreach(actor => actor.Forward(msg)));
    }

    protected override void ConfigImpl()
    {
        
    }
}