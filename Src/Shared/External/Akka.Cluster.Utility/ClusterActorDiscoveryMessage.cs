using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public static class ClusterActorDiscoveryMessage
    {
        // Notify other ClusterActorDiscoveries that I'm up
        public sealed record RegisterCluster(UniqueAddress ClusterAddress, List<ClusterActorUp> ActorUpList);

        // Notify other ClusterActorDiscoveries that I'm up again
        #pragma warning disable AV1564
        public sealed record ResyncCluster(UniqueAddress ClusterAddress, List<ClusterActorUp> ActorUpList, bool Request);
        #pragma warning restore AV1564

        //// Notify other ClusterNodeActors that I'm down
        //public class UnregisterCluster
        //{
        //    public UniqueAddress ClusterAddress { get; }

        //    public UnregisterCluster(UniqueAddress clusterAddress)
        //    {
        //        ClusterAddress = clusterAddress;
        //    }
        //}

        // Notify other ClusterNodeActors that Actor in my cluster node is up
        public sealed record ClusterActorUp(IActorRef Actor, string Tag);

        // Notify other ClusterNodeActors that Actor in my cluster node is down
        public sealed record ClusterActorDown(IActorRef Actor);

        // Notify watcher that actor monitored is up
        public sealed record ActorUp(IActorRef Actor, string Tag);

        // Notify watcher that actor monitored is down
        public sealed record ActorDown(IActorRef Actor, string? Tag);

        // Notify discovery actor that actor is up
        public sealed record RegisterActor(IActorRef Actor, string Tag);

        // Notify discovery actor that actor is down
        public sealed record UnregisterActor(IActorRef Actor);

        // Monitors actors with specific tag up or down.
        public sealed record MonitorActor(string Tag);

        // Stops monitoring actors with specific tag up or down.
        public sealed record UnmonitorActor(string Tag);
    }
}