using System;
using System.Threading.Tasks;
using Akka;
using Akka.Hosting;
using Akka.Streams.Dsl;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Features;
using UnitsNet;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed class ConfigEventDispatcher : AggregateEvent<IConfigEvent> { }

    [UsedImplicitly]
    public sealed class ConfigApiEventDispatcherRef : EventDispatcherRef<ConfigApiEventDispatcherRef>, IEventDispatcher
    {
        public ConfigApiEventDispatcherRef(IRequiredActor<ConfigApiEventDispatcherRef> actor) : base(actor)
        {
        }
    }

    public sealed class ConfigApiEventDispatcherActor : RestartingEventDispatcherActorBase<IConfigEvent, ConfigurationApi>
    {
        public static Func<ConfigEventDispatcher, ConfigurationApi, ILogger<ConfigApiEventDispatcherActor>, IPreparedFeature> New()
        {
            IPreparedFeature _(ConfigEventDispatcher aggregator, ConfigurationApi api, ILogger<ConfigApiEventDispatcherActor> logger)
                => Feature.Create(() => new ConfigApiEventDispatcherActor(logger), _ => new State(api, ExecuteEventSourceQuery, aggregator));

            return _;
        }

        private static async Task<Source<IConfigEvent, NotUsed>> ExecuteEventSourceQuery(ConfigurationApi api)
        {
            var response = await api.Query<QueryConfigEventSource, ConfigEventSource>(new ApiParameter(Duration.FromSeconds(20)));

            return response.Fold(r => r.Source, err => throw new InvalidOperationException(err.Info ?? err.Code));
        }

        public ConfigApiEventDispatcherActor(ILogger<ConfigApiEventDispatcherActor> logger) : base(logger)
        {
        }
    }
}