using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public sealed class ClusterActorDiscovery : IExtension
    {
        public ClusterActorDiscovery(ExtendedActorSystem system)
            => Discovery = system.ActorOf<ClusterActorDiscoveryActor>(nameof(ClusterActorDiscovery));

        public IActorRef Discovery { get; }

        public static ClusterActorDiscovery Get(ActorSystem system) => system.GetExtension<ClusterActorDiscovery>();

        public void MonitorActor(ClusterActorDiscoveryMessage.MonitorActor actor)
            => Discovery.Tell(actor);

        public void UnMonitorActor(ClusterActorDiscoveryMessage.UnmonitorActor actor)
            => Discovery.Tell(actor);

        public void RegisterActor(ClusterActorDiscoveryMessage.RegisterActor actor)
            => Discovery.Tell(actor);

        public void UnRegisterActor(ClusterActorDiscoveryMessage.UnmonitorActor actor)
            => Discovery.Tell(actor);
    }

    [PublicAPI]
    public sealed class ClusterActorDiscoveryId : ExtensionIdProvider<ClusterActorDiscovery>
    {
        public override ClusterActorDiscovery CreateExtension(ExtendedActorSystem system) => new(system);
    }

    [PublicAPI]
    public class ClusterActorDiscoveryActor : ReceiveActor
    {
        private readonly List<ActorItem> _actorItems = new();

        // Watching actors

        private readonly Dictionary<IActorRef, int[]> _actorWatchCountMap = new();
        private readonly Cluster _cluster;
        private readonly ILoggingAdapter _log;

        private readonly List<MonitorItem> _monitorItems = new();
        private readonly string _name;

        private readonly Dictionary<IActorRef, NodeItem> _nodeMap = new();

        public ClusterActorDiscoveryActor()
        {
            _cluster = Cluster.Get(Context.System);
            _name = Self.Path.Name;
            _log = Context.GetLogger();

            Receive<ClusterEvent.MemberUp>(Handle);
            Receive<ClusterEvent.ReachableMember>(Handle);
            Receive<ClusterEvent.UnreachableMember>(Handle);
            Receive<ClusterEvent.MemberRemoved>(Handle);
            Receive<ClusterActorDiscoveryMessage.RegisterCluster>(Handle);
            Receive<ClusterActorDiscoveryMessage.ResyncCluster>(Handle);
            // Receive<ClusterActorDiscoveryMessage.UnregisterCluster>(m => Handle(m));
            Receive<ClusterActorDiscoveryMessage.ClusterActorUp>(Handle);
            Receive<ClusterActorDiscoveryMessage.ClusterActorDown>(Handle);
            Receive<ClusterActorDiscoveryMessage.RegisterActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.UnregisterActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.MonitorActor>(Handle);
            Receive<ClusterActorDiscoveryMessage.UnmonitorActor>(Handle);
            Receive<Terminated>(Handle);
        }

        protected override void PreStart()
        {
            _cluster.Subscribe(
                Self,
                ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents,
                typeof(ClusterEvent.MemberUp),
                typeof(ClusterEvent.ReachableMember),
                typeof(ClusterEvent.UnreachableMember),
                typeof(ClusterEvent.MemberRemoved));
        }

        protected override void PostStop()
        {
            _cluster.Unsubscribe(Self);
        }

        private void Handle(ClusterEvent.MemberUp member)
        {
            if (_cluster.SelfUniqueAddress == member.Member.UniqueAddress)
            {
                var roles = string.Join(", ", _cluster.SelfRoles);
                _log.Info($"Cluster.Up: {_cluster.SelfUniqueAddress} Role={roles}");
            }
            else
            {
                var remoteDiscoveryActor = Context.ActorSelection(member.Member.Address + "/user/" + _name);
                remoteDiscoveryActor.Tell(
                    new ClusterActorDiscoveryMessage.RegisterCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                           .ToList()));
            }
        }

        private void Handle(ClusterEvent.ReachableMember member)
        {
            if (_cluster.SelfUniqueAddress == member.Member.UniqueAddress)
            {
                var roles = string.Join(", ", _cluster.SelfRoles);
                _log.Info($"Cluster.RechableMe: {_cluster.SelfUniqueAddress} Role={roles}");
            }
            else
            {
                _log.Info($"Cluster.Rechable: {member.Member.Address} Role={string.Join(",", member.Member.Roles)}");

                var remoteDiscoveryActor = Context.ActorSelection(member.Member.Address + "/user/" + _name);
                remoteDiscoveryActor.Tell(
                    new ClusterActorDiscoveryMessage.ResyncCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                           .ToList(),
                        Request: true));
            }
        }

        private void Handle(ClusterEvent.UnreachableMember member)
        {
            _log.Info($"Cluster.Unreachable: {member.Member.Address} Role={string.Join(",", member.Member.Roles)}");

            var (key, _) = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.Member.UniqueAddress);
            RemoveNode(key);
        }

        private void Handle(ClusterEvent.MemberRemoved member)
        {
            _log.Info($"Cluster.MemberRemoved: {member.Member.Address} Role={string.Join(",", member.Member.Roles)}");

            var (key, _) = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.Member.UniqueAddress);
            RemoveNode(key);
        }

        private void Handle(ClusterActorDiscoveryMessage.RegisterCluster member)
        {
            _log.Info($"RegisterCluster: {member.ClusterAddress}");

            // Register node

            // var item = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.ClusterAddress);
            //  if (item.Key != null)
            //  {
            //      _log.Error($"Already registered node. {member.ClusterAddress}");
            //      return;
            //  }

            _nodeMap.Add(Sender, new NodeItem(new List<ActorItem>(), member.ClusterAddress));

            // Process attached actorUp messages

            //if (member.ActorUpList is null) return;

            foreach (var actorUp in member.ActorUpList)
                Handle(actorUp);
        }

        private void Handle(ClusterActorDiscoveryMessage.ResyncCluster member)
        {
            _log.Info($"ResyncCluster: {member.ClusterAddress} Request={member.Request}");

            // Reregister node

            var (key, _) = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.ClusterAddress);
            //if (key != null)
            RemoveNode(key);

            _nodeMap.Add(Sender, new NodeItem(new List<ActorItem>(), member.ClusterAddress));

            // Process attached actorUp messages

            //if (member.ActorUpList != null)
            foreach (var actorUp in member.ActorUpList)
                Handle(actorUp);

            // Response

            if (member.Request)
                Sender.Tell(
                    new ClusterActorDiscoveryMessage.ResyncCluster(
                        _cluster.SelfUniqueAddress,
                        _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                           .ToList(),
                        Request: false));
        }

        //private void Handle(ClusterActorDiscoveryMessage.UnregisterCluster m)
        //{
        //    _log.Info($"UnregisterCluster: {m.ClusterAddress}");

        //    var item = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.ClusterAddress);
        //    if (item.Key != null)
        //        RemoveNode(item.Key);
        //}

        private void RemoveNode(IActorRef discoveryActor)
        {
            if (_nodeMap.TryGetValue(discoveryActor, out var node) == false)
                return;

            _nodeMap.Remove(discoveryActor);

            foreach (var actorItem in node.ActorItems)
                NotifyActorDownToMonitor(actorItem.Actor, actorItem.Tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.ClusterActorUp member)
        {
            _log.Debug($"ClusterActorUp: Actor={member.Actor.Path} Tag={member.Tag}");

            if (_nodeMap.TryGetValue(Sender, out var node) == false)
            {
                _log.Error($"Cannot find node: Discovery={Sender.Path}");

                return;
            }

            node.ActorItems.Add(new ActorItem(member.Actor, member.Tag));

            NotifyActorUpToMonitor(member.Actor, member.Tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.ClusterActorDown member)
        {
            _log.Debug($"ClusterActorDown: Actor={member.Actor.Path}");

            if (_nodeMap.TryGetValue(Sender, out var node) == false)
            {
                _log.Error($"Cannot find node: Discovery={Sender.Path}");

                return;
            }

            // remove actor from node.ActorItems

            var index = node.ActorItems.FindIndex(item => item.Actor.Equals(member.Actor));
            if (index == -1)
            {
                _log.Error($"Cannot find actor: Discovery={Sender.Path} Actor={member.Actor.Path}");

                return;
            }

            var tag = node.ActorItems[index].Tag;
            node.ActorItems.RemoveAt(index);

            NotifyActorDownToMonitor(member.Actor, tag);
        }

        private void Handle(ClusterActorDiscoveryMessage.RegisterActor member)
        {
            _log.Debug($"RegisterActor: Actor={member.Actor.Path} Tag={member.Tag}");

            // add actor to _actorItems

            var index = _actorItems.FindIndex(item => item.Actor.Equals(member.Actor));
            if (index != -1)
            {
                _log.Error($"Already registered actor: Actor={member.Actor.Path} Tag={member.Tag}");

                return;
            }

            _actorItems.Add(new ActorItem(member.Actor, member.Tag));
            WatchActor(member.Actor, 0);

            // tell monitors & other discovery actors that local actor up

            NotifyActorUpToMonitor(member.Actor, member.Tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorUp(member.Actor, member.Tag));
        }

        private void Handle(ClusterActorDiscoveryMessage.UnregisterActor member)
        {
            _log.Debug($"UnregisterActor: Actor={member.Actor.Path}");

            // remove actor from _actorItems

            var index = _actorItems.FindIndex(item => item.Actor.Equals(member.Actor));

            if (index == -1)
                return;

            var tag = _actorItems[index].Tag;
            _actorItems.RemoveAt(index);
            UnwatchActor(member.Actor, 0);

            // tell monitors & other discovery actors that local actor down

            NotifyActorDownToMonitor(member.Actor, tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(member.Actor));
        }

        private void Handle(ClusterActorDiscoveryMessage.MonitorActor member)
        {
            _log.Debug($"MonitorActor: Monitor={Sender.Path} Tag={member.Tag}");

            _monitorItems.Add(new MonitorItem(Sender, member.Tag));
            WatchActor(Sender, 1);

            // Send actor up message to just registered monitor

            foreach (var actor in _actorItems.Where(item => item.Tag == member.Tag))
                Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));

            foreach (var actor in _nodeMap.Values.SelectMany(node => node.ActorItems.Where(item => item.Tag == member.Tag)))
                Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));
        }

        private void Handle(ClusterActorDiscoveryMessage.UnmonitorActor member)
        {
            _log.Debug($"UnmonitorActor: Monitor={Sender.Path} Tag={member.Tag}");

            var count = _monitorItems.RemoveAll(item => item.Actor.Equals(Sender) && item.Tag == member.Tag);
            for (var index = 0; index < count; index++)
                UnwatchActor(Sender, 1);
        }

        private void Handle(Terminated member)
        {
            _log.Debug($"Terminated: Actor={member.ActorRef.Path}");

            if (_actorWatchCountMap.TryGetValue(member.ActorRef, out var counts) == false)
                return;

            if (counts[1] > 0)
            {
                _monitorItems.RemoveAll(item => item.Actor.Equals(Sender));
                counts[1] = 0;
            }

            if (counts[0] <= 0) return;

            var index = _actorItems.FindIndex(item => item.Actor.Equals(member.ActorRef));

            if (index == -1) return;

            var tag = _actorItems[index].Tag;
            _actorItems.RemoveAt(index);

            // tell monitors & other discovery actors that local actor down

            NotifyActorDownToMonitor(member.ActorRef, tag);
            foreach (var discoveryActor in _nodeMap.Keys)
                discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(member.ActorRef));
        }

        private void NotifyActorUpToMonitor(IActorRef actor, string tag)
        {
            foreach (var monitor in _monitorItems.Where(item => item.Tag == tag))
                monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor, tag));
        }

        private void NotifyActorDownToMonitor(IActorRef actor, string tag)
        {
            foreach (var monitor in _monitorItems.Where(item => item.Tag == tag))
                monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorDown(actor, tag));
        }

        private void WatchActor(IActorRef actor, int channel)
        {
            // every watched actor counter has 2 values identified by channel
            // - channel 0: source actor watching counter
            // - channel 1: monitor actor watching counter (to track monitoring actor destroyed)

            if (_actorWatchCountMap.TryGetValue(actor, out var counts))
            {
                counts[channel] += 1;

                return;
            }

            counts = new int[2];
            counts[channel] += 1;
            _actorWatchCountMap.Add(actor, counts);
            Context.Watch(actor);
        }

        private void UnwatchActor(IActorRef actor, int channel)
        {
            if (_actorWatchCountMap.TryGetValue(actor, out var counts) == false)
                return;

            counts[channel] -= 1;

            if (counts.Sum() > 0)
                return;

            _actorWatchCountMap.Remove(actor);
            Context.Unwatch(actor);
        }

        // Per cluster-node data

        private sealed record NodeItem(List<ActorItem> ActorItems, UniqueAddress ClusterAddress);

        // Actors in cluster

        private sealed record ActorItem(IActorRef Actor, string Tag);

        // Monitor items registered in this discovery actor

        private sealed record MonitorItem(IActorRef Actor, string Tag);
    }
}