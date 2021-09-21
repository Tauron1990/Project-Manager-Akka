using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using JetBrains.Annotations;

namespace Akka.Cluster.Utility
{
    [PublicAPI]
    public class DistributedActorTable<TKey> : ReceiveActor
        where TKey : notnull
    {
        private readonly Dictionary<TKey, IActorRef?> _actorMap = new();
        private readonly IActorRef _clusterActorDiscovery;

        private readonly Dictionary<IActorRef, Container> _containerMap = new();
        private readonly TimeSpan _createTimeout = TimeSpan.FromSeconds(10);

        private readonly Dictionary<TKey, Creating?> _creatingMap = new();
        private readonly IIdGenerator<TKey>? _idGenerator;
        private readonly ILoggingAdapter _log;
        private readonly string _name;
        private readonly bool _underTestEnvironment;
        private List<IActorRef>? _containerWorkQueue;
        private int _lastWorkNodeIndex = -1;
        private bool _queuedCreatingExists;

        private bool _stopping;

        public DistributedActorTable(string name, ActorSystem system)
            : this(name, ClusterActorDiscovery.Get(system).Discovery) { }

        public DistributedActorTable(string name, IActorRef clusterActorDiscovery)
            : this(name, clusterActorDiscovery, typeof(IncrementalIntegerIdGenerator), Array.Empty<object>()) { }

        protected override void PreStart()
        {
            _log.Info($"DistributedActorTable({_name}) Start");

            _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, _name));
            _clusterActorDiscovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(_name + "Container"));

            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10),
                Self,
                new CreateTimeoutMessage(),
                Self);
        }

        private void Handle(ClusterActorDiscoveryMessage.ActorUp member)
        {
            var (actorRef, _) = member;
            _log.Info($"Container.ActorUp (Actor={actorRef.Path})");

            if (_stopping)
            {
                _log.Info($"Ignore ActorUp while stopping. (Actor={actorRef.Path})");

                return;
            }

            if (_containerMap.ContainsKey(actorRef))
            {
                _log.Error($"I already have that container. (Actor={actorRef.Path})");

                return;
            }

            var container = new Container();
            _containerMap.Add(actorRef, container);
            RebuildContainerWorkQueue();

            if (_queuedCreatingExists)
            {
                foreach (var (key, creating) in _creatingMap.Where(item => item.Value is { WorkingContainer: null }))
                {
                    creating!.WorkingContainer = actorRef;
                    container.ActorMap.Add(key, null);
                    actorRef.Tell(new DistributedActorTableMessage<TKey>.Internal.Create(key, creating.Arguments));
                }

                _queuedCreatingExists = false;
            }

            if (_underTestEnvironment)
                Context.Watch(actorRef);
        }

        private void Handle(ClusterActorDiscoveryMessage.ActorDown member)
        {
            var (actor, _) = member;
            _log.Info($"Container.ActorDown (Actor={actor.Path})");

            if (_containerMap.TryGetValue(actor, out var container) == false)
            {
                _log.Error($"I don't have that container. (Actor={actor.Path})");

                return;
            }

            _containerMap.Remove(actor);
            RebuildContainerWorkQueue();

            // Remove all actors owned by this container

            foreach (var (key, actorRef) in container.ActorMap)
            {
                _actorMap.Remove(key);

                if (actorRef != null) continue;

                // cancel all pending creating requests

                if (!_creatingMap.TryGetValue(key, out var creating) || creating is null) continue;

                _creatingMap.Remove(key);

                foreach (var (targetActor, type) in creating.Requesters)
                    targetActor.Tell(CreateReplyMessage(type, key, null, created: false));
            }

            // When stopping done, ingest poison pill

            if (_stopping && _containerMap.Count == 0)
                Context.Stop(Self);
        }

        private void Handle(Terminated member)
        {
            // This function should be used under test environment only.

            Handle(new ClusterActorDiscoveryMessage.ActorDown(member.ActorRef, null));
        }

        private IActorRef? DecideWorkingContainer()
        {
            // round-robind

            if (_containerWorkQueue == null || _containerWorkQueue.Count == 0)
                return null;

            var index = _lastWorkNodeIndex + 1;
            if (index >= _containerWorkQueue.Count)
                index = 0;
            _lastWorkNodeIndex = index;

            return _containerWorkQueue[index];
        }

        private void RebuildContainerWorkQueue()
            => _containerWorkQueue = _containerMap.Keys.ToList();

        private void CreateActor(RequestType requestType, TKey id, object[] args)
        {
            var container = DecideWorkingContainer();

            // add actor with creating status

            _actorMap.Add(id, null);

            _creatingMap.Add(
                id,
                new Creating
                {
                    Arguments = args,
                    RequestTime = DateTime.UtcNow,
                    WorkingContainer = container
                });

            // send "create actor" request to container or enqueue it to pending list

            if (container != null)
            {
                _containerMap[container].ActorMap.Add(id, null);
                container.Tell(new DistributedActorTableMessage<TKey>.Internal.Create(id, args));
            }
            else
            {
                _queuedCreatingExists = true;
            }
        }

        #pragma warning disable AV1561
        private static object CreateReplyMessage(RequestType requestType, TKey id, IActorRef? actor, bool created)
            #pragma warning restore AV1561
        {
            return requestType switch
            {
                RequestType.Create => new DistributedActorTableMessage<TKey>.CreateReply(id, actor),
                RequestType.GetOrCreate => new DistributedActorTableMessage<TKey>.GetOrCreateReply(id, actor, created),
                RequestType.Get => new DistributedActorTableMessage<TKey>.GetReply(id, actor),
                _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null)
            };
        }

        private void PutOnCreateWaitingList(RequestType requestType, TKey id, IActorRef requester)
        {
            var (_, creating) = _creatingMap.FirstOrDefault(map => map.Key.Equals(id));
            if (creating == null)
            {
                _log.Error($"Cannot find creatingMap. (Id=${id} RequestType={requestType})");
                Sender.Tell(CreateReplyMessage(requestType, id, null, created: false));

                return;
            }

            creating.Requesters.Add(Tuple.Create(requester, requestType));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Create tableMsg)
        {
            if (_stopping)
                return;

            // decide ID (if provided, use it, otherwise generate new one)

            TKey id;
            var (key, args) = tableMsg;
            if (key != null && key.Equals(default(TKey)) == false)
            {
                if (_actorMap.ContainsKey(key))
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(key, null));

                    return;
                }

                id = key;
            }
            else
            {
                if (_idGenerator is null)
                {
                    _log.Error("I don't have ID Generator.");
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(key, null));

                    return;
                }

                id = _idGenerator.GenerateId();
                if (_actorMap.ContainsKey(id))
                {
                    _log.Error($"ID generated by generator is duplicated. ID={id}, Actor={_actorMap[id]}");
                    Sender.Tell(new DistributedActorTableMessage<TKey>.CreateReply(key, null));

                    return;
                }
            }

            CreateActor(RequestType.Create, id, args);
        }

        private void Handle(DistributedActorTableMessage<TKey>.GetOrCreate tableMsg)
        {
            if (_stopping)
                return;

            var id = tableMsg.Id;

            // try to get actor

            if (_actorMap.TryGetValue(id, out var actor))
            {
                if (actor != null)
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.GetOrCreateReply(tableMsg.Id, actor, Created: false));

                    return;
                }

                PutOnCreateWaitingList(RequestType.GetOrCreate, id, Sender);

                return;
            }

            CreateActor(RequestType.GetOrCreate, id, tableMsg.Args);
        }

        private void Handle(DistributedActorTableMessage<TKey>.Get get)
        {
            var id = get.Id;

            // try to get actor

            if (_actorMap.TryGetValue(id, out var actor))
            {
                if (actor != null)
                {
                    Sender.Tell(new DistributedActorTableMessage<TKey>.GetReply(get.Id, actor));

                    return;
                }

                PutOnCreateWaitingList(RequestType.GetOrCreate, id, Sender);

                return;
            }

            Sender.Tell(new DistributedActorTableMessage<TKey>.GetReply(get.Id, null));
        }

        private void Handle(DistributedActorTableMessage<TKey>.GetIds getIds)
            => Sender.Tell(new DistributedActorTableMessage<TKey>.GetIdsReply(_actorMap.Keys.ToArray()));

        private void Handle(DistributedActorTableMessage<TKey>.GracefulStop gracefulStop)
        {
            if (_stopping)
                return;

            _stopping = true;

            if (_containerMap.Count > 0)
                foreach (var pair in _containerMap)
                    pair.Key.Tell(new DistributedActorTableMessage<TKey>.Internal.GracefulStop(gracefulStop.StopMessage));
            else
                Context.Stop(Self);
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.CreateReply createReply)
        {
            var id = createReply.Id;

            if (_actorMap.TryGetValue(id, out var actor) == false)
            {
                // request was already expired so ask to remove it
                _log.Info($"I got CreateReply but might be timeouted. (Id={id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.Remove(id));

                return;
            }

            if (actor != null)
            {
                _log.Error(
                    $"I got CreateReply but already have an actor. (Id={id} Actor={actor} ArrivedActor={createReply.Actor})");

                return;
            }

            if (_creatingMap.TryGetValue(id, out var creating) == false)
            {
                _log.Error($"I got CreateReply but I don't have a creating. Id={id}");

                return;
            }

            // update created actor in map

            _creatingMap.Remove(id);
            _actorMap[id] = createReply.Actor;

            if (creating is not { WorkingContainer: { } }) return;

            _containerMap[creating.WorkingContainer].ActorMap[id] = createReply.Actor;

            // send reply to requesters

            for (var index = 0; index < creating.Requesters.Count; index++)
            {
                var (actorRef, requestType) = creating.Requesters[index];
                actorRef.Tell(CreateReplyMessage(requestType, id, createReply.Actor, index == 0));
            }
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.Add add)
        {
            var (id, actor) = add;
            if (actor is null)
            {
                _log.Error($"Invalid null actor for adding. (Id={id})");
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(id, actor, Added: false));

                return;
            }

            if (_containerMap.TryGetValue(Sender, out var container) == false)
            {
                _log.Error($"Cannot find a container trying to add an actor. (Id={id} Container={Sender})");

                return;
            }

            try
            {
                _actorMap.Add(id, actor);
                container.ActorMap.Add(id, actor);
            }
            catch (Exception)
            {
                Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(id, actor, Added: false));
                #pragma warning disable ERP022
            }
            #pragma warning restore ERP022

            Sender.Tell(new DistributedActorTableMessage<TKey>.Internal.AddReply(id, actor, Added: true));
        }

        private void Handle(DistributedActorTableMessage<TKey>.Internal.Remove remove)
        {
            if (_actorMap.TryGetValue(remove.Id, out var actor) == false)
                return;

            if (actor is null)
            {
                _log.Error($"Cannot remove an actor waiting for creating. (Id={remove.Id})");

                return;
            }

            if (_containerMap.TryGetValue(Sender, out var container) == false)
            {
                _log.Error($"Cannot find a container trying to remove an actor. (Id={remove.Id} Container={Sender})");

                return;
            }

            if (container.ActorMap.ContainsKey(remove.Id) == false)
            {
                _log.Error($"Cannot remove an actor owned by another container. (Id={remove.Id} Container={Sender})");

                return;
            }

            _actorMap.Remove(remove.Id);
            container.ActorMap.Remove(remove.Id);
        }

        private void Handle(CreateTimeoutMessage timeoutMessage)
        {
            var threshold = DateTime.UtcNow - (timeoutMessage.Timeout ?? _createTimeout);

            var expiredItems = _creatingMap.Where(entry => entry.Value != null && entry.Value.RequestTime <= threshold).ToList();
            foreach (var (id, creating) in expiredItems)
            {
                if (creating is null) continue;

                var container = creating.WorkingContainer;

                _log.Info($"CreateTimeout Id={id} Container={container}");

                // remove pending item

                _creatingMap.Remove(id);
                _actorMap.Remove(id);
                if (container is not null)
                    _containerMap[container].ActorMap.Remove(id);

                // send reply to requester

                foreach (var (actorRef, requestType) in creating.Requesters)
                    actorRef.Tell(CreateReplyMessage(requestType, id, null, false));
            }
        }

        private sealed class Container
        {
            //public DateTime LinkTime;
            internal Dictionary<TKey, IActorRef?> ActorMap { get; } = new();
        }

        private enum RequestType
        {
            Create,
            GetOrCreate,
            Get
        }

        private sealed class Creating
        {
            internal object[]? Arguments { get; init; }
            internal List<Tuple<IActorRef, RequestType>> Requesters { get; } = new();
            internal DateTime RequestTime { get; init; }
            internal IActorRef? WorkingContainer { get; set; }
        }

        internal sealed class CreateTimeoutMessage
        {
            #pragma warning disable 649
            internal TimeSpan? Timeout => null;
            #pragma warning restore 649
        }

        #pragma warning disable AV1561
        public DistributedActorTable(
            string name, IActorRef clusterActorDiscovery,
            Type? idGeneratorType, object[] idGeneratorInitializeArgs)
        {
            _name = name;
            _clusterActorDiscovery = clusterActorDiscovery;
            _log = Context.GetLogger();

            if (idGeneratorType != null)
                try
                {
                    _idGenerator = (IIdGenerator<TKey>)Activator.CreateInstance(idGeneratorType)!;
                    _idGenerator?.Initialize(idGeneratorInitializeArgs);
                }
                catch (Exception exception)
                {
                    _log.Error(exception, $"Exception in initializing ${idGeneratorType.FullName}");
                    _idGenerator = null;
                }

            Receive<ClusterActorDiscoveryMessage.ActorUp>(Handle);
            Receive<ClusterActorDiscoveryMessage.ActorDown>(Handle);

            Receive<DistributedActorTableMessage<TKey>.Create>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GetOrCreate>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Get>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GetIds>(Handle);
            Receive<DistributedActorTableMessage<TKey>.GracefulStop>(Handle);

            Receive<DistributedActorTableMessage<TKey>.Internal.CreateReply>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Internal.Add>(Handle);
            Receive<DistributedActorTableMessage<TKey>.Internal.Remove>(Handle);

            Receive<CreateTimeoutMessage>(Handle);
        }

        public DistributedActorTable(
            string primingTest,
            string name, IActorRef clusterActorDiscovery,
            Type idGeneratorType, object[] idGeneratorInitializeArgs)
            : this(name, clusterActorDiscovery, idGeneratorType, idGeneratorInitializeArgs)
        {
            if (primingTest != "TEST")
                throw new ArgumentException(nameof(primingTest));

            _underTestEnvironment = true;

            // Test environment doesn't use cluster so we need to watch container actors by itself.
            Receive<Terminated>(Handle);
        }
        #pragma warning restore AV1561
    }
}