﻿using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Dsl;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Internal;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Operations;

namespace ServiceManager.ServiceDeamon.ConfigurationServer
{
    public class ConfigQueryFeature : ReportingActor<ConfigFeatureConfiguration>
    {
        protected override void ConfigImpl()
        {
            var mat = Context.Materializer();
            var source = Source.FromObservable(
                from evt in CurrentState.EventPublisher
                where CurrentState.Configugration.MonitorChanges
                select evt);
            var hubSource = source.ToMaterialized(BroadcastHub.Sink<IConfigEvent>(), Keep.Right).Run(mat);

            (from evt in CurrentState.EventPublisher
             where evt is ServerConfigurationEvent
             from pair in UpdateAndSyncActor((ServerConfigurationEvent)evt)
             select pair.State with { Configugration = pair.Event.Configugration }
                ).AutoSubscribe(UpdateState).DisposeWith(this);

            TryReceive<QueryConfigEventSource>(
                nameof(QueryConfigEventSource),
                obs => from _ in obs
                       select OperationResult.Success(new ConfigEventSource(hubSource)));

            TryReceive<QueryServerConfiguration>(
                nameof(QueryServerConfiguration),
                obs => from _ in obs
                       select OperationResult.Success(CurrentState.Configugration));

            TryReceive<QuerySeedUrls>(
                nameof(QuerySeedUrls),
                obs => from _ in obs
                       from urls in Task.Run(() => CurrentState.Seeds.GetAll())
                       select OperationResult.Success(new SeedUrls(urls.Select(u => u.Url).ToImmutableList())));

            TryReceive<QueryGlobalConfig>(
                nameof(QueryGlobalConfig),
                obs => from _ in obs
                       from config in Task.Run(() => CurrentState.GlobalRepository.Get(GlobalConfigEntity.EntityId))
                       select config == null
                           ? OperationResult.Failure(ConfigError.NoGlobalConfigFound)
                           : OperationResult.Success(config.Config));

            TryReceive<QuerySpecificConfig>(
                nameof(QuerySpecificConfig),
                obs => from req in obs
                       from sc in Task.Run(() => CurrentState.Apps.Get(req.Event.Id))
                       select sc == null
                           ? OperationResult.Failure(ConfigError.SpecificConfigurationNotFound)
                           : OperationResult.Success(sc.Config));

            TryReceive<QuerySpecificConfigList>(
                nameof(QuerySpecificConfigList),
                obs => from r in obs
                       from configs in Task.Run(
                           () => r.State.Apps.GetAll()
                              .Select(e => e.Config)
                              .ToImmutableList())
                       select OperationResult.Success(new SpecificConfigList(configs)));

            string MakeConfig(GlobalConfigEntity? root, ImmutableList<SpecificConfig> specific)
            {
                Config MergeSpecific()
                    => specific.Aggregate(
                        Akka.Configuration.Config.Empty,
                        (config, specificConfig) => config.WithFallback(ConfigurationFactory.ParseString(specificConfig.ConfigContent)));

                if (root is not null && !specific.IsEmpty)
                    return MergeSpecific().WithFallback(ConfigurationFactory.ParseString(root.Config.ConfigContent))
                       .ToString(includeFallback: true);

                if (root is null && !specific.IsEmpty)
                    return MergeSpecific().ToString(includeFallback: true);

                if (root is not null && specific.IsEmpty)
                    return root.Config.ConfigContent;


                return string.Empty;
            }

            TryReceive<QueryFinalConfigData>(
                nameof(QueryFinalConfigData),
                obs => from request in obs
                       from global in Task.Run(() => CurrentState.GlobalRepository.Get(GlobalConfigEntity.EntityId))
                       from spec in Task.Run(
                           () => (
                               from entity in CurrentState.Apps.GetAll()
                               where ConditionChecker.MeetCondition(request.Event.SoftwareName, request.Event.ApplicationName.Value, entity.Config)
                               select entity.Config).ToImmutableList())
                       select OperationResult.Success(new FinalAppConfig(MakeConfig(global, spec))));
        }
    }
}