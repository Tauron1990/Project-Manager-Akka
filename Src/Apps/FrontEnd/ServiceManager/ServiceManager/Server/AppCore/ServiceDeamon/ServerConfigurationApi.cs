using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ServiceHost.ClientApp.Shared.ConfigurationServer;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Events;
using ServiceManager.Server.Controllers;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;
using Tauron.Operations;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    [UsedImplicitly]
    public class ServerConfigurationApi : IServerConfigurationApi, IDisposable
    {
        private const string OperationCanceled = "Vorgang Abgebrochen";

        private readonly ConfigurationApi _api;
        private readonly CompositeDisposable _disposable = new();
        private readonly ILogger<ServerConfigurationApi> _log;
        private readonly IProcessServiceHost _processService;
        private readonly ActorSystem _system;

        public ServerConfigurationApi(
            ConfigurationApi api,
            ActorSystem system,
            ConfigEventDispatcher dispatcher,
            IProcessServiceHost processService,
            ILogger<ServerConfigurationApi> log)
        {
            _api = api;
            _system = system;
            _processService = processService;
            _log = log;

            dispatcher.Get().OfType<SpecificConfigEvent>()
               .ToUnit(
                    () =>
                    {
                        using (Computed.Invalidate())
                        {
                            QueryAppConfig().Ignore();
                        }
                    })
               .AutoSubscribe(e => log.LogError(e, "Error on Process ConfigEvent"))
               .DisposeWith(_disposable);


            dispatcher.Get().OfType<GlobalConfigEvent>()
               .ToUnit(
                    () =>
                    {
                        using (Computed.Invalidate())
                        {
                            GlobalConfig().Ignore();
                        }
                    })
               .AutoSubscribe(e => log.LogError(e, "Error on Process ConfigEvent"))
               .DisposeWith(_disposable);


            dispatcher.Get().OfType<ServerConfigurationEvent>()
               .ToUnit(
                    () =>
                    {
                        using (Computed.Invalidate())
                        {
                            ServerConfigugration().Ignore();
                        }
                    })
               .AutoSubscribe(e => log.LogError(e, "Error on Process ConfigEvent"))
               .DisposeWith(_disposable);
        }

        public void Dispose()
            => _disposable.Dispose();

        public virtual async Task<GlobalConfig> GlobalConfig()
        {
            if (Computed.IsInvalidating()) return default!;

            try
            {
                await EnsureConfigAlive();

                return (await _api.Query<QueryGlobalConfig, GlobalConfig>(new ApiParameter(TimeSpan.FromSeconds(10)))).GetOrThrow();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Query Global Config");

                if (e.Message == ConfigError.NoGlobalConfigFound)
                    return new GlobalConfig(string.Empty, null);

                throw;
            }
        }

        public virtual async Task<ServerConfigugration> ServerConfigugration()
        {
            if (Computed.IsInvalidating()) return default!;

            try
            {
                await EnsureConfigAlive();

                return (await _api.Query<QueryServerConfiguration, ServerConfigugration>(new ApiParameter(TimeSpan.FromSeconds(10)))).GetOrThrow();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Query ServerConfiguration");

                throw;
            }
        }

        public virtual async Task<ImmutableList<SpecificConfig>> QueryAppConfig()
        {
            if (Computed.IsInvalidating()) return default!;

            try
            {
                await EnsureConfigAlive();

                return (await _api.Query<QuerySpecificConfigList, SpecificConfigList>(new ApiParameter(TimeSpan.FromSeconds(10))))
                   .GetOrThrow().ConfigList;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Query Specifig Configs");

                throw;
            }
        }

        public virtual Task<string> QueryBaseConfig()
            => Task.FromResult(Resources.BaseConfig);

        public virtual Task<string?> QueryDefaultFileContent(ConfigOpensElement element)
            => Task.FromResult(ConfigurationController.GetConfigData(element));

        public virtual async Task<string> DeleteSpecificConfig(DeleteSpecificConfigCommand command, CancellationToken token = default)
        {
            try
            {
                await EnsureConfigAlive();

                if (token.IsCancellationRequested) return OperationCanceled;

                var result = await _api.Command(new UpdateSpecificConfigCommand(ConfigDataAction.Delete, command.Name, string.Empty, string.Empty, null), new ApiParameter(TimeSpan.FromSeconds(20)));

                return result.ErrorToString();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on delete Specifiv Config {Name}", command.Name);

                return e.Message;
            }
        }

        public virtual async Task<string> UpdateSpecificConfig(UpdateSpecifcConfigCommand command, CancellationToken token = default)
        {
            try
            {
                await EnsureConfigAlive();

                if (token.IsCancellationRequested) return OperationCanceled;

                var data = command.Data;

                var mode = data.IsNew ? ConfigDataAction.Create : ConfigDataAction.Update;

                var result = await _api.Command(
                    new UpdateSpecificConfigCommand(
                        mode,
                        data.Name,
                        data.Content,
                        data.Info,
                        data.Conditions?.Where(i => i.Action != ConfigDataAction.Delete).Select(i => i.Condition).ToArray()),
                    new ApiParameter(TimeSpan.FromSeconds(20)));

                //foreach (var (name, configDataAction, condition) in data.Conditions) 
                //await _api.Command(new UpdateConditionCommand(name, configDataAction, condition), TimeSpan.FromSeconds(20));

                return result.ErrorToString();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error while Update Specific Config Data");

                return e.Message;
            }
        }

        public virtual async Task<string> UpdateGlobalConfig(UpdateGlobalConfigApiCommand command, CancellationToken token = default)
        {
            var config = command.Config;

            if (config == await GlobalConfig())
                return "Die Konfiguration ist gleich";

            if (token.IsCancellationRequested) return OperationCanceled;

            var resonse = await _processService.ConfigAlive(_api, _system);

            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            if (token.IsCancellationRequested) return OperationCanceled;

            bool needUpdate;

            try
            {
                (await _api.Query<QueryGlobalConfig, GlobalConfig>(new ApiParameter(TimeSpan.FromSeconds(10)))).GetOrThrow();
                needUpdate = true;
            }
            catch (Exception e)
            {
                if (e.Message == ConfigError.NoGlobalConfigFound)
                    needUpdate = false;
                else
                    throw;
            }

            var res = await _api.Command(new UpdateGlobalConfigCommand(needUpdate ? ConfigDataAction.Update : ConfigDataAction.Create, config), new ApiParameter(TimeSpan.FromSeconds(20)));

            return res.ErrorToString();
        }

        public virtual async Task<string> UpdateServerConfig(UpdateServerConfiguration command, CancellationToken token = default)
        {
            var serverConfigugration = command.ServerConfigugration;

            if (serverConfigugration == await ServerConfigugration())
                return "Die Konfiguration ist gleich";

            if (token.IsCancellationRequested) return OperationCanceled;

            var resonse = await _processService.ConfigAlive(_api, _system);

            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            if (token.IsCancellationRequested) return OperationCanceled;

            var res = await _api.Command(new UpdateServerConfigurationCommand(serverConfigugration), new ApiParameter(TimeSpan.FromSeconds(20)));

            return res.ErrorToString();
        }

        private async Task EnsureConfigAlive()
        {
            var resonse = await _processService.ConfigAlive(_api, _system);

            if (!string.IsNullOrWhiteSpace(resonse))
                throw new InvalidOperationException(resonse);
        }
    }
}