using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Application;

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

[PublicAPI]
public sealed class ClusterActorDiscoveryId : ExtensionIdProvider<ClusterActorDiscovery>
{
    public override ClusterActorDiscovery CreateExtension(ExtendedActorSystem system)
        => new(system);
}

[PublicAPI]
public partial class ClusterActorDiscoveryActor : ReceiveActor
{
    private readonly List<ActorItem> _actorItems = new();

    // Watching actors

    private readonly Dictionary<IActorRef, int[]> _actorWatchCountMap = new();
    private readonly Cluster _cluster;
    private readonly ILogger _logger;

    private readonly List<MonitorItem> _monitorItems = new();
    private readonly string _name;

    private readonly Dictionary<IActorRef, NodeItem> _nodeMap = new();

    public ClusterActorDiscoveryActor()
    {
        _cluster = Cluster.Get(Context.System);
        _name = Self.Path.Name;
        _logger = TauronEnviroment.GetLogger<ClusterActorDiscoveryActor>();

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
        => _cluster.Unsubscribe(Self);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Cluster.Up: {selfAdress} Role={roles}")]
    private partial void SelfMemberUp(UniqueAddress selfAdress, string? roles);

    private void Handle(ClusterEvent.MemberUp member)
    {
        if(_cluster.SelfUniqueAddress == member.Member.UniqueAddress)
        {
            string roles = string.Join(", ", _cluster.SelfRoles);
            SelfMemberUp(_cluster.SelfUniqueAddress, roles);
        }
        else
        {
            ActorSelection? remoteDiscoveryActor = Context.ActorSelection(member.Member.Address + "/user/" + _name);
            remoteDiscoveryActor.Tell(
                new ClusterActorDiscoveryMessage.RegisterCluster(
                    _cluster.SelfUniqueAddress,
                    _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                       .ToList()));
        }
    }

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Cluster.ReachableMe: {selfAddress}, Role={roles}")]
    private partial void SelfMemberReachable(UniqueAddress selfAddress, string roles);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Cluster.Reachable: {address}, Role={Roles}")]
    private partial void MemberReachable(Address address, string roles);

    private void Handle(ClusterEvent.ReachableMember member)
    {
        if(_cluster.SelfUniqueAddress == member.Member.UniqueAddress)
        {
            string roles = string.Join(", ", _cluster.SelfRoles);
            SelfMemberReachable(_cluster.SelfUniqueAddress, roles);
        }
        else
        {
            MemberReachable(member.Member.Address, string.Join(",", member.Member.Roles));

            ActorSelection? remoteDiscoveryActor = Context.ActorSelection(member.Member.Address + "/user/" + _name);
            remoteDiscoveryActor.Tell(
                new ClusterActorDiscoveryMessage.ResyncCluster(
                    _cluster.SelfUniqueAddress,
                    _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                       .ToList(),
                    Request: true));
        }
    }

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Cluster.Unreachable: {address}, Role={role}")]
    private partial void MemberUnreachable(Address address, string role);

    private void Handle(ClusterEvent.UnreachableMember member)
    {
        MemberUnreachable(member.Member.Address, string.Join(",", member.Member.Roles));

        #pragma warning disable GU0019
        IActorRef? key = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.Member.UniqueAddress).Key;
        #pragma warning restore GU0019

        RemoveNode(key);
    }

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Cluster.MemberRemoved: {address}, Role={role}")]
    private partial void MemberRemoved(Address address, string role);

    private void Handle(ClusterEvent.MemberRemoved member)
    {
        MemberRemoved(member.Member.Address, string.Join(",", member.Member.Roles));

        #pragma warning disable GU0019
        IActorRef? key = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.Member.UniqueAddress).Key;
        #pragma warning restore GU0019
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if(key is null) return;

