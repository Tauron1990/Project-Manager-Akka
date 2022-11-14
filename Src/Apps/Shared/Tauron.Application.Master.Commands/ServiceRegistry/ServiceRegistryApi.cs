using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using Akka.Event;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.ServiceRegistry;

[PublicAPI]
public sealed class ServiceRegistryApi
{
    public static readonly ServiceRegistryApi Empty = new(ActorRefs.Nobody);

    private static readonly object Lock = new();
    private static ServiceRegistryApi? _registry;
    private readonly IActorRef _target;

    private ServiceRegistryApi(IActorRef target) => _target = target;

    public void RegisterService(RegisterService service)
        => _target.Tell(service);

    public Task<QueryRegistratedServicesResponse> QueryServices()
        => _target.Ask<QueryRegistratedServicesResponse>(new QueryRegistratedServices(), TimeSpan.FromMinutes(1));

    public Task<QueryRegistratedServiceResponse> QueryService(Member member)
        => _target.Ask<QueryRegistratedServiceResponse>(new QueryRegistratedService(member.UniqueAddress));

    public static ServiceRegistryApi Start(ActorSystem system, RegisterService? self)
    {
        if(_registry != null)
            system.Stop(_registry._target);
        _registry = new ServiceRegistryApi(system.ActorOf(Props.Create(() => new ServiceRegistryServiceActor(ClusterActorDiscovery.Get(system).Discovery, self)), nameof(ServiceRegistryApi)));

        return _registry;
    }

    public static ServiceRegistryApi Start(ActorSystem system, Func<Cluster, RegisterService?> self)
    {
        if(_registry != null)
            system.Stop(_registry._target);
        _registry = new ServiceRegistryApi(system.ActorOf(Props.Create(() => new ServiceRegistryServiceActor(self)), nameof(ServiceRegistryApi)));

        return _registry;
    }

    public static ServiceRegistryApi Get(ActorSystem refFactory)
    {
        lock (Lock)
        {
            return _registry ??= new ServiceRegistryApi(refFactory.ActorOf(Props.Create(() => new ServiceRegistryClientActor()), nameof(ServiceRegistryApi)));
        }
    }

    private sealed class ServiceRegistryClientActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef _discovery;

        private readonly CircularBuffer<IActorRef> _serviceRegistrys = new();

        internal ServiceRegistryClientActor()
        {
            _discovery = ClusterActorDiscovery.Get(Context.System).Discovery;
            Initializing();
        }

        private ILoggingAdapter Log { get; } = Context.GetLogger();

        public IStash Stash { get; set; } = null!;

        private void Initializing()
        {
            Receive<ClusterActorDiscoveryMessage.ActorUp>(OnActorUp);
            Receive<ClusterActorDiscoveryMessage.ActorDown>(OnActorDown);
            ReceiveAny(_ => Stash.Stash());
        }

        private void OnActorDown(ClusterActorDiscoveryMessage.ActorDown ad)
        {
            Log.Info("Remove Service Registry {Name}", ad.Actor.Path);
            #pragma warning disable GU0011
            _serviceRegistrys.Remove(ad.Actor);
            #pragma warning restore GU0011
        }

        private void OnActorUp(ClusterActorDiscoveryMessage.ActorUp au)
        {
            Log.Info("New Service Registry {Name}", au.Actor.Path);
            _serviceRegistrys.Add(au.Actor);
            Become(Running);
            Stash.UnstashAll();
        }

        private void Running()
        {
            Receive<ClusterActorDiscoveryMessage.ActorUp>(OnActorUppRunning);
            Receive<ClusterActorDiscoveryMessage.ActorDown>(OnActorDownRunning);
            Receive<RegisterService>(OnRegisterService);
            Receive<QueryRegistratedServices>(QueryServices);
            Receive<QueryRegistratedService>(QueryServices);
        }

        private void QueryServices(QueryRegistratedService r)
        {
            Log.Info("Try Query Service");
            IActorRef? target = _serviceRegistrys.Next();
            switch (target)
            {
                case null:
                    Log.Warning("No Service Registry Registrated");
                    Sender.Tell(new QueryRegistratedServicesResponse(ImmutableList<MemberService>.Empty));

                    break;
                default:
                    target.Forward(r);

                    break;
            }
        }

        private void QueryServices(QueryRegistratedServices rs)
        {
            Log.Info("Try Query Services");
            IActorRef? target = _serviceRegistrys.Next();
            switch (target)
            {
                case null:
                    Log.Warning("No Service Registry Registrated");
                    Sender.Tell(new QueryRegistratedServicesResponse(ImmutableList<MemberService>.Empty));

                    break;
                default:
                    target.Forward(rs);

                    break;
            }
        }

        private void OnRegisterService(RegisterService s)
        {
            Log.Info("Register New Service {Name} -- {Adress}", s.Name, s.Address);
            _serviceRegistrys.Foreach(r => r.Tell(s));
        }

