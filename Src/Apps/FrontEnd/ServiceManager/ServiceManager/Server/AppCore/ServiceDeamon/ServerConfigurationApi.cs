using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using NLog;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed class ServerConfigurationApi : ObservableObject, IServerConfigurationApi, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ConfigurationApi _api;
        private readonly ActorSystem _system;
        private readonly IPropertyChangedNotifer _notifer;
        private readonly IProcessServiceHost _processService;
        private readonly IDisposable _subscriptions;

        private bool _isGlobalInitialized;
        private GlobalConfig _globalConfig = new(string.Empty, string.Empty);
        private bool _isServiceConfigInitialized;
        private ServerConfigugration _serverConfigugration = new(false, false, string.Empty);

        public GlobalConfig GlobalConfig
        {
            get => _globalConfig;
            set => SetProperty(ref _globalConfig, value, () => _notifer.SendPropertyChanged<IServerConfigurationApi>(nameof(GlobalConfig)));
        }

        public ServerConfigugration ServerConfigugration
        {
            get => _serverConfigugration;
            set => SetProperty(ref _serverConfigugration, value, () => _notifer.SendPropertyChanged<IServerConfigurationApi>(nameof(ServerConfigugration)));
        }

        public ServerConfigurationApi(ConfigurationApi api, ActorSystem system, ConfigEventDispatcher dispatcher, IPropertyChangedNotifer notifer, IProcessServiceHost processService)
        {
            _api = api;
            _system = system;
            _notifer = notifer;
            _processService = processService;
            _subscriptions = new CompositeDisposable(
                dispatcher.Get()
                          .OfType<GlobalConfigEvent>()
                          .Subscribe(gce =>
                                     {
                                         GlobalConfig = gce.Action == ConfigDataAction.Delete
                                             ? new GlobalConfig(string.Empty, string.Empty)
                                             : gce.Config;
                                     }),
                dispatcher.Get()
                          .OfType<ServerConfigurationEvent>()
                          .Subscribe(gce => ServerConfigugration = gce.Configugration));
        }

        public async Task<GlobalConfig> QueryConfig()
        {
            if (_isGlobalInitialized)
                return GlobalConfig;

            try
            {
                await EnsureConfigAlive();

                GlobalConfig = await _api.Query<QueryGlobalConfig, GlobalConfig>(TimeSpan.FromSeconds(10));

                _isGlobalInitialized = true;
                return GlobalConfig;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Query Global Config");
                if (e.Message == ConfigError.NoGlobalConfigFound)
                    return new GlobalConfig(string.Empty, null);
                throw;
            }
        }

        public async Task<ServerConfigugration> QueryServerConfig()
        {
            if (_isServiceConfigInitialized)
                return ServerConfigugration;

            try
            {
                await EnsureConfigAlive();

                ServerConfigugration = await _api.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));

                _isServiceConfigInitialized = true;
                return ServerConfigugration;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Query ServerConfiguration");
                throw;
            }
        }

        private async Task EnsureConfigAlive()
        {
            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                throw new InvalidOperationException(resonse);
        }

        public async Task<string> Update(GlobalConfig config)
        {
            if (config == GlobalConfig)
                return "Die Konfiguration ist gleich";

            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            bool needUpdate;

            try
            {
                await _api.Query<QueryGlobalConfig, GlobalConfig>(TimeSpan.FromSeconds(10));
                needUpdate = true;
            }
            catch (Exception e)
            {
                if (e.Message == ConfigError.NoGlobalConfigFound)
                    needUpdate = false;
                else
                    throw;
            }
            
            await _api.Command(new UpdateGlobalConfigCommand(needUpdate ? ConfigDataAction.Update : ConfigDataAction.Create, config), TimeSpan.FromSeconds(20));
            return string.Empty;
        }

        public Task<string> QueryBaseConfig()
            => Task.FromResult(Properties.Resources.BaseConfig);

        public Task<string> Update(ServerConfigugration serverConfigugration)
        {

        }

        public void Dispose() => _subscriptions.Dispose();
    }
}