using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Cluster;
using Microsoft.Extensions.Hosting;
using ServiceHost.Client.Shared;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Server.AppCore.Helper;
using Stl.CommandR;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Features;
using Tauron.TAkka;

namespace ServiceManager.Server.AppCore.ClusterTracking
{
    public sealed class ClusterHostManagerActor : ActorFeatureBase<ClusterHostManagerActor.ClusterState>
    {
        public static Func<ICommander, IPreparedFeature> New()
        {
            static IPreparedFeature _(ICommander commander)
                => Feature.Create(
                    () => new ClusterHostManagerActor(),
                    c => new ClusterState(commander, ImmutableHashSet<MemberAddress>.Empty, Cluster.Get(c.System)));

            return _;
        }


        protected override void ConfigImpl()
        {
            var cluster = Cluster.Get(Context.System);

            ServiceRegistry serviceRegistry = ServiceRegistry.Start(
                Context.System,
                c => new RegisterService(ActorApplication.ServiceProvider.GetRequiredService<IHostEnvironment>().ApplicationName, c.SelfUniqueAddress, ServiceTypes.ServiceManager));

            cluster.RegisterOnMemberUp(() => cluster.Subscribe(Self, ClusterEvent.SubscriptionInitialStateMode.InitialStateAsEvents, typeof(ClusterEvent.IClusterDomainEvent)));

            Receive<ClusterEvent.IClusterDomainEvent>(
                obs => obs.SelectMany(
                    async evt =>
                    {
                        var (clusterDomainEvent, state) = evt;

                        if (clusterDomainEvent is not ClusterEvent.MemberStatusChange c)
                            return state;

                        async Task<ClusterState> ExecAdd()
                        {
                            if (state.CurrentMember.Contains(MemberAddress.From(c.Member.UniqueAddress)))
                                return state;

                            await state.Commander.Call(
                                new AddNodeCommad(
                                    c.Member.UniqueAddress.ToString(),
                                    "Abrufen",
                                    ToString(c.Member.Status),
                                    "Unbekannt"));

                            return state with
                                   {
                                       CurrentMember = state.CurrentMember.Add(MemberAddress.From(c.Member.UniqueAddress))
                                   };
                        }

                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        switch (c.Member.Status)
                        {
                            case MemberStatus.Joining:
                                return await ExecAdd();
                            case MemberStatus.Up:

                                var newState = state.CurrentMember.Contains(MemberAddress.From(c.Member.UniqueAddress))
                                    ? state
                                    : await ExecAdd();

                                serviceRegistry.QueryService(c.Member)
                                   .PipeTo(
                                        Context.Self,
                                        success: q => q,
                                        failure: e =>
                                                 {
                                                     Log.Warning(e, "Error on Query Service Data for {Address}", c.Member.UniqueAddress);

                                                     return new QueryRegistratedServiceResponse(null);
                                                 })
                                   .Ignore();

                                return newState;
                            case MemberStatus.Removed:
                                await state.Commander.Call(new RemoveNodeCommand(c.Member.UniqueAddress.ToString()));

                                return state with
                                       {
                                           CurrentMember = state.CurrentMember.Remove(MemberAddress.From(c.Member.UniqueAddress))
                                       };
                            default:
                                return await AddAndUpdate(
                                    state,
                                    c.Member,
                                    commander => commander.Call(new UpdateStatusCommand(c.Member.UniqueAddress.ToString(), ToString(c.Member.Status))));
                        }
                    }).ObserveOn(ActorScheduler.CurrentSelf));

            Receive<QueryRegistratedServiceResponse>(
                obs => (from response in obs
                        let service = response.Event.Service
                        where service != null
                        from result in AddAndUpdate(
                            response.State,
                            response.State.Cluster.State.Members.First(m => m.UniqueAddress == service.Address),
                            com => com.Call(new UpdateNameCommand(service.Address.ToString(), service.Name, service.ServiceType.DisplayName)))
                        select result)
                   .ObserveOn(ActorScheduler.CurrentSelf));

            Receive<InitActor>(obs => obs.ToUnit());
        }

        private async Task<ClusterState> AddAndUpdate(ClusterState state, Member member, Func<ICommander, Task> updater)
        {
            if (state.CurrentMember.Contains(MemberAddress.From(member.UniqueAddress)))
            {
                await updater(state.Commander);

                return state;
            }

            await state.Commander.Call(new AddNodeCommad(member.UniqueAddress.ToString(), "Unbekannt", ToString(member.Status), "Unbekannt"));
            await updater(state.Commander);

            return state with { CurrentMember = state.CurrentMember.Add(MemberAddress.From(member.UniqueAddress)) };
        }


        private string ToString(MemberStatus status)
            => status switch
            {
                MemberStatus.Joining => "Beitreten",
                MemberStatus.Up => "Mitglied",
                MemberStatus.Leaving => "am Verlassen",
                MemberStatus.Exiting => "am Verlassen",
                MemberStatus.Down => "Verlassen",
                MemberStatus.Removed => "Entfernt",
                MemberStatus.WeaklyUp => "Aufweken",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };

        public sealed record ClusterState(ICommander Commander, ImmutableHashSet<MemberAddress> CurrentMember, Cluster Cluster);
    }
}