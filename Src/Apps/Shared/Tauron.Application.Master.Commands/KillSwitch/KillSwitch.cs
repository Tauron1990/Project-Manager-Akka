using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Utility;
using JetBrains.Annotations;
using NLog;
using Tauron.Akka;
using Tauron.Features;
using static Akka.Cluster.Utility.ClusterActorDiscoveryMessage;

namespace Tauron.Application.Master.Commands
{
    [PublicAPI]
    public static class KillSwitch
    {
        private const string KillSwitchName = "KillSwitch";

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private static IActorRef _switch = ActorRefs.Nobody;

        public static void Setup(ActorSystem system)
        {
            Log.Info("Setup Killswitch");
            _switch = system.ActorOf(KillSwitchName, KillSwitchFeature.Create());
        }

        public static void Subscribe(ActorSystem system, KillRecpientType type)
        {
            Log.Info("SubscribeToEvent Killswitch");
            _switch = system.ActorOf(() => new KillWatcher(type), KillSwitchName);
        }

        public static void KillCluster()
            => _switch.Tell(new KillClusterMsg());

        public sealed class KillWatcher : ObservableActor
        {
            public KillWatcher(KillRecpientType type)
            {
                Receive<ActorUp>(obs => obs.Do(au => Log.Info("Send ActorUp Back {Name}", au.Actor.Path))
                    .SubscribeWithStatus(au => au.Actor.Tell(new ActorUp(Self, nameof(KillWatcher)))));

                Receive<RequestRegistration>(obs => obs.Do(_ => Log.Info($"Sending Respond {type}"))
                    .Select(_ => new RespondRegistration(type))
                    .ToSender());

                Receive<KillNode>(obs => obs.Do(_ => Log.Info("Leaving Cluster"))
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
                Receive<ActorDown>(obs => obs.Do(ad => Log.Info($"Remove KillSwitch Actor {ad.Event.Actor.Path}"))
                    .Select(m =>
                    {
                        var (actorDown, state, _) = m;
                        var entry = state.Actors.Find(e => e.Target.Equals(actorDown.Actor));

                        if (entry == null)
                            return state;
                        return state with {Actors = state.Actors.Remove(entry)};
                    }));

                Receive<ActorUp>(obs => obs.Do(au => Log.Info($"New killswitch Actor {au.Event.Actor.Path}"))
                    .Select(m =>
                    {
                        var (actorUp, state, _) = m;
                        actorUp.Actor.Tell(new RequestRegistration());
                        return state with {Actors = state.Actors.Add(ActorElement.New(actorUp.Actor))};
                    }));

                Receive<RespondRegistration>(obs => obs
                    .Do(r => Log.Info($"Set Killswitch Actor Type {r.Event.RecpientType} {Sender.Path}"))
                    .Select(r =>
                    {
                        var (respondRegistration, state, _) = r;
                        var ele = state.Actors.Find(e => e.Target.Equals(Sender));
                        if (ele == null)
                            return state;

                        return state with
                        {
                            Actors = state.Actors.Replace(ele,
                                ele with {RecpientType = respondRegistration.RecpientType})
                        };
                    }));

                Receive<RequestRegistration>(obs => obs.Select(_ => new RespondRegistration(KillRecpientType.Seed))
                    .ToSender());

                Receive<KillClusterMsg>(obs => obs.SubscribeWithStatus(_ => RunKillCluster()));

                Receive<KillNode>(obs => obs.Do(_ => Log.Info("Leaving Cluster"))
                    .SubscribeWithStatus(_ => Cluster.Get(Context.System).LeaveAsync()));

                Receive<ActorUp>(obs => obs.Where(m => m.Event.Tag == nameof(KillWatcher))
                    .Do(up => Log.Info($"Incomming Kill Watcher {up.Event.Actor.Path}"))
                    .Select(m => m.Event.Actor)
                    .SubscribeWithStatus(actor => Context.Watch(actor)));

                Receive<Terminated>(obs => obs.Select(t => new ActorDown(t.Event.ActorRef, nameof(KillWatcher)))
                    .ToSelf());

                Start.Subscribe(_ =>
                {
                    var actorDiscovery = ClusterActorDiscovery.Get(ObservableActor.Context.System).Discovery;

                    actorDiscovery.Tell(new RegisterActor(Self, KillSwitchName));
                    actorDiscovery.Tell(new MonitorActor(nameof(KillSwitchName)));
                });
            }

            private void RunKillCluster()
            {
                Log.Info("Begin Cluster Shutdown");

                var dic = new GroupDictionary<KillRecpientType, IActorRef>
                {
                    KillRecpientType.Unkowen,
                    KillRecpientType.Frontend,
                    KillRecpientType.Host,
                    KillRecpientType.Seed,
                    KillRecpientType.Service
                };

                foreach (var (target, recpientType) in CurrentState.Actors)
                    dic.Add(recpientType, target);

                foreach (var recpientType in Order)
                {
                    var actors = dic[recpientType];
                    Log.Info("Tell Shutdown {Type} -- {Count}", recpientType, actors.Count);
                    foreach (var actorRef in actors)
                        actorRef.Tell(new KillNode());
                }
            }

            public sealed record KillState(ImmutableList<ActorElement> Actors);


            public sealed record ActorElement(IActorRef Target,
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
}