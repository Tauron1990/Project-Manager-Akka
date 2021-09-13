using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Apps;
using Stl.Async;
using Stl.Fusion;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;

namespace ServiceManager.Server.AppCore.Apps
{
    public class AppManagment : IAppManagment, IDisposable
    {
        private delegate Task<TResult> AppApiRunner<TResult>(IProcessServiceHost host, DeploymentApi api, IServiceProvider services);

        private delegate Task<TResult> AppApiRunnerParam<TResult, TParam>(TParam command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services);
        
        private readonly IServiceScopeFactory  _scopeFactory;
        private readonly ActorSystem           _system;
        private readonly ILogger<AppManagment> _log;
        private readonly IDisposable           _subscription;
        
        public AppManagment(IServiceScopeFactory scopeFactory, ActorSystem system, AppEventDispatcher dispatcher, ILogger<AppManagment> log)
        {
            _scopeFactory = scopeFactory;
            _system       = system;
            _log     = log;

            _subscription = dispatcher.Get().AutoSubscribe(
                _ =>
                {
                    using (Computed.Invalidate())
                    {
                        NeedBasicApps().Ignore();
                        QueryAllApps().Ignore();
                    }
                }, e => log.LogError(e, "Error on Process App Event"));
        }

        private async Task<TResult> Run<TResult>(AppApiRunner<TResult> runner)
        {
            if (Computed.IsInvalidating()) return default!;
            
            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<IProcessServiceHost>();
            var api = scope.ServiceProvider.GetRequiredService<DeploymentApi>();

            await EnsureDeploymentApi(host, api);
            return await runner(host, api, scope.ServiceProvider);
        }
        
        private async Task<TResult> Run<TResult, TParam>(TParam command, AppApiRunnerParam<TResult, TParam> runner)
        {
            Task<TResult> RunLocal(IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
                => runner(command, host, api, services);

            return await Run(RunLocal);
        }
        

        private async Task EnsureDeploymentApi(IProcessServiceHost host, DeploymentApi deploymentApi)
        {
            var response = await deploymentApi.QueryIsAlive(_system, TimeSpan.FromSeconds(20));
            if(response.IsAlive) return;

            var (isRunning, message) = await host.TryStart(null);
            if(isRunning) return;
            
            throw new InvalidOperationException(message);
        }
        
        public virtual Task<NeedSetupData> NeedBasicApps()
            => Run(NeedBasicAppsImpl);

        public virtual Task<AppList> QueryAllApps()
            => Run(QueryAllAppsImpl);

        public virtual Task<AppInfo> QueryApp(string name)
            => Run(name, QueryAppImpl);

        public virtual Task<string> CreateNewApp(ApiCreateAppCommand command)
            => Run(command, CreateNewAppImpl);

        public virtual Task<string> DeleteAppCommand(ApiDeleteAppCommand command)
            => run;
        
        public virtual Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command)
            => Run(command, RunSetupImpl);

        private async Task<string> CreateNewAppImpl(ApiCreateAppCommand command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
        {
            try
            {
                await api.Command<CreateAppCommand, AppInfo>(new CreateAppCommand(command.Name, command.ProjectName, command.RepositoryName), TimeSpan.FromSeconds(20));

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        
        private Task<AppList> QueryAllAppsImpl(IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
            => api.Query<QueryApps, AppList>(TimeSpan.FromSeconds(20));

        private Task<AppInfo> QueryAppImpl(string command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
            => api.Query<QueryApp, AppInfo>(new QueryApp(command), TimeSpan.FromSeconds(20));
        
        private async Task<RunAppSetupResponse> RunSetupImpl(RunAppSetupCommand command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
        {
            string name = string.Empty;
            var hub = services.GetRequiredService<IHubContext<ClusterInfoHub>>();
            
            async void MessageSender(string msg)
            {
                try
                {
                    await hub.Clients.All.SendAsync(command.OperationId, msg);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error on Sending Message to Client");
                }
            }
            
            try
            {
                var appConfig = DefaultApps.Apps[command.Step];
                name = appConfig.Name;

                await api.Command<CreateAppCommand, AppInfo>(
                    new CreateAppCommand(appConfig.Name, appConfig.Repository, appConfig.ProjectName), 
                    TimeSpan.FromSeconds(20),
                    MessageSender);

                return DefaultApps.Apps.Length - 1 == command.Step
                    ? new RunAppSetupResponse(-1, null, true, "Setup Fertig")
                    : new RunAppSetupResponse(command.Step + 1, null, false, $"Anwednug {name} wurde erstellt");
            }
            catch (Exception e)
            {
                if (e is CommandFailedException { Message: DeploymentErrorCodes.CommandDuplicateApp })
                    return new RunAppSetupResponse(command.Step + 1, null, false, $"Anwendung {name} ist schon Erstellt");
                
                _log.LogError(e, "Error on Run app setup on {Step}", command.Step);

                return new RunAppSetupResponse(-1, e.Message, true, "Schwerer Fehler");
            }
        }

        private async Task<NeedSetupData> NeedBasicAppsImpl(IProcessServiceHost host, DeploymentApi api, IServiceProvider services)
        {
            try
            {
                var apps = await api.Query<QueryApps, AppList>(TimeSpan.FromSeconds(20));

                return new NeedSetupData(null, DefaultApps.Apps.Any(c => apps.Apps.All(i => i.Name != c.Name)));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Fetch App Infos");
                return new NeedSetupData(e.Message, false);
            }
        }
        
        public void Dispose()
            => _subscription.Dispose();
    }
}