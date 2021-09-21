using System;
using System.Threading.Tasks;
using Akka;
using Akka.Streams.Dsl;
using JetBrains.Annotations;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed class ConfigEventDispatcher : AggregateEvent<IConfigEvent> { }

    [UsedImplicitly]
    public sealed class ConfigApiEventDispatcherRef : EventDispatcherRef
    {
        public ConfigApiEventDispatcherRef() : base(nameof(ConfigApiEventDispatcherActor)) { }
    }

    public sealed class ConfigApiEventDispatcherActor : RestartingEventDispatcherActorBase<IConfigEvent, ConfigurationApi>
    {
        public static Func<ConfigEventDispatcher, ConfigurationApi, IPreparedFeature> New()
        {
            IPreparedFeature _(ConfigEventDispatcher aggregator, ConfigurationApi api)
                => Feature.Create(() => new ConfigApiEventDispatcherActor(), _ => new State(api, ExecuteEventSourceQuery, aggregator));

            return _;
        }

        private static async Task<Source<IConfigEvent, NotUsed>> ExecuteEventSourceQuery(ConfigurationApi api)
        {
            var response = await api.Query<QueryConfigEventSource, ConfigEventSource>(new ApiParameter(TimeSpan.FromSeconds(20)));

            return response.Fold(r => r.Source, err => throw new InvalidOperationException(err.Info ?? err.Code));
        }
    }
}