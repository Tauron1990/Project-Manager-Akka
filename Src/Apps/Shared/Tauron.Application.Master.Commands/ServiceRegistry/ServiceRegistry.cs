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

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public sealed class ServiceRegistry
    {
        private readonly IActorRef _target;

        private ServiceRegistry(IActorRef target) => _target = target;

        public void RegisterService(RegisterService service)
            => _target.Tell(service);

        public Task<QueryRegistratedServicesResponse> QueryService()
            => _target.Ask<QueryRegistratedServicesResponse>(new QueryRegistratedServices(), TimeSpan.FromMinutes(1));

        private static readonly object Lock = new();
        private static ServiceRegistry? _registry;

        public static void Start(ActorSystem system, RegisterService? self)
        {
            if(_registry != null)
                system.Stop(_registry._target);
            _registry = new ServiceRegistry(system.ActorOf(Props.Create(() => new ServiceRegistryServiceActor(ClusterActorDiscovery.Get(system).Discovery, self)), nameof(ServiceRegistry)));
        }

        public static ServiceRegistry GetRegistry(ActorSystem refFactory)
        {
            lock (Lock)
                return _registry ??= new ServiceRegistry(refFactory.ActorOf(Props.Create(() => new ServiceRegistryClientActor()), nameof(ServiceRegistry)));
        }

        private sealed class ServiceRegistryClientActor : ReceiveActor, IWithUnboundedStash
        {
            private class CircularBuffer<T> : IEnumerable<T>
            {
                private readonly  List<T> _data = new();
                private int _curr = -1;

                public void Add(T data)
                    => _data.Add(data);

                public int Remove(T data)
                {
                    _data.Remove(data);
                    return _data.Count;
                }

                public T Next()
                {
                    if (_data.Count == 0)
                        return default!;

                    _curr++;
                    if (_curr >= _data.Count)
                        _curr = 0;
                    return _data[_curr];
                }

                public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _data).GetEnumerator();
            }

            private readonly CircularBuffer<IActorRef> _serviceRegistrys = new();
            private readonly IActorRef _discovery;
            private ILoggingAdapter Log { get; } = Context.GetLogger();

            public ServiceRegistryClientActor()
            {
                _discovery = ClusterActorDiscovery.Get(Context.System).Discovery;
                Initializing();
            }

            private void Initializing()
            {
                Receive<ClusterActorDiscoveryMessage.ActorUp>(au =>
                                                              {
                                                                  Log.Info("New Service Registry {Name}", au.Actor.Path);
                                                                  _serviceRegistrys.Add(au.Actor);
                                                                  Become(Running);
                                                                  Stash.UnstashAll();
                                                              });

                Receive<ClusterActorDiscoveryMessage.ActorDown>(ad =>
                                                                {
                                                                    Log.Info("Remove Service Registry {Name}", ad.Actor.Path);
                                                                    _serviceRegistrys.Remove(ad.Actor);
                                                                });

                ReceiveAny(_ => Stash.Stash());
            }

            private void Running()
            {
                Receive<ClusterActorDiscoveryMessage.ActorUp>(au =>
                                                              {
                                                                  Log.Info("New Service Registry {Name}", au.Actor.Path);
                                                                  _serviceRegistrys.Add(au.Actor);
                                                              });

                Receive<ClusterActorDiscoveryMessage.ActorDown>(ad =>
                                                                {
                                                                    Log.Info("Remove Service Registry {Name}", ad.Actor.Path);
                                                                    var count = _serviceRegistrys.Remove(ad.Actor);

                                                                    if (count == 0)
                                                                        Become(Initializing);
                                                                });

                Receive<RegisterService>(s =>
                                         {
                                             Log.Info("Register New Service {Name} -- {Adress}", s.Name, s.Address);
                                             _serviceRegistrys.Foreach(r => r.Tell(s));
                                         });

                Receive<QueryRegistratedServices>(rs =>
                                                  {
                                                      Log.Info("Try Query Service");
                                                      var target = _serviceRegistrys.Next();
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
                                                  });
            }

            protected override void PreStart()
            {
                _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistry)));
                base.PreStart();
            }

            public IStash Stash { get; set; } = null!;
        }

        private sealed class ServiceRegistryServiceActor : ReceiveActor
        {
            private sealed class SyncRegistry
            {
                public Dictionary<UniqueAddress, string> ToSync { get; }

                public SyncRegistry(Dictionary<UniqueAddress, string> sync) => ToSync = sync;
            }
            
            private readonly Dictionary<UniqueAddress, string> _services = new();
            private readonly IActorRef _discovery;
            private ILoggingAdapter Log { get; } = Context.GetLogger();

            public ServiceRegistryServiceActor(IActorRef discovery, RegisterService? self)
            {
                _discovery = discovery;
                if (self != null) _services[self.Address] = self.Name;

                Receive<RegisterService>(service =>
                                         {
                                             Log.Info("Register Service {Name} -- {Adress}", service.Name, service.Address);
                                             _services[service.Address] = service.Name;
                                         });

                Receive<QueryRegistratedServices>(_ =>
                                                  {
                                                      Log.Info("Return Registrated Services");
                                                      var temp = _services
                                                         .ToDictionary(service => MemberAddress.From(service.Key),
                                                                       service => service.Value);

                                                      Sender.Tell(new QueryRegistratedServicesResponse(temp
                                                                                                      .Select(e => new MemberService(e.Value, e.Key))
                                                                                                      .ToImmutableList()));
                                                  });

                Receive<ClusterActorDiscoveryMessage.ActorUp>(au =>
                                                              {
                                                                  if(au.Actor.Equals(Self)) return;
                                                                  Log.Info("Send Sync New Service registry");
                                                                  au.Actor.Tell(new SyncRegistry(_services));
                                                              });

                Receive<ClusterActorDiscoveryMessage.ActorDown>(_ => { });

                Receive<SyncRegistry>(sr =>
                                      {
                                          Log.Info("Sync Services");
                                          sr.ToSync.Foreach(kp => _services[kp.Key] = kp.Value);
                                      });
            }

            protected override void PreStart()
            {
                _discovery.Tell(new ClusterActorDiscoveryMessage.RegisterActor(Self, nameof(ServiceRegistry)));
                _discovery.Tell(new ClusterActorDiscoveryMessage.MonitorActor(nameof(ServiceRegistry)));
                base.PreStart();
            }
        }
    }
}