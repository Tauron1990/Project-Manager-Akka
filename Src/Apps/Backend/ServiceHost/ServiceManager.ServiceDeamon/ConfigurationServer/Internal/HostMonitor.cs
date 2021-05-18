using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Akka.Actor;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using Tauron;
using Tauron.Akka;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Features;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed class HostMonitor : ActorFeatureBase<HostMonitor.State>
    {
        public record State(string Name, IObservable<IConfigEvent> Publisher, ServerConfigugration ServerConfigugration, ImmutableList<HostApp> Apps, HostApi Api);

        public static IPreparedFeature New(string name, IObservable<IConfigEvent> publisher, ServerConfigugration serverConfigugration)
            => Feature.Create(() => new HostMonitor(), c =>  new State(name, publisher, serverConfigugration, ImmutableList<HostApp>.Empty, HostApi.CreateOrGet(c.System)));

        protected override void ConfigImpl()
        {
            var root = CurrentState.Publisher.ObserveOnSelf();
            var api = CurrentState.Api;

            Unit MakeInstallationSubscription()
            {
                CurrentState.Api.ExecuteCommand(new SubscribeInstallationCompled(CurrentState.Name, false))
                   .ToObservable()
                   .Subscribe(
                        r =>
                        {
                            r.Subscription.DisposeWith(this);
                            Log.Info("Installation Subscription Compled");
                        },
                        e =>
                        {
                            Log.Warning(e, "Installation Subscrion Failed {Name}", CurrentState.Name);
                            Timers.StartSingleTimer(ReSheduleInstallEvent.Inst, ReSheduleInstallEvent.Inst, TimeSpan.FromMinutes(1));
                        });
                return Unit.Default;
            }

            Receive<ReSheduleInstallEvent>(
                obs => from request in obs
                       from apps in api.ExecuteCommand(new QueryHostApps(CurrentState.Name))
                       let _ = MakeInstallationSubscription()
                       from syncApps in UpdateAndSyncActor(request.NewEvent(apps))
                       select syncApps.State with {Apps = syncApps.Event.Apps});

            (
                from evt in root.OfType<ServerConfigurationEvent>()
                from pair in UpdateAndSyncActor(evt.Configugration)
                select pair.State with {ServerConfigugration = pair.Event}
            ).AutoSubscribe(UpdateState).DisposeWith(this);
            



            Self.Tell(ReSheduleInstallEvent.Inst);
        }

        private sealed record ReSheduleInstallEvent
        {
            public static readonly ReSheduleInstallEvent Inst = new();
        }
    }
}