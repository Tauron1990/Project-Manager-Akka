using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility;

[PublicAPI]
public sealed class ClusterActorDiscovery : IExtension
{
    public ClusterActorDiscovery(ExtendedActorSystem system)
    {
        Discovery = system.ActorOf(
            Props.Create(
                () => new ClusterActorDiscoveryActor()),
            nameof(ClusterActorDiscovery));
    }

    public IActorRef Discovery { get; }

    public static ClusterActorDiscovery Get(ActorSystem system)
        => system.GetExtension<ClusterActorDiscovery?>() ?? new ClusterActorDiscoveryId().Apply(system);

    public void MonitorActor(ClusterActorDiscoveryMessage.MonitorActor actor)
        => Discovery.Tell(actor);

    public void UnMonitorActor(ClusterActorDiscoveryMessage.UnmonitorActor actor)
        => Discovery.Tell(actor);

    public void RegisterActor(ClusterActorDiscoveryMessage.RegisterActor actor)
        => Discovery.Tell(actor);

    public void UnRegisterActor(ClusterActorDiscoveryMessage.UnmonitorActor actor)
        => Discovery.Tell(actor);
}