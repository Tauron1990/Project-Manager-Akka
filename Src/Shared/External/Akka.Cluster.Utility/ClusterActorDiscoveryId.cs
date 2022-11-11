using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility;

[PublicAPI]
public sealed class ClusterActorDiscoveryId : ExtensionIdProvider<ClusterActorDiscovery>
{
    public override ClusterActorDiscovery CreateExtension(ExtendedActorSystem system)
        => new(system);
}