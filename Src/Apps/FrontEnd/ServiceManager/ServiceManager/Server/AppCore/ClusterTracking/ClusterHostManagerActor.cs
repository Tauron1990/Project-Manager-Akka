using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using DynamicData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceHost.Client.Shared;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public sealed class ClusterHostManagerActor : ActorFeatureBase<ClusterHostManagerActor.ClusterState>
    {
        public sealed record ClusterState(SourceCache<MemberData, MemberAddress> Entrys, IHubContext<ClusterInfoHub> Hub);

        public static Func<IHubContext<ClusterInfoHub>, IPreparedFeature> New()
        {
            static IPreparedFeature _(IHubContext<ClusterInfoHub> hub)
                => Feature.Create(() => new ClusterHostManagerActor(),
                    _ => new ClusterState(new SourceCache<MemberData, MemberAddress>(md => MemberAddress.From(md.Member.UniqueAddress)), hub));

            return _;
        }

        protected override void ConfigImpl()
        {
            var entrys = CurrentState.Entrys;

            var cluster = Cluster.Get(Context.System);

            ServiceRegistry serviceRegistry = ServiceRegistry.Start(Context.System, 
                c => new RegisterService(ActorApplication.ServiceProvider.GetRequiredService<IHostEnvironment>().ApplicationName, c.SelfUniqueAddress, ServiceTypes.ServiceManager));

            cluster.RegisterOnMemberUp(() => cluster.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, typeof(ClusterEvent.IClusterDomainEvent)));
            
            Receive<ClusterEvent.IClusterDomainEvent>(obs => obs.ToUnit(evt =>
                                                                        {
                                                                            if(evt.Event is not ClusterEvent.MemberStatusChange c)
                                                                                return;

                                                                            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                                                            switch (c.Member.Status)
                                                                            {
                                                                                case MemberStatus.Joining:
                                                                                    entrys.AddOrUpdate(new MemberData(c.Member, string.Empty, ServiceType.Empty));
                                                                                    break;
                                                                                case MemberStatus.Up:
                                                                                    serviceRegistry.QueryService(c.Member)
                                                                                                   .PipeTo(Context.Self, success: q => q,
                                                                                                        failure: e =>
                                                                                                                 {
                                                                                                                     Log.Warning(e, "Error on Query Service Data for {Address}", c.Member.UniqueAddress);
                                                                                                                     return new QueryRegistratedServiceResponse(null);
                                                                                                                 });
                                                                                    if(!entrys.Lookup(MemberAddress.From(c.Member.UniqueAddress)).HasValue)
                                                                                        entrys.AddOrUpdate(new MemberData(c.Member, string.Empty, ServiceType.Empty));
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
                                                             select data.Value with {Name = service!.Name, ServiceType = service!.ServiceType})
                                                        .Subscribe(entrys.AddOrUpdate));

            Receive<GetMemberChangeset>(obs => obs.Select(d => new MemberChangeSet(d.Event.Prepare(entrys.Connect()))).ToSender());

            Receive<QueryAllNodes>(obs => (from r in obs
                                           select new AllNodesResponse(r.State.Entrys.Items.Select(m => new ClusterNodeInfo(m.Member.Address.ToString(), m)).ToArray()))
                                      .ToSender());

            Receive<InitActor>(obs => obs.ToUnit(() =>
                                                 {
                                                     CurrentState.Entrys.Connect()
                                                                 .Select(cs =>
                                                                         {
                                                                             var changes = new List<NodeChange>();
                                                                             foreach (var change in cs)
                                                                             {
                                                                                 // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                                                                                 switch (change.Reason)
                                                                                 {
                                                                                     case ChangeReason.Remove:
                                                                                         changes.Add(new NodeChange(change.Current, true));
                                                                                         break;
                                                                                     default:
                                                                                         changes.Add(new NodeChange(change.Current, false));
                                                                                         break;
                                                                                 }
                                                                             }

                                                                             return changes;
                                                                         })
                                                                 .SelectMany(l => CurrentState.Hub.Clients.All.SendAsync(HubEvents.NodesChanged, l.ToArray()).ToObservable())
                                                                 .AutoSubscribe(e => Log.Error(e, "Error on sending Nodes Changed"));
                                                 }));
        }
    }
}