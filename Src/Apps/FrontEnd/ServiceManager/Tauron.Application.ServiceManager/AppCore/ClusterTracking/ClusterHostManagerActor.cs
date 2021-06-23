using System;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using DynamicData;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Features;

namespace Tauron.Application.ServiceManager.AppCore.ClusterTracking
{
    public sealed class ClusterHostManagerActor : ActorFeatureBase<ClusterHostManagerActor.ClusterState>
    {
        public sealed record ClusterState(SourceCache<MemberData, MemberAddress> Entrys);

        public static Func<IPreparedFeature> New()
        {
            IPreparedFeature _()
                => Feature.Create(() => new ClusterHostManagerActor(), _ => new ClusterState(new SourceCache<MemberData, MemberAddress>(md => MemberAddress.From(md.Member.UniqueAddress))));

            return _;
        }

        protected override void ConfigImpl()
        {
            var entrys = CurrentState.Entrys;

            Cluster.Get(Context.System).Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents);
            var registry = ServiceRegistry.Get(Context.System);

            Receive<ClusterEvent.IClusterDomainEvent>(obs => obs.OfType<ClusterEvent.MemberStatusChange>()
                                                                .ToUnit(c =>
                                                                        {
                                                                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                                                            switch (c.Member.Status)
                                                                            {
                                                                                case MemberStatus.Joining:
                                                                                    entrys.AddOrUpdate(new MemberData(c.Member, string.Empty, ServiceType.Empty));
                                                                                    break;
                                                                                case MemberStatus.Up:
                                                                                    registry.QueryService(c.Member)
                                                                                            .PipeTo(Context.Self, success: q => q,
                                                                                                 failure: e =>
                                                                                                          {
                                                                                                              Log.Warning(e, "Eroor on Query Service Data for {Adress}", c.Member.UniqueAddress);
                                                                                                              return new QueryRegistratedServiceResponse(null);
                                                                                                          });
                                                                                    break;
                                                                                case MemberStatus.Removed:
                                                                                    entrys.RemoveKey(MemberAddress.From(c.Member.UniqueAddress));
                                                                                    break;
                                                                                default:
                                                                                    var data = entrys.Lookup(MemberAddress.From(c.Member.UniqueAddress));
                                                                                    if (data.HasValue)
                                                                                        entrys.AddOrUpdate(data.Value with {Member = c.Member});
                                                                                    break;
                                                                            }
                                                                        }));

            Receive<QueryRegistratedServiceResponse>(obs => (from response in obs
                                                             let service = response.Event.Service
                                                             where service != null
                                                             let data = entrys.Lookup(service.Address)
                                                             where data.HasValue
                                                             select data.Value with {Name = service.Name, ServiceType = service.ServiceType})
                                                        .Subscribe(entrys.AddOrUpdate));

            Receive<GetMemberChangeset>(obs => obs.Select(d => new MemberChangeSet(d.Event.Prepare(entrys.Connect()))).ToSender());
        }
    }
}