        RemoveNode(key);
    }

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "RegisterCluster: {clusterAddress}")]
    private partial void RegisterCluster(UniqueAddress clusterAddress);


    private void Handle(ClusterActorDiscoveryMessage.RegisterCluster member)
    {
        RegisterCluster(member.ClusterAddress);

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

        foreach (ClusterActorDiscoveryMessage.ClusterActorUp actorUp in member.ActorUpList)
            Handle(actorUp);
    }

    [LoggerMessage(EventId = 42, Level = LogLevel.Information, Message = "ResyncCluster: {clusterAddress}, Request={request}")]
    private partial void ResyncCluster(UniqueAddress clusterAddress, bool request);

    private void Handle(ClusterActorDiscoveryMessage.ResyncCluster member)
    {
        ResyncCluster(member.ClusterAddress, member.Request);

        // Reregister node

        #pragma warning disable GU0019
        IActorRef? key = _nodeMap.FirstOrDefault(node => node.Value.ClusterAddress == member.ClusterAddress).Key;
        #pragma warning restore GU0019
        if(key != null)
            RemoveNode(key);

        _nodeMap.Add(Sender, new NodeItem(new List<ActorItem>(), member.ClusterAddress));

        // Process attached actorUp messages

        //if (member.ActorUpList != null)
        foreach (ClusterActorDiscoveryMessage.ClusterActorUp actorUp in member.ActorUpList)
            Handle(actorUp);

        // Response

        if(member.Request)
            Sender.Tell(
                new ClusterActorDiscoveryMessage.ResyncCluster(
                    _cluster.SelfUniqueAddress,
                    _actorItems.Select(item => new ClusterActorDiscoveryMessage.ClusterActorUp(item.Actor, item.Tag))
                       .ToList(),
                    false));
    }

    //private void Handle(ClusterActorDiscoveryMessage.UnregisterCluster m)
    //{
    //    _log.Info($"UnregisterCluster: {m.ClusterAddress}");

    //    var item = _nodeMap.FirstOrDefault(i => i.Value.ClusterAddress == m.ClusterAddress);
    //    if (item.Key != null)
    //        RemoveNode(item.Key);
    //}

    private void RemoveNode(IActorRef? discoveryActor)
    {
        if(discoveryActor is null) return;

        if(_nodeMap.TryGetValue(discoveryActor, out NodeItem? node) == false)
            return;

        _nodeMap.Remove(discoveryActor);

        foreach (ActorItem actorItem in node.ActorItems)
            NotifyActorDownToMonitor(actorItem.Actor, actorItem.Tag);
    }

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "ClusterActor{type}: Actor={path}, Tag={tag}")]
    private partial void ClusterActorEvent(string type, ActorPath path, string tag);

    [LoggerMessage(EventId = 14, Level = LogLevel.Error, Message = "Cannot find node: Discovery={path}")]
    private partial void ClusterActornodeNotFound(ActorPath path);

    private void Handle(ClusterActorDiscoveryMessage.ClusterActorUp member)
    {
        ClusterActorEvent("Up", member.Actor.Path, member.Tag);

        if(_nodeMap.TryGetValue(Sender, out NodeItem? node) == false)
        {
            ClusterActornodeNotFound(Sender.Path);

            return;
        }

        node.ActorItems.Add(new ActorItem(member.Actor, member.Tag));

        NotifyActorUpToMonitor(member.Actor, member.Tag);
    }

    [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "Cannot fing Actor: Discovery={path}, Actor={actorPath}")]
    private partial void ActorNotFound(ActorPath path, ActorPath actorPath);

    private void Handle(ClusterActorDiscoveryMessage.ClusterActorDown member)
    {
        ClusterActorEvent("Down", member.Actor.Path, string.Empty);

        if(_nodeMap.TryGetValue(Sender, out NodeItem? node) == false)
        {
            ClusterActornodeNotFound(Sender.Path);

            return;
        }

        // remove actor from node.ActorItems

        int index = node.ActorItems.FindIndex(item => item.Actor.Equals(member.Actor));
        if(index == -1)
        {
            ActorNotFound(Sender.Path, member.Actor.Path);

            return;
        }

        string tag = node.ActorItems[index].Tag;
        node.ActorItems.RemoveAt(index);

        NotifyActorDownToMonitor(member.Actor, tag);
    }

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "RegisterActor: Actor={path}, Tag={tag}")]
    private partial void ActorRegister(ActorPath path, string tag);

    [LoggerMessage(EventId = 17, Level = LogLevel.Error, Message = "Already registred actor: Actor={path}, Tag={tag}")]
    private partial void AlreadyRegistredActor(ActorPath path, string tag);

    private void Handle(ClusterActorDiscoveryMessage.RegisterActor member)
    {
        ActorRegister(member.Actor.Path, member.Tag);

        // add actor to _actorItems

        int index = _actorItems.FindIndex(item => item.Actor.Equals(member.Actor));
        if(index != -1)
        {
            AlreadyRegistredActor(member.Actor.Path, member.Tag);

            return;
        }

        _actorItems.Add(new ActorItem(member.Actor, member.Tag));
        WatchActor(member.Actor, 0);

        // tell monitors & other discovery actors that local actor up

        NotifyActorUpToMonitor(member.Actor, member.Tag);
        foreach (IActorRef discoveryActor in _nodeMap.Keys)
            discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorUp(member.Actor, member.Tag));
    }

    [LoggerMessage(EventId = 18, Level = LogLevel.Debug, Message = "UnregisterActor: Actor={path}")]
    private partial void UnregisterActor(ActorPath path);

    private void Handle(ClusterActorDiscoveryMessage.UnregisterActor member)
    {
        UnregisterActor(member.Actor.Path);

        // remove actor from _actorItems

        int index = _actorItems.FindIndex(item => item.Actor.Equals(member.Actor));

        if(index == -1)
            return;

        string tag = _actorItems[index].Tag;
        _actorItems.RemoveAt(index);
        UnwatchActor(member.Actor, 0);

        // tell monitors & other discovery actors that local actor down

        NotifyActorDownToMonitor(member.Actor, tag);
        foreach (IActorRef discoveryActor in _nodeMap.Keys)
            discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(member.Actor));
    }

    [LoggerMessage(EventId = 19, Level = LogLevel.Debug, Message = "MonitorActor: Monitor={path}, Tag={tag}")]
    private partial void MonitorActor(ActorPath path, string tag);

    private void Handle(ClusterActorDiscoveryMessage.MonitorActor member)
    {
        MonitorActor(Sender.Path, member.Tag);

        _monitorItems.Add(new MonitorItem(Sender, member.Tag));
        WatchActor(Sender, 1);

        // Send actor up message to just registered monitor

        foreach (ActorItem actor in _actorItems.Where(item => item.Tag == member.Tag))
            Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));

        foreach (ActorItem actor in _nodeMap.Values.SelectMany(node => node.ActorItems.Where(item => item.Tag == member.Tag)))
            Sender.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor.Actor, actor.Tag));
    }

    [LoggerMessage(EventId = 20, Level = LogLevel.Debug, Message = "UnmonitorActor: Monitor={path}, Tag={tag}")]
    private partial void UnmonitorActor(ActorPath path, string tag);

    private void Handle(ClusterActorDiscoveryMessage.UnmonitorActor member)
    {
        UnmonitorActor(Sender.Path, member.Tag);

        int count = _monitorItems.RemoveAll(item => item.Actor.Equals(Sender) && item.Tag == member.Tag);
        for (var index = 0; index < count; index++)
            UnwatchActor(Sender, 1);
    }

    [LoggerMessage(EventId = 41, Level = LogLevel.Debug, Message = "Terminated: Actor={path}")]
    private partial void ActorTerminated(ActorPath path);

    private void Handle(Terminated member)
    {
        ActorTerminated(member.ActorRef.Path);

        if(_actorWatchCountMap.TryGetValue(member.ActorRef, out int[]? counts) == false)
            return;

        if(counts[1] > 0)
        {
            _monitorItems.RemoveAll(item => item.Actor.Equals(Sender));
            counts[1] = 0;
        }

        if(counts[0] <= 0) return;

        int index = _actorItems.FindIndex(item => item.Actor.Equals(member.ActorRef));

        if(index == -1) return;

        string tag = _actorItems[index].Tag;
        _actorItems.RemoveAt(index);

        // tell monitors & other discovery actors that local actor down

        NotifyActorDownToMonitor(member.ActorRef, tag);
        foreach (IActorRef discoveryActor in _nodeMap.Keys)
            discoveryActor.Tell(new ClusterActorDiscoveryMessage.ClusterActorDown(member.ActorRef));
    }

    private void NotifyActorUpToMonitor(IActorRef actor, string tag)
    {
        foreach (MonitorItem monitor in _monitorItems.Where(item => item.Tag == tag))
            monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorUp(actor, tag));
    }

    private void NotifyActorDownToMonitor(IActorRef actor, string tag)
    {
        foreach (MonitorItem monitor in _monitorItems.Where(item => item.Tag == tag))
            monitor.Actor.Tell(new ClusterActorDiscoveryMessage.ActorDown(actor, tag));
    }

    private void WatchActor(IActorRef actor, int channel)
    {
        // every watched actor counter has 2 values identified by channel
        // - channel 0: source actor watching counter
        // - channel 1: monitor actor watching counter (to track monitoring actor destroyed)

        if(_actorWatchCountMap.TryGetValue(actor, out int[]? counts))
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
        if(_actorWatchCountMap.TryGetValue(actor, out int[]? counts) == false)
            return;

        counts[channel] -= 1;

        if(counts.Sum() > 0)
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