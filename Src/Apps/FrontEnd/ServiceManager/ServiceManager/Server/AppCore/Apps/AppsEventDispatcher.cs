using System;
using System.Threading.Tasks;
using Akka;
using Akka.Streams.Dsl;
using ServiceManager.Server.AppCore.Helper;
using Tauron.Application;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Features;

namespace ServiceManager.Server.AppCore.Apps
{
    public sealed class AppEventDispatcher : AggregateEvent<AppInfo> { }

    public sealed class AppEventDispatcherRef : EventDispatcherRef
    {
        public AppEventDispatcherRef() : base(nameof(AppEventDispatcherRef))
        {
        }
    }

    public sealed class AppEventDispatcherActor : RestartingEventDispatcherActorBase<AppInfo, DeploymentApi>
    {
        public static Func<AppEventDispatcher, DeploymentApi, IPreparedFeature> New()
        {
            IPreparedFeature _(AppEventDispatcher aggregator, DeploymentApi api)
                => Feature.Create(() => new AppEventDispatcherActor(), _ => new State(api, ExecuteEventSourceQuery, aggregator));

            return _;
        }

        private static async Task<Source<AppInfo, NotUsed>> ExecuteEventSourceQuery(DeploymentApi api)
        {
            var response = await api.Query<QueryAppChangeSource, AppChangedSource>(TimeSpan.FromSeconds(20));

            return response.AppSource;
        }
    }
}