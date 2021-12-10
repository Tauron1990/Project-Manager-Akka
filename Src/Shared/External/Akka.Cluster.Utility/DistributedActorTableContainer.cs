using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility;

[PublicAPI]
public class DistributedActorTableContainer<TKey> : ReceiveActor
    where TKey : notnull
{
    private readonly IActorFactory _actorFactory;
    private readonly Dictionary<IActorRef, TKey> _actorInverseMap = new();
    private readonly Dictionary<TKey, IActorRef> _actorMap = new();

    private readonly Dictionary<TKey, IActorRef> _addingMap = new();
    private readonly IActorRef _clusterActorDiscovery;
    private readonly object? _downMessage;
    private readonly ILoggingAdapter _log;

    private readonly string _name;

    private bool _stopping;

    private IActorRef? _table;
    private int _watchingActorCount;


    public DistributedActorTableContainer(string name, ActorSystem system, Type actorType)
        : this(name, ClusterActorDiscovery.Get(system).Discovery, actorType) { }

    public DistributedActorTableContainer(string name, IActorRef clusterActorDiscovery, Type actorType)
        : this(name, clusterActorDiscovery, typeof(SimpleActorFactory), new object[] { actorType }) { }

    public DistributedActorTableContainer(
        string name, IActorRef clusterActorDiscovery,
        Type actorFactoryType, object[] actorFactoryInitalizeArgs,
        object? downMessage = null)
    {
        _name = name;
        _clusterActorDiscovery = clusterActorDiscovery;
        _downMessage = downMessage;
        _log = Context.GetLogger();

        _actorFactory = (IActorFactory)Activator.CreateInstance(actorFactoryType)!;
        _actorFactory.Initialize(actorFactoryInitalizeArgs);

        Receive<ClusterActorDiscoveryMessage.ActorUp>(Handle);
        Receive<ClusterActorDiscoveryMessage.ActorDown>(Handle);

        Receive<DistributedActorTableMessage<TKey>.Add>(Handle);
        Receive<DistributedActorTableMessage<TKey>.Remove>(Handle);

        Receive<DistributedActorTableMessage<TKey>.Internal.Create>(Handle);
        Receive<DistributedActorTableMessage<TKey>.Internal.AddReply>(Handle);
        Receive<DistributedActorTableMessage<TKey>.Internal.GracefulStop>(Handle);

        Receive<Terminated>(Handle);
    }

    protected override void PreStart()
    {
        _log.Info($"DistributedActorTableContainer({_name}) Start");

        _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, _name + "Container"));
        _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(_name));
    }

    private void Handle(ClusterActorDiscoveryMessage.ActorUp actorUp)
    {
        var (actorRef, _) = actorUp;
        _log.Info($"Table.ActorUp (Actor={actorRef.Path})");

        if (_table != null)
        {
            _log.Error($"But I already have table. (Actor={_table.Path})");

            return;
        }

        _table = actorRef;
    }

    private void Handle(ClusterActorDiscoveryMessage.ActorDown actorDown)
    {
        var (actorRef, _) = actorDown;
        _log.Info($"Table.ActorDown (Actor={actorRef.Path})");

        if (_table != null && _table.Equals(actorRef) == false)
        {
            _log.Error($"But I have a different table. (Actor={_table.Path})");

            return;
        }

        _table = null;

        CancelAllPendingAddRequests();

        foreach (var (_, actor) in _actorMap) actor.Tell(_downMessage ?? PoisonPill.Instance);

        // NOTE: should we clear actor map or let them to be removed ?
    }

    private void Handle(DistributedActorTableMessage<TKey>.Add add)
    {
        var (id, actor) = add;
        if (_table is null || _stopping)
        {
            Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: false));

            return;
        }

        if (actor is null)
        {
            _log.Error($"Invalid null actor. (ID={id})");
            Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: false));

            return;
        }

        if (_actorMap.ContainsKey(id))
        {
            _log.Error($"Duplicate ID in local container. (ID={id})");
            Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: false));

            return;
        }

        _actorMap.Add(id, actor);
        _actorInverseMap.Add(actor, id);
        _addingMap.Add(id, Sender);
        Context.Watch(actor);
        _watchingActorCount += 1;

        _table.Tell(new DistributedActorTableMessage<TKey>.Internal.Add(id, actor));
    }

    private void Handle(DistributedActorTableMessage<TKey>.Remove remove)
    {
        if (_actorMap.TryGetValue(remove.Id, out var actor) == false)
        {
            _log.Error($"Cannot remove an actor that doesn't exist. (Id={remove.Id} Sender={Sender})");

            return;
        }

        _actorMap.Remove(remove.Id);
        _actorInverseMap.Remove(actor);
        Context.Unwatch(actor);
        _watchingActorCount -= 1;

        _table?.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(remove.Id));
    }

    private void Handle(DistributedActorTableMessage<TKey>.Internal.Create create)
    {
        if (_table is null || _stopping)
            return;

        // if (_actorFactory == null)
        // {
        //     _log.Error("I don't have ActorFactory.");
        //     Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(m.Id, null));
        //     return;
        // }

        IActorRef actor;
        var (id, args) = create;
        try
        {
            actor = _actorFactory.CreateActor(Context, id, args);
        }
        catch (Exception exception)
        {
            _log.Error(exception, $"Exception in creating actor (Id={id})");
            Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(id, null));

            return;
        }

        _actorMap.Add(id, actor);
        _actorInverseMap.Add(actor, id);
        Context.Watch(actor);
        _watchingActorCount += 1;

        Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(id, actor));
    }

    private void Handle(DistributedActorTableMessage<TKey>.Internal.AddReply addReply)
    {
        var (id, actor, added) = addReply;

        if (_addingMap.TryGetValue(id, out var requester) == false)
            // already removed locally
            return;

        _addingMap.Remove(id);

        if (added)
        {
            requester.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: true));
        }
        else
        {
            _actorMap.Remove(id);
            if (actor is not null)
                _actorInverseMap.Remove(actor);
            Context.Unwatch(actor);
            _watchingActorCount -= 1;

            requester.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: false));
        }
    }

    private void CancelAllPendingAddRequests()
    {
        foreach (var (id, actor) in _addingMap)
        {
            _actorMap.Remove(id);
            _actorInverseMap.Remove(actor);
            Context.Unwatch(actor);
            _watchingActorCount -= 1;

            actor.Tell(new DistributedActorTableMessage<TKey>.AddReply(id, actor, Added: false));
        }

        _addingMap.Clear();
    }

    private void Handle(DistributedActorTableMessage<TKey>.Internal.GracefulStop gracefulStop)
    {
        if (_stopping)
            return;

        _stopping = true;

        CancelAllPendingAddRequests();

        if (_actorMap.Count > 0)
            foreach (var (_, actor) in _actorMap)
                actor.Tell(gracefulStop.StopMessage ?? PoisonPill.Instance);
        else
            Context.Stop(Self);
    }

    private void Handle(Terminated terminated)
    {
        if (_actorInverseMap.TryGetValue(terminated.ActorRef, out var id) == false)
            return;

        _actorMap.Remove(id);
        _actorInverseMap.Remove(terminated.ActorRef);
        _watchingActorCount -= 1;

        if (_stopping)
        {
            if (_watchingActorCount == 0)
                Context.Stop(Self);
        }
        else
        {
            _table?.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(id));
        }
    }

    private sealed class SimpleActorFactory : IActorFactory
    {
        private object? _actor;

        public void Initialize(object[]? args)
        {
            if (args != null)
                _actor = args[0];
        }

        public IActorRef CreateActor(IActorRefFactory actorRefFactory, object id, object[]? args)
            => actorRefFactory.ActorOf(GetProps(args), id.ToString());

        private Props? GetProps(object[]? args)
        {
            return _actor switch
            {
                Props props => props,
                Type type => Props.Create(type, args),
                _ => null
            };
        }
    }
}