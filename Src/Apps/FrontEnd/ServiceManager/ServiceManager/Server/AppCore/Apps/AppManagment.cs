using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Apps;
using Stl.Fusion;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;

namespace ServiceManager.Server.AppCore.Apps
{
    [UsedImplicitly]
    public class AppManagment : IAppManagment, IDisposable
    {
        private delegate Task<TResult> AppApiRunner<TResult>(IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token);

        private delegate Task<TResult> AppApiRunnerParam<TResult, in TParam>(TParam command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token);
        
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
                        NeedBasicApps(CancellationToken.None).Ignore();
                        QueryAllApps(CancellationToken.None).Ignore();
                    }
                }, e => log.LogError(e, "Error on Process App Event"));
        }

        private async Task<TResult> Run<TResult>(AppApiRunner<TResult> runner, CancellationToken token)
        {
            if (Computed.IsInvalidating()) return default!;
            
            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<IProcessServiceHost>();
            var api = scope.ServiceProvider.GetRequiredService<DeploymentApi>();

            await EnsureDeploymentApi(host, api);
            return await runner(host, api, scope.ServiceProvider, token);
        }
        
        private async Task<TResult> Run<TResult, TParam>(TParam command, AppApiRunnerParam<TResult, TParam> runner, CancellationToken token)
        {
            Task<TResult> RunLocal(IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken innerToken)
                => runner(command, host, api, services, innerToken);

            return await Run(RunLocal, token);
        }
        

        private async Task EnsureDeploymentApi(IProcessServiceHost host, DeploymentApi deploymentApi)
        {
            var response = await deploymentApi.QueryIsAlive(_system, TimeSpan.FromSeconds(20));
            if(response.IsAlive) return;

            var (isRunning, message) = await host.TryStart(null);
            if(isRunning) return;
            
            throw new InvalidOperationException(message);
        }
        
        public virtual Task<NeedSetupData> NeedBasicApps(CancellationToken token)
            => Run(NeedBasicAppsImpl, token);

        public virtual Task<AppList> QueryAllApps(CancellationToken token)
            => Run(QueryAllAppsImpl, token);

        public virtual Task<AppInfo> QueryApp(string name, CancellationToken token)
            => Run(name, QueryAppImpl, token);

        public virtual Task<string> CreateNewApp(ApiCreateAppCommand command, CancellationToken token)
            => Run(command, CreateNewAppImpl, token);

        public virtual Task<string> DeleteAppCommand(ApiDeleteAppCommand command, CancellationToken token)
            => throw new NotSupportedException("Not supportet");
        
        public virtual Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command, CancellationToken token)
            => Run(command, RunSetupImpl, token);

        private async Task<string> CreateNewAppImpl(ApiCreateAppCommand command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            try
            {
                var result = await api.Command<CreateAppCommand, AppInfo>(new CreateAppCommand(command.Name, command.ProjectName, command.RepositoryName), 
                    new ApiParameter(TimeSpan.FromSeconds(20), token));

                return result.Fold(_ => string.Empty, err => err.Info ?? err.Code);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Create new App");
                return e.Message;
            }
        }
        
        private async Task<AppList> QueryAllAppsImpl(IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var queryResult = await api.Query<QueryApps, AppList>(new ApiParameter(TimeSpan.FromSeconds(20), token));

            return queryResult.Fold(l => l, err => throw new InvalidOperationException(err.Info ?? err.Code));
        }

        private async Task<AppInfo> QueryAppImpl(string command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var queryResult = await api.Query<QueryApp, AppInfo>(new QueryApp(command), new ApiParameter(TimeSpan.FromSeconds(20), token));

            return queryResult.Fold(ai => ai, err => AppInfo.Empty with
                                                     {
                                                         Deleted = true,
                                                         Name = err.Info ?? err.Code
                                                     });
        }

        private async Task<RunAppSetupResponse> RunSetupImpl(RunAppSetupCommand command, IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var hub = services.GetRequiredService<IHubContext<ClusterInfoHub>>();
            
            async void MessageSender(string msg)
            {
                try
                {
                    await hub.Clients.All.SendAsync(command.OperationId, msg, token);
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error on Sending Message to Client");
                }
            }
            
            try
            {
                var (name, repository, projectName) = DefaultApps.Apps[command.Step];

                var result =
                    await api.Command<CreateAppCommand, AppInfo>(
                    new CreateAppCommand(name, repository, projectName),
                    new ApiParameter(TimeSpan.FromSeconds(20), token, MessageSender));

                return result.Fold(
                    _ => DefaultApps.Apps.Length - 1 == command.Step
                        ? new RunAppSetupResponse(-1, null, IsCompled: true, "Setup Fertig")
                        : new RunAppSetupResponse(command.Step + 1, null, IsCompled: false, $"Anwednug {name} wurde erstellt"),
                    err => err.Code == DeploymentErrorCodes.CommandDuplicateApp
                        ? new RunAppSetupResponse(command.Step + 1, null, IsCompled: false, $"Anwendung {name} ist schon Erstellt")
                        : new RunAppSetupResponse(-1, err.Code, IsCompled: true, "Schwerer Fehler"));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Run app setup on {Step}", command.Step);

                return new RunAppSetupResponse(-1, e.Message, IsCompled: true, "Schwerer Fehler");
            }
        }

        private async Task<NeedSetupData> NeedBasicAppsImpl(IProcessServiceHost host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            try
            {
                var queryResult = await api.Query<QueryApps, AppList>(new ApiParameter(TimeSpan.FromSeconds(20), token));

                return queryResult.Fold(
                    apps => new NeedSetupData(null, DefaultApps.Apps.Any(c => apps.Apps.All(i => i.Name != c.Name))),
                    err => new NeedSetupData(err.Info ?? err.Code, Need: false));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Fetch App Infos");
                return new NeedSetupData(e.Message, Need: false);
            }
        }
        
        public void Dispose()
            => _subscription.Dispose();
    }
}