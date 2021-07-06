using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using Tauron;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.Helper
{
    public sealed class ConfigEventDispatcher : SharedEvent<IConfigEvent> { }

    public interface IApiEventDispatcher : IFeatureActorRef<IApiEventDispatcher>
    {
        void Init();
    }

    public sealed class ApiEventDispatcherRef : FeatureActorRefBase<IApiEventDispatcher>, IApiEventDispatcher
    {
        public ApiEventDispatcherRef() : base(nameof(ApiEventDispatcherActor))
        {
        }

        public void Init() => Tell(new InitActor());
    }

    public sealed class ApiEventDispatcherActor : ActorFeatureBase<ApiEventDispatcherActor.State>
    {
        public sealed record State(ConfigurationApi Configuration, ConfigEventDispatcher ConfigEventDispatcher);

        public static Func<ConfigEventDispatcher, ConfigurationApi, IPreparedFeature> New()
        {
            IPreparedFeature _(ConfigEventDispatcher aggregator, ConfigurationApi api)
                => Feature.Create(() => new ApiEventDispatcherActor(), _ => new State(api, aggregator));

            return _;
        }

        protected override void ConfigImpl()
        {
            var materializer = Context.Materializer();

            Receive<InitActor>(obs => obs.ToUnit(p => p.Self.Tell(new TryQueryConfigEventSource())));

            Receive<TryQueryConfigEventSource>(obs => (from r in obs
                                                  let system = r.Context.System

                                                  let capi = r.State.Configuration
                                                  from cr in capi.QueryIsAlive(system, TimeSpan.FromSeconds(10))
                                                  from cs in capi.Query<QueryConfigEventSource, ConfigEventSource>(TimeSpan.FromSeconds(20))

                                                  select cs.Source)
                                             .AutoSubscribe(
                                                  s => s.RunForeach(evt => CurrentState.ConfigEventDispatcher.Publish(evt), materializer)
                                                        .ContinueWith(t =>
                                                                      {
                                                                          if (t.IsFaulted)
                                                                              Log.Warning(t.Exception, "Config Event Stream Failed");

                                                                          Self.Tell(new TryQueryConfigEventSource());
                                                                      }),
                                                  e =>
                                                  {
                                                      if (e is not TaskCanceledException)
                                                          Log.Error(e, "Error on get Event Sources from Deamons");
                                                      Timers.StartSingleTimer(nameof(TryQueryConfigEventSource), new TryQueryConfigEventSource(), TimeSpan.FromSeconds(30));
                                                  }));
        }

        private sealed record TryQueryConfigEventSource;
    }
}