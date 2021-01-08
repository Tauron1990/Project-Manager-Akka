﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public class DistributedActorTableContainer<TKey> : ReceiveActor
    {
        private readonly IActorFactory _actorFactory;
        private readonly Dictionary<IActorRef, TKey> _actorInverseMap = new();
        private readonly Dictionary<TKey, IActorRef> _actorMap = new();

        private readonly Dictionary<TKey, IActorRef> _addingMap = new();
        private readonly IActorRef _clusterActorDiscovery;
        private readonly object _downMessage;
        private readonly ILoggingAdapter _log;

        private readonly string _name;

        private bool _stopping;

        private IActorRef _table;
        private int _watchingActorCount;


        public DistributedActorTableContainer(string name, ActorSystem system, Type actorType)
            : this(name, ClusterActorDiscovery.Get(system).Discovery, actorType) { }

        public DistributedActorTableContainer(string name, IActorRef clusterActorDiscovery, Type actorType)
            : this(name, clusterActorDiscovery, typeof(SimpleActorFactory), new object[] {actorType}) { }

        public DistributedActorTableContainer(string name, IActorRef clusterActorDiscovery,
            Type actorFactoryType, object[] actorFactoryInitalizeArgs,
            object downMessage = null)
        {
            _name = name;
            _clusterActorDiscovery = clusterActorDiscovery;
            _downMessage = downMessage;
            _log = Context.GetLogger();

            if (actorFactoryType != null)
            {
                try
                {
                    _actorFactory = (IActorFactory) Activator.CreateInstance(actorFactoryType);
                    _actorFactory.Initialize(actorFactoryInitalizeArgs);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Exception in initializing ${actorFactoryType.FullName}");
                    _actorFactory = null;
                }
            }

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

        private void Handle(ClusterActorDiscoveryMessage.ActorUp m)
        {
            _log.Info($"Table.ActorUp (Actor={m.Actor.Path})");

            if (_table != null)
            {
                _log.Error($"But I already have table. (Actor={_table.Path})");
                return;
            }

            _table = m.Actor;
        }

        private void Handle(ClusterActorDiscoveryMessage.ActorDown m)
        {
            _log.Info($"Table.ActorDown (Actor={m.Actor.Path})");

            if (_table.Equals(m.Actor) == false)
            {
                _log.Error($"But I have a different table. (Actor={_table.Path})");
                return;
            }

            _table = null;

            CancelAllPendingAddRequests();

            foreach (var i in _actorMap) i.Value.Tell(_downMessage ?? PoisonPill.Instance);

            // NOTE: should we clear actor map or let them to be removed ?
        }

        private void Handle(DistributedActorTableMessage<TKey>.Add m)
        {
            if (_table == null || _stopping)
            {
                Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(m.Id, m.Actor, false));
                return;
            }

            if (m.Actor == null)
            {
                _log.Error($"Invalid null actor. (ID={m.Id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(m.Id, m.Actor, false));
                return;
            }

            if (_actorMap.ContainsKey(m.Id))
            {
                _log.Error($"Duplicate ID in local container. (ID={m.Id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.AddReply(m.Id, m.Actor, false));
                return;
            }

            _actorMap.Add(m.Id, m.Actor);
            _actorInverseMap.Add(m.Actor, m.Id);
            _addingMap.Add(m.Id, Sender);
            Context.Watch(m.Actor);
            _watchingActorCount += 1;

            _table.Tell(new DistributedActorTableMessage<TKey>.Internal.Add(m.Id, m.Actor));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Remove m)
        {
            if (_actorMap.TryGetValue(m.Id, out var actor) == false)
            {
                _log.Error($"Cannot remove an actor that doesn't exist. (Id={m.Id} Sender={Sender})");
                return;
            }

            _actorMap.Remove(m.Id);
            _actorInverseMap.Remove(actor);
            Context.Unwatch(actor);
            _watchingActorCount -= 1;

            _table?.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(m.Id));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.Create m)
        {
            if (_table == null || _stopping)
                return;

            if (_actorFactory == null)
            {
                _log.Error("I don't have ActorFactory.");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(m.Id, null));
                return;
            }

            IActorRef actor;
            try
            {
                actor = _actorFactory.CreateActor(Context, m.Id, m.Args);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Exception in creating actor (Id={m.Id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(m.Id, null));
                return;
            }

            _actorMap.Add(m.Id, actor);
            _actorInverseMap.Add(actor, m.Id);
            Context.Watch(actor);
            _watchingActorCount += 1;

            Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.CreateReply(m.Id, actor));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.AddReply m)
        {
            if (_addingMap.TryGetValue(m.Id, out var requester) == false)
                // already removed locally
                return;

            _addingMap.Remove(m.Id);

            if (m.Added)
                requester.Tell(new DistributedActorTableMessage<TKey>.AddReply(m.Id, m.Actor, true));
            else
            {
                _actorMap.Remove(m.Id);
                _actorInverseMap.Remove(m.Actor);
                Context.Unwatch(m.Actor);
                _watchingActorCount -= 1;

                requester.Tell(new DistributedActorTableMessage<TKey>.AddReply(m.Id, m.Actor, false));
            }
        }

        private void CancelAllPendingAddRequests()
        {
            foreach (var i in _addingMap)
            {
                _actorMap.Remove(i.Key);
                _actorInverseMap.Remove(i.Value);
                Context.Unwatch(i.Value);
                _watchingActorCount -= 1;

                i.Value.Tell(new DistributedActorTableMessage<TKey>.AddReply(i.Key, i.Value, false));
            }

            _addingMap.Clear();
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.GracefulStop m)
        {
            if (_stopping)
                return;

            _stopping = true;

            CancelAllPendingAddRequests();

            if (_actorMap.Count > 0)
            {
                foreach (var i in _actorMap)
                    i.Value.Tell(m.StopMessage ?? PoisonPill.Instance);
            }
            else
                Context.Stop(Self);
        }

        private void Handle(Terminated m)
        {
            if (_actorInverseMap.TryGetValue(m.ActorRef, out var id) == false)
                return;

            _actorMap.Remove(id);
            _actorInverseMap.Remove(m.ActorRef);
            _watchingActorCount -= 1;

            if (_stopping)
            {
                if (_watchingActorCount == 0)
                    Context.Stop(Self);
            }
            else
                _table?.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(id));
        }

        private sealed class SimpleActorFactory : IActorFactory
        {
            private object _actor;

            public void Initialize(object[] args)
            {
                _actor = args[0];
            }

            public IActorRef CreateActor(IActorRefFactory actorRefFactory, object id, object[] args) => actorRefFactory.ActorOf(GetProps(args), id.ToString());

            private Props GetProps(object[] args)
            {
                return _actor switch
                       {
                           Props props => props,
                           Type type   => Props.Create(type, args),
                           _           => null
                       };
            }
        }
    }
}