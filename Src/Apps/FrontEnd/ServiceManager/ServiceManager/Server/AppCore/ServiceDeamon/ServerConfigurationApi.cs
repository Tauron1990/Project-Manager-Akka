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
using Tauron.Application.AkkaNode.Services.Core;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed class ServerConfigurationApi : IServerConfigurationApi, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ConfigurationApi _api;
        private readonly ActorSystem _system;
        private readonly IPropertyChangedNotifer _notifer;
        private readonly IDisposable _subscriptions;
        private readonly object _lock = new();

        private bool _isInitialized;
        private GlobalConfig _globalConfig = new(string.Empty, string.Empty);

        public GlobalConfig GlobalConfig
        {
            get => _globalConfig;
            set
            {
                _globalConfig = value;
                _notifer.SendPropertyChanged<IServerConfigurationApi>(nameof(GlobalConfig));
            }
        }

        public ServerConfigurationApi(ConfigurationApi api, ActorSystem system, ConfigEventDispatcher dispatcher, IPropertyChangedNotifer notifer)
        {
            _api = api;
            _system = system;
            _notifer = notifer;
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
                var response = await _api.QueryIsAlive(_system, TimeSpan.FromSeconds(10));
                if (!response.IsAlive)
                    throw new InvalidOperationException("Query an Konfigurations Api Fehlgeschlagen");

                _isInitialized = true;
                return GlobalConfig;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Query Global Config");
                throw;
            }
        }

        public void Dispose() => _subscriptions.Dispose();
    }
}