        private void OnActorDownRunning(ClusterActorDiscoveryMessage.ActorDown ad)
        {
            Log.Info("Remove Service Registry {Name}", ad.Actor.Path);
            int count = _serviceRegistrys.Remove(ad.Actor);

            if(count == 0)
                Become(Initializing);
        }

        private void OnActorUppRunning(ClusterActorDiscoveryMessage.ActorUp au)
        {
            Log.Info("New Service Registry {Name}", au.Actor.Path);
            _serviceRegistrys.Add(au.Actor);
        }

        protected override void PreStart()
        {
            _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistryApi)));
            base.PreStart();
        }

        private class CircularBuffer<T> : IEnumerable<T>
        {
            private readonly List<T> _data = new();
            private int _curr = -1;

            public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();

            internal void Add(T data)
                => _data.Add(data);

            internal int Remove(T data)
            {
                _data.Remove(data);

                return _data.Count;
            }

            internal T Next()
            {
                if(_data.Count == 0)
                    return default!;

                _curr++;
                if(_curr >= _data.Count)
                    _curr = 0;

                return _data[_curr];
            }
        }
    }

    private sealed class ServiceRegistryServiceActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly Dictionary<UniqueAddress, ServiceEntry> _services = new();
        private IActorRef _discovery = ActorRefs.Nobody;
        private RegisterService? _self;

        #pragma warning disable GU0073
        public ServiceRegistryServiceActor(IActorRef discovery, RegisterService? self)
            #pragma warning restore GU0073
        {
            _discovery = discovery;
            _self = self;

            Running();
        }

        #pragma warning disable GU0073
        public ServiceRegistryServiceActor(Func<Cluster, RegisterService?> selfCreation)
            #pragma warning restore GU0073
        {
            Cluster cluster = Cluster.Get(Context.System);
            IActorRef self = Self;

            cluster.RegisterOnMemberUp(
                () =>
                {
                    _self = selfCreation(cluster);
                    _discovery = ClusterActorDiscovery.Get(Context.System).Discovery;
                    _discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ServiceRegistryApi)));
                    _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistryApi)));

                    self.Tell(new Init());
                });

            Receive<Init>(
                _ =>
                {
                    Stash?.UnstashAll();
                    Stash = null;
                    Become(Running);
                });
            ReceiveAny(_ => Stash?.Stash());
        }

        private ILoggingAdapter Log { get; } = Context.GetLogger();

        [UsedImplicitly]
        public IStash? Stash { get; set; }

        private void Running()
        {
            if(_self != null) _services[_self.Address] = new ServiceEntry(_self.Name, _self.ServiceType);

            Receive<RegisterService>(
                service =>
                {
                    Log.Info("Register Service {Name} -- {Adress}", service.Name, service.Address);
                    _services[service.Address] = new ServiceEntry(service.Name, service.ServiceType);
                });

            Receive<QueryRegistratedServices>(
                _ =>
                {
                    Log.Info("Return Registrated Services");
                    var temp = _services
                       .ToDictionary(
                            service => MemberAddress.From(service.Key),
                            service => service.Value);

                    Sender.Tell(
                        new QueryRegistratedServicesResponse(
                            temp
                               .Select(e => new MemberService(e.Value.Name, e.Key, e.Value.ServiceType))
                               .ToImmutableList()));
                });

            Receive<QueryRegistratedService>(
                s => Sender.Tell(
                    _services.TryGetValue(s.Address, out ServiceEntry? entry)
                        ? new QueryRegistratedServiceResponse(new MemberService(entry.Name, MemberAddress.From(s.Address), entry.ServiceType))
                        : new QueryRegistratedServiceResponse(null)));

            Receive<ClusterActorDiscoveryMessage.ActorUp>(
                au =>
                {
                    if(au.Actor.Equals(Self)) return;

                    Log.Info("Send Sync New Service registry");
                    au.Actor.Tell(new SyncRegistry(_services));
                });

            Receive<ClusterActorDiscoveryMessage.ActorDown>(_ => { });

            Receive<SyncRegistry>(
                sr =>
                {
                    Log.Info("Sync Services");
                    sr.ToSync.Foreach(kp => _services[kp.Key] = kp.Value);
                });
        }

        protected override void PreStart()
        {
            if(!_discovery.Equals(ActorRefs.Nobody))
            {
                _discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ServiceRegistryApi)));
                _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistryApi)));
            }

            base.PreStart();
        }

        private sealed record ServiceEntry(ServiceName Name, ServiceType ServiceType);

        private sealed class SyncRegistry
        {
            internal SyncRegistry(Dictionary<UniqueAddress, ServiceEntry> sync) => ToSync = sync;
            internal Dictionary<UniqueAddress, ServiceEntry> ToSync { get; }
        }

        private sealed record Init;
    }
}