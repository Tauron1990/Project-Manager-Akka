using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.TAkka;
using Tauron.Features;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands.KillSwitch;

internal static partial class KillSwitchWatchLog
{
    [LoggerMessage(EventId = 29, Level = LogLevel.Information, Message = "Send ActorUp Back {path}")]
    internal static partial void SendingActorUp(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 30, Level = LogLevel.Information, Message = "Sending Respond {type}")]
    internal static partial void SendingRespond(ILogger logger, KillRecpientType type);

    [LoggerMessage(EventId = 31, Level = LogLevel.Information, Message = "Leaving Cluster")]
    internal static partial void LeavingCluster(ILogger logger);
}

internal static partial class KillSwitchFeatureLog
{
    [LoggerMessage(EventId = 32, Level = LogLevel.Information, Message = "Remove KillSwitch Actor {path}")]
    internal static partial void RemoveKillswitch(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 33, Level = LogLevel.Information, Message = "NewKillSwitch Actor {path}")]
    internal static partial void NewKillSwitch(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 34, Level = LogLevel.Information, Message = "Set KillSwitch Actor Type {type} {path}")]
    internal static partial void KillSwitchActorType(ILogger logger, KillRecpientType type, ActorPath path);

    [LoggerMessage(EventId = 35, Level = LogLevel.Information, Message = "Begin Cluster Shutdown")]
    internal static partial void BeginClusterShutdown(ILogger logger);

    [LoggerMessage(EventId = 36, Level = LogLevel.Information, Message = "Incomming KillSwitch Actor {path}")]
    internal static partial void IncommingKillWatcher(ILogger logger, ActorPath path);

    [LoggerMessage(EventId = 37, Level = LogLevel.Information, Message = "Send Kill Message for {type} to {count} Actors")]
    internal static partial void TellClusterShutdown(ILogger logger, KillRecpientType type, int count);
}

[PublicAPI]
public static partial class KillSwitch
{
    private const string KillSwitchName = "KillSwitch";

    private static readonly ILogger Logger = TauronEnviroment.GetLogger(typeof(KillSwitch));

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
            KillRecpientType.Seed
        };

        public static IPreparedFeature Create()
            => Feature.Create(() => new KillSwitchFeature(), _ => new KillState(ImmutableList<ActorElement>.Empty));

        protected override void ConfigImpl()
        {
            Receive<ActorDown>(
                obs => obs.Do(ad => KillSwitchFeatureLog.RemoveKillswitch(Logger, ad.Event.Actor.Path))
                   .Select(
                        m =>
                        {
                            (ActorDown actorDown, KillState state) = m;
                            ActorElement? entry = state.Actors.Find(e => e.Target.Equals(actorDown.Actor));

                            if (entry is null)
                                return state;

                            return state with { Actors = state.Actors.Remove(entry) };
                        }));

            Receive<ActorUp>(
                obs => obs.Do(au => KillSwitchFeatureLog.NewKillSwitch(Logger, au.Event.Actor.Path))
                   .Select(
                        m =>
                        {
                            (ActorUp actorUp, KillState state) = m;
                            actorUp.Actor.Tell(new RequestRegistration());

                            return state with { Actors = state.Actors.Add(ActorElement.New(actorUp.Actor)) };
                        }));

            Receive<RespondRegistration>(
                obs => obs
                   .Do(r => KillSwitchFeatureLog.KillSwitchActorType(Logger, r.Event.RecpientType, r.Sender.Path))
                   .Select(
                        r =>
                        {
                            (RespondRegistration respondRegistration, KillState state) = r;
                            ActorElement? ele = state.Actors.Find(e => e.Target.Equals(Sender));

                            if (ele is null)
                                return state;

                            return state with
                                   {
                                       Actors = state.Actors.Replace(
                                           ele,
                                           ele with { RecpientType = respondRegistration.RecpientType })
                                   };
                        }));

            Receive<RequestRegistration>(
                obs => obs.Select(_ => new RespondRegistration(KillRecpientType.Seed))
                   .ToSender());

            Receive<KillClusterMsg>(obs => obs.SubscribeWithStatus(_ => RunKillCluster()));

            Receive<KillNode>(
                obs => obs.Do(_ => KillSwitchWatchLog.LeavingCluster(Logger))
                   .SubscribeWithStatus(_ => Cluster.Get(Context.System).LeaveAsync()));

            Receive<ActorUp>(
                obs => obs.Where(m => m.Event.Tag == nameof(KillWatcher))
                   .Do(up => KillSwitchFeatureLog.IncommingKillWatcher(Logger, up.Event.Actor.Path))
                   .Select(m => m.Event.Actor)
                   .SubscribeWithStatus(actor => Context.Watch(actor)));

            Receive<Terminated>(
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
                          KillRecpientType.Service
                      };

            foreach ((IActorRef target, KillRecpientType recpientType) in CurrentState.Actors)
                dic.Add(recpientType, target);

            foreach (var recpientType in Order)
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