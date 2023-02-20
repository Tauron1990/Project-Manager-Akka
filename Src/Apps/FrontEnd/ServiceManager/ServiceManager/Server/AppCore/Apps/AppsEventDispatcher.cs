using System;
using System.Threading.Tasks;
using Akka;
using Akka.Hosting;
using Akka.Streams.Dsl;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Features;
using UnitsNet;

namespace ServiceManager.Server.AppCore.Apps
{
    public sealed class AppEventDispatcher : AggregateEvent<AppInfo> { }

    [UsedImplicitly]
    public sealed class AppEventDispatcherRef : EventDispatcherRef<AppEventDispatcherRef>, IEventDispatcher
    {
        public AppEventDispatcherRef(IRequiredActor<AppEventDispatcherRef> actor) : base(actor)
        {
        }
    }

    public sealed class AppEventDispatcherActor : RestartingEventDispatcherActorBase<AppInfo, DeploymentApi>
    {
        public static Func<AppEventDispatcher, DeploymentApi, ILogger<AppEventDispatcherActor>, IPreparedFeature> New()
        {
            IPreparedFeature _(AppEventDispatcher aggregator, DeploymentApi api, ILogger<AppEventDispatcherActor> logger)
                => Feature.Create(() => new AppEventDispatcherActor(logger), _ => new State(api, ExecuteEventSourceQuery, aggregator));

            return _;
        }

        private static async Task<Source<AppInfo, NotUsed>> ExecuteEventSourceQuery(DeploymentApi api)
        {
            var response = await api.Query<QueryAppChangeSource, AppChangedSource>(new ApiParameter(Duration.FromSeconds(20)));

            return response.Fold(d => d.AppSource, err => throw new InvalidOperationException(err.Info ?? err.Code));
        }

        public AppEventDispatcherActor(ILogger<AppEventDispatcherActor> logger) : base(logger)
        {
        }
    }
}