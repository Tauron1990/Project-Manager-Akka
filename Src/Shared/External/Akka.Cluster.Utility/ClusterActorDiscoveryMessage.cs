using System.Collections.Generic;
using Akka.Actor;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public static class ClusterActorDiscoveryMessage
    {
        // Notify other ClusterActorDiscoveries that I'm up
        public class RegisterCluster
        {
            public RegisterCluster(UniqueAddress clusterAddress, List<ClusterActorUp> actorUpList = null)
            {
                ClusterAddress = clusterAddress;
                ActorUpList = actorUpList;
            }

            public UniqueAddress ClusterAddress { get; }
            public List<ClusterActorUp> ActorUpList { get; }
        }

        // Notify other ClusterActorDiscoveries that I'm up again
        public class ResyncCluster
        {
            public ResyncCluster(UniqueAddress clusterAddress, List<ClusterActorUp> actorUpList, bool request)
            {
                ClusterAddress = clusterAddress;
                ActorUpList = actorUpList;
                Request = request;
            }

            public UniqueAddress ClusterAddress { get; }
            public List<ClusterActorUp> ActorUpList { get; }
            public bool Request { get; }
        }

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
        public class ClusterActorUp
        {
            public ClusterActorUp(IActorRef actor, string tag)
            {
                Actor = actor;
                Tag = tag;
            }

            public IActorRef Actor { get; }
            public string Tag { get; }
        }

        // Notify other ClusterNodeActors that Actor in my cluster node is down
        public class ClusterActorDown
        {
            public ClusterActorDown(IActorRef actor) => Actor = actor;

            public IActorRef Actor { get; }
        }

        // Notify watcher that actor monitored is up
        public class ActorUp
        {
            public ActorUp(IActorRef actor, string tag)
            {
                Actor = actor;
                Tag = tag;
            }

            public IActorRef Actor { get; }
            public string Tag { get; }
        }

        // Notify watcher that actor monitored is down
        public class ActorDown
        {
            public ActorDown(IActorRef actor, string tag)
            {
                Actor = actor;
                Tag = tag;
            }

            public IActorRef Actor { get; }
            public string Tag { get; }
        }

        // Notify discovery actor that actor is up
        public class RegisterActor
        {
            public RegisterActor(IActorRef actor, string tag)
            {
                Actor = actor;
                Tag = tag;
            }

            public IActorRef Actor { get; }
            public string Tag { get; }
        }

        // Notify discovery actor that actor is down
        public class UnregisterActor
        {
            public UnregisterActor(IActorRef actor) => Actor = actor;

            public IActorRef Actor { get; }
        }

        // Monitors actors with specific tag up or down.
        public class MonitorActor
        {
            public MonitorActor(string tag) => Tag = tag;

            public string Tag { get; }
        }

        // Stops monitoring actors with specific tag up or down.
        public class UnmonitorActor
        {
            public UnmonitorActor(string tag) => Tag = tag;

            public string Tag { get; }
        }
    }
}