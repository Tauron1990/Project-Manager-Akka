using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Features;
using Tauron.TAkka;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.KillSwitch;

[PublicAPI]
public static partial class KillSwitchApi
{
    private const string KillSwitchName = "KillSwitch";

    private static readonly ILogger Logger = TauronEnviroment.GetLogger(typeof(KillSwitchApi));

    private static IActorRef _switch = ActorRefs.Nobody;

    [LoggerMessage(EventId = 27, Level = LogLevel.Information, Message = "Setup Killswitch")]
    private static partial void SetupKillswitch(ILogger logger);

    [LoggerMessage(EventId = 28, Level = LogLevel.Information, Message = "Subscribe To Event Killswitch")]
    private static partial void SubscribeKillswitch(ILogger logger);

    public static void Setup(ActorSystem system)
    {
        SetupKillswitch(Logger);
        _switch = system.ActorOf(KillSwitchName, KillSwitchFeature.Create());
    }

    public static void Subscribe(ActorSystem system, KillRecpientType type)
    {
        SubscribeKillswitch(Logger);
        _switch = system.ActorOf(() => new KillWatcher(type), KillSwitchName);
    }

    public static void KillCluster()
        => _switch.Tell(new KillClusterMsg());

    public sealed class KillWatcher : ObservableActor
    {
        public KillWatcher(KillRecpientType type)
        {
            Receive<ActorUp>(
                obs => obs.Do(au => KillSwitchWatchLog.SendingActorUp(Log, au.Actor.Path))
                   .SubscribeWithStatus(au => au.Actor.Tell(new ActorUp(Self, nameof(KillWatcher)))));

            Receive<RequestRegistration>(
                obs => obs.Do(_ => KillSwitchWatchLog.SendingRespond(Log, type))
                   .Select(_ => new RespondRegistration(type))
                   .ToSender());

            Receive<KillNode>(
                obs => obs.Do(_ => KillSwitchWatchLog.LeavingCluster(Log))
                   .ToUnit(() => Cluster.Get(Context.System).LeaveAsync()));
        }

        protected override void PreStart()
        {
            ClusterActorDiscovery.Get(Context.System).Discovery.Tell(new MonitorActor(KillSwitchName));
            base.PreStart();
        }
    }

    public sealed class KillSwitchFeature : ActorFeatureBase<KillSwitchFeature.KillState>
    {
        private static readonly KillRecpientType[] Order =
        {
            KillRecpientType.Unkowen, KillRecpientType.Frontend, KillRecpientType.Host, KillRecpientType.Service,
            KillRecpientType.Seed,
        };

        public static IPreparedFeature Create()
            => Feature.Create(() => new KillSwitchFeature(), _ => new KillState(ImmutableList<ActorElement>.Empty));

        #pragma warning disable MA0051
        protected override void ConfigImpl()
            #pragma warning restore MA0051
        {
            Observ<ActorDown>(
                obs => obs.Do(ad => KillSwitchFeatureLog.RemoveKillswitch(Logger, ad.Event.Actor.Path))
                   .Select(
                        m =>
                        {
                            (ActorDown actorDown, KillState state) = m;
                            ActorElement? entry = state.Actors.Find(e => e.Target.Equals(actorDown.Actor));

                            if(entry is null)
                                return state;

                            return state with { Actors = state.Actors.Remove(entry) };
                        }));

            Observ<ActorUp>(
                obs => obs.Do(au => KillSwitchFeatureLog.NewKillSwitch(Logger, au.Event.Actor.Path))
                   .Select(
                        m =>
                        {
                            (ActorUp actorUp, KillState state) = m;
                            actorUp.Actor.Tell(new RequestRegistration());

                            return state with { Actors = state.Actors.Add(ActorElement.New(actorUp.Actor)) };
                        }));

            Observ<RespondRegistration>(
                obs => obs
                   .Do(r => KillSwitchFeatureLog.KillSwitchActorType(Logger, r.Event.RecpientType, r.Sender.Path))
                   .Select(
                        r =>
                        {
                            (RespondRegistration respondRegistration, KillState state) = r;
                            ActorElement? ele = state.Actors.Find(e => e.Target.Equals(Sender));

                            if(ele is null)
                                return state;

                            return state with
                                   {
                                       Actors = state.Actors.Replace(
                                           ele,
                                           ele with { RecpientType = respondRegistration.RecpientType }),
                                   };
                        }));

            Observ<RequestRegistration>(
                obs => obs.Select(_ => new RespondRegistration(KillRecpientType.Seed))
                   .ToSender());

            Observ<KillClusterMsg>(obs => obs.SubscribeWithStatus(_ => RunKillCluster()));

            Observ<KillNode>(
                obs => obs.Do(_ => KillSwitchWatchLog.LeavingCluster(Logger))
                   .SubscribeWithStatus(_ => Cluster.Get(Context.System).LeaveAsync()));

            Observ<ActorUp>(
                obs => obs.Where(m => string.Equals(m.Event.Tag, nameof(KillWatcher), StringComparison.Ordinal))
                   .Do(up => KillSwitchFeatureLog.IncommingKillWatcher(Logger, up.Event.Actor.Path))
                   .Select(m => m.Event.Actor)
                   .SubscribeWithStatus(actor => Context.Watch(actor)));

            Observ<Terminated>(
                obs => obs.Select(t => new ActorDown(t.Event.ActorRef, nameof(KillWatcher)))
                   .ToSelf());

            Start.Subscribe(
                _ =>
                {
                    IActorRef actorDiscovery = ClusterActorDiscovery.Get(ObservableActor.Context.System).Discovery;

                    actorDiscovery.Tell(new RegisterActor(Self, KillSwitchName));
                    actorDiscovery.Tell(new MonitorActor(nameof(KillSwitchName)));
                });
        }

        private void RunKillCluster()
        {
            KillSwitchFeatureLog.BeginClusterShutdown(Logger);

            var dic = new GroupDictionary<KillRecpientType, IActorRef>
                      {
                          KillRecpientType.Unkowen,
                          KillRecpientType.Frontend,
                          KillRecpientType.Host,
                          KillRecpientType.Seed,
                          KillRecpientType.Service,
                      };

            foreach ((IActorRef target, KillRecpientType recpientType) in CurrentState.Actors)
                dic.Add(recpientType, target);

            #pragma warning disable GU0071
            foreach (KillRecpientType recpientType in Order)
                #pragma warning restore GU0071
            {
                var actors = dic[recpientType];
                KillSwitchFeatureLog.TellClusterShutdown(Logger, recpientType, actors.Count);
                foreach (IActorRef actorRef in actors)
                    actorRef.Tell(new KillNode());
            }
        }

        public sealed record KillState(ImmutableList<ActorElement> Actors);


        public sealed record ActorElement(
            IActorRef Target,
            KillRecpientType RecpientType = KillRecpientType.Unkowen)
        {
            public static ActorElement New(IActorRef r)
                => new(r);
        }
    }

    public sealed record RespondRegistration(KillRecpientType RecpientType);

    public sealed record RequestRegistration;

    public sealed record KillNode;

    public sealed record KillClusterMsg;
}