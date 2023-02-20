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
using Tauron.Application.AkkaNode.Services.Core;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Commands;
using Tauron.Application.Master.Commands.Deployment.Build.Data;
using Tauron.Application.Master.Commands.Deployment.Build.Querys;
using Tauron.Application.Master.Commands.Deployment.Repository;
using UnitsNet;

namespace ServiceManager.Server.AppCore.Apps
{
    [UsedImplicitly]
    public class AppManagment : IAppManagment, IDisposable
    {
        private readonly ILogger<AppManagment> _log;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDisposable _subscription;
        private readonly ActorSystem _system;

        public AppManagment(IServiceScopeFactory scopeFactory, ActorSystem system, AppEventDispatcher dispatcher, ILogger<AppManagment> log)
        {
            _scopeFactory = scopeFactory;
            _system = system;
            _log = log;

            _subscription = dispatcher.Get().AutoSubscribe(
                ai =>
                {
                    using (Computed.Invalidate())
                    {
                        NeedBasicApps(CancellationToken.None).Ignore();
                        QueryAllApps(CancellationToken.None).Ignore();
                        QueryApp(ai.Name, CancellationToken.None).Ignore();
                    }
                },
                e => log.LogError(e, "Error on Process App Event"));
        }

        public virtual Task<NeedSetupData> NeedBasicApps(CancellationToken token)
            => Run(NeedBasicAppsImpl, token);

        public virtual Task<AppList> QueryAllApps(CancellationToken token)
            => Run(QueryAllAppsImpl, token);

        public virtual Task<AppInfo> QueryApp(AppName name, CancellationToken token)
            => Run(name, QueryAppImpl, token);

        public virtual async Task<QueryRepositoryResult> QueryRepository(RepositoryName name, CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<ProcessServiceHostRef>();
            var api = scope.ServiceProvider.GetRequiredService<RepositoryApi>();
            
            
            await EnsureApi(host, api);

            var result = await api.Send(new RegisterRepository(name, true), Duration.FromSeconds(30), static _ => {}, token);

            if(result.HasValue)
                return new QueryRepositoryResult(false, result.Value.Info ?? result.Value.Code);

            return new QueryRepositoryResult(true, string.Empty);
        }

        public virtual Task<string> CreateNewApp(ApiCreateAppCommand command, CancellationToken token)
            => Run(command, CreateNewAppImpl, token);

        public virtual Task<string> DeleteAppCommand(ApiDeleteAppCommand command, CancellationToken token)
            => Run(command, DeleteAppImpl, token);

        public virtual Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command, CancellationToken token)
            => Run(command, RunSetupImpl, token);

        public void Dispose()
            => _subscription.Dispose();

        private async Task<TResult> Run<TResult>(AppApiRunner<TResult> runner, CancellationToken token)
        {
            if (Computed.IsInvalidating()) return default!;

            using var scope = _scopeFactory.CreateScope();
            var host = scope.ServiceProvider.GetRequiredService<ProcessServiceHostRef>();
            var api = scope.ServiceProvider.GetRequiredService<DeploymentApi>();

            await EnsureApi(host, api);

            return await runner(host, api, scope.ServiceProvider, token);
        }

        private async Task<TResult> Run<TResult, TParam>(TParam command, AppApiRunnerParam<TResult, TParam> runner, CancellationToken token)
        {
            Task<TResult> RunLocal(ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken innerToken)
                => runner(command, host, api, services, innerToken);

            return await Run(RunLocal, token);
        }


        private async Task EnsureApi(ProcessServiceHostRef host, IQueryIsAliveSupport deploymentApi)
        {
            var response = await deploymentApi.QueryIsAlive(_system, TimeSpan.FromSeconds(20));

            if (response.IsAlive) return;

            var (isRunning, message) = await host.TryStart(null);

            if (isRunning) return;

            throw new InvalidOperationException(message);
        }

        private async Task<string> DeleteAppImpl(ApiDeleteAppCommand command, ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var result = await api.Command<DeleteAppCommand, AppInfo>(new DeleteAppCommand(command.Name), new ApiParameter(Duration.FromSeconds(20), token));

            return result.ErrorToString();
        }
        
        private async Task<string> CreateNewAppImpl(ApiCreateAppCommand command, ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            try
            {
                var (appName, projectName, repositoryName) = command;
                var result = await api.Command<CreateAppCommand, AppInfo>(
                    new CreateAppCommand(appName, repositoryName, projectName),
                    new ApiParameter(Duration.FromSeconds(20), token));

                return result.Fold(_ => string.Empty, err => err.Info ?? err.Code);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Create new App");

                return e.Message;
            }
        }

        private async Task<AppList> QueryAllAppsImpl(ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var queryResult = await api.Query<QueryApps, AppList>(new ApiParameter(Duration.FromSeconds(20), token));

            return queryResult.Fold(l => l, err => throw new InvalidOperationException(err.Info ?? err.Code));
        }

        private async Task<AppInfo> QueryAppImpl(AppName command, ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            var queryResult = await api.Query<QueryApp, AppInfo>(new QueryApp(command), new ApiParameter(Duration.FromSeconds(20), token));

            return queryResult.Fold(
                ai => ai,
                err => AppInfo.Empty with
                       {
                           Deleted = true,
                           Name = AppName.From(err.Info ?? err.Code),
                       });
        }

        private async Task<RunAppSetupResponse> RunSetupImpl(RunAppSetupCommand command, ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
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
                        new ApiParameter(Duration.FromSeconds(20), token, MessageSender));

                return result.Fold(
                    _ => DefaultApps.Apps.Length - 1 == command.Step
                        ? new RunAppSetupResponse(-1, null, IsCompled: true, "Setup Fertig")
                        : new RunAppSetupResponse(command.Step + 1, null, IsCompled: false, $"Anwednug {name} wurde erstellt"),
                    err => err.Code == DeploymentErrorCode.CommandDuplicateApp
                        ? new RunAppSetupResponse(command.Step + 1, null, IsCompled: false, $"Anwendung {name} ist schon Erstellt")
                        : new RunAppSetupResponse(-1, err.Code, IsCompled: true, "Schwerer Fehler"));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Run app setup on {Step}", command.Step);

                return new RunAppSetupResponse(-1, e.Message, IsCompled: true, "Schwerer Fehler");
            }
        }

        private async Task<NeedSetupData> NeedBasicAppsImpl(ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token)
        {
            try
            {
                var queryResult = await api.Query<QueryApps, AppList>(new ApiParameter(Duration.FromSeconds(20), token));

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

        private delegate Task<TResult> AppApiRunner<TResult>(ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token);

        private delegate Task<TResult> AppApiRunnerParam<TResult, in TParam>(TParam command, ProcessServiceHostRef host, DeploymentApi api, IServiceProvider services, CancellationToken token);
    }
}