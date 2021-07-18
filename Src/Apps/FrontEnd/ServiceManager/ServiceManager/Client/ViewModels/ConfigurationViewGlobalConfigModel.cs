using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Configuration;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Client.Shared.Configuration;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewGlobalConfigModel : ObservableObject, IInitable, IDisposable
    {
        private readonly IServerConfigurationApi _api;
        private readonly IEventAggregator _aggregator;
        private readonly IDatabaseConfig _databaseConfig;
        private readonly IDisposable _subscription;

        private string _configInfo = string.Empty;
        private string _configContent = string.Empty;

        public string ConfigInfo
        {
            get => _configInfo;
            set => SetProperty(ref _configInfo, value);
        }

        public string ConfigContent
        {
            get => _configContent;
            set => SetProperty(ref _configContent, value);
        }

        public ConfigurationViewGlobalConfigModel(IServerConfigurationApi api, IEventAggregator aggregator, IDatabaseConfig databaseConfig)
        {
            _api = api;
            _aggregator = aggregator;
            _databaseConfig = databaseConfig;

            _subscription = api.PropertyChangedObservable.Where(s => s == nameof(api.GlobalConfig)).Select(_ => api.GlobalConfig)
                               .Subscribe(gc =>
                                          {
                                              var (configContent, info) = gc;
                                              ConfigInfo = info ?? string.Empty;
                                              ConfigContent = configContent;
                                          });

            ConfigInfo = api.GlobalConfig.Info ?? string.Empty;
            ConfigContent = api.GlobalConfig.ConfigContent;
        }

        public Task Init() => PropertyChangedComponent.Init(_api);

        public async Task GenerateDefaultConfig()
        {
            if(!_databaseConfig.IsReady) return;

            var result = AkkaConfigurationBuilder.ApplyMongoUrl(ConfigContent, await _api.QueryBaseConfig(), _databaseConfig.Url);
            ConfigContent = result;
            ConfigInfo = "Generierte Konfiguration";
        }

        public void UpdateContent(OptionSelected optionSelected)
        {
            try
            {
                var toUpdate = ConfigurationFactory.ParseString($"{optionSelected.Path}: {optionSelected.Value}");
                var config = ConfigurationFactory.ParseString(ConfigContent);

                var newData = toUpdate.WithFallback(config).ToString(true);

                ConfigContent = newData;
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public void Reset()
        {
            ConfigInfo = _api.GlobalConfig.Info ?? string.Empty;
            ConfigContent = _api.GlobalConfig.ConfigContent;
        }

        public async Task UpdateConfig(IOperationManager manager)
        {
            try
            {
                using (manager.Start())
                {
                    var result = await _api.Update(new GlobalConfig(ConfigContent, ConfigInfo));
                    if(string.IsNullOrWhiteSpace(result)) return;

                    _aggregator.PublishWarnig(result);
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public void Dispose() => _subscription.Dispose();
    }
}