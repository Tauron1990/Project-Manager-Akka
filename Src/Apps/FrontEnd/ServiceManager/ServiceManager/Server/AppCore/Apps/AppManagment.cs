using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Shared.Apps;
using Stl.Async;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;

namespace ServiceManager.Server.AppCore.Apps
{
    public class AppManagment : IAppManagment, IDisposable
    {
        private delegate Task<TResult> AppApiRunner<TResult>(IProcessServiceHost host, DeploymentApi api, IServiceProvider services);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ActorSystem _system;
        private readonly IDisposable _subscription;
        
        public AppManagment(IServiceScopeFactory scopeFactory, ActorSystem system, AppEventDispatcher dispatcher, ILogger<AppManagment> log)
        {
            _scopeFactory = scopeFactory;
            _system = system;

            _subscription = dispatcher.Get().AutoSubscribe(
                _ =>
                {
                    using(Computed.Invalidate())
                        NeedBasicApps().Ignore();
                }, e => log.LogError(e, "Error on Process App Event"));
        }

        private async Task<TResult> Run<TResult>(AppApiRunner<TResult> runner)
        {
            if (Computed.IsInvalidating()) return default!;
            
            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<IProcessServiceHost>();
            var api = scope.ServiceProvider.GetRequiredService<DeploymentApi>();

            return await runner(host, api, scope.ServiceProvider);
        }

        private async Task EnsureDeploymentApi(IProcessServiceHost host, DeploymentApi deploymentApi)
        {
            var response = await deploymentApi.QueryIsAlive(_system, TimeSpan.FromSeconds(20));
            if(response.IsAlive) return;

            var (isRunning, message) = await host.TryStart(null);
            if(isRunning) return;
            
            throw new InvalidOperationException(message);
        }
        
        public virtual Task<NeedSetupData> NeedBasicApps(CancellationToken token = default)
            => Run(NeedBasicAppsImpl);

        private async Task<NeedSetupData> NeedBasicAppsImpl(IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
        {
            try
            {
                await EnsureDeploymentApi(host, api);
                var apps = await api.Query<QueryApps, AppList>(TimeSpan.FromSeconds(20));

                return new NeedSetupData(null, DefaultApps.Apps.Any(c => apps.Apps.All(i => i.Name != c.Name)));
            }
            catch (Exception e)
            {
                return new NeedSetupData(e.Message, false);
            }
        }


        public void Dispose()
            => _subscription.Dispose();
    }
}