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
        private readonly object _lock = new();

        private bool _isInitialized;
        private GlobalConfig _globalConfig = new(string.Empty, string.Empty);

        public GlobalConfig GlobalConfig
        {
            get => _globalConfig;
            set => SetProperty(ref _globalConfig, value, () => _notifer.SendPropertyChanged<IServerConfigurationApi>(nameof(GlobalConfig)));
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
                                         lock (_lock)
                                             GlobalConfig = gce.Action == ConfigDataAction.Delete
                                                 ? new GlobalConfig(string.Empty, string.Empty)
                                                 : gce.Config;
                                     }));
        }

        public async Task<GlobalConfig> QueryConfig()
        {
            if (_isInitialized)
                return GlobalConfig;

            try
            {
                var resonse = await _processService.ConfigAlive(_api, _system);
                if (!string.IsNullOrWhiteSpace(resonse))
                    throw new InvalidOperationException(resonse);

                GlobalConfig = await _api.Query<QueryGlobalConfig, GlobalConfig>(TimeSpan.FromSeconds(10));

                _isInitialized = true;
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

        public async Task<string> Update(GlobalConfig config)
        {
            if (config == GlobalConfig)
                return "Die Konfiguration ist gleich";

            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            await _api.Command(new UpdateGlobalConfigCommand(ConfigDataAction.Update, config), TimeSpan.FromSeconds(20));
            return string.Empty;
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}