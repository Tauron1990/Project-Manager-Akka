using System;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Hosting;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Client.Shared.Configuration;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Configuration
{
    public sealed class ConfigurationViewGlobalConfigModel
    {
        private readonly IEventAggregator _aggregator;
        private readonly IServerConfigurationApi _api;
        private readonly IDatabaseConfig _databaseConfig;

        public ConfigurationViewGlobalConfigModel(IServerConfigurationApi api, IEventAggregator aggregator, IDatabaseConfig databaseConfig)
        {
            _api = api;
            _aggregator = aggregator;
            _databaseConfig = databaseConfig;
        }

        public string ConfigInfo { get; set; } = string.Empty;

        public string ConfigContent { get; set; } = string.Empty;

        public async Task GenerateDefaultConfig()
        {
            if (!await _databaseConfig.GetIsReady()) return;

            var result = AkkaConfigurationBuilder.ApplyMongoUrl(ConfigContent, await _api.QueryBaseConfig(), await _databaseConfig.GetUrl());
            ConfigContent = result;
            ConfigInfo = "Generierte Konfiguration";
        }

        public void UpdateContent(OptionSelected optionSelected)
        {
            try
            {
                var toUpdate = ConfigurationFactory.ParseString($"{optionSelected.Path}: {optionSelected.Value}");
                var config = ConfigurationFactory.ParseString(ConfigContent);

                var newData = toUpdate.WithFallback(config).ToString(includeFallback: true);

                ConfigContent = newData;
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public void Reset(GlobalConfig config)
        {
            ConfigInfo = config.Info ?? string.Empty;
            ConfigContent = config.ConfigContent;
        }

        public async Task UpdateConfig(IOperationManager manager)
        {
            try
            {
                using (manager.Start())
                {
                    if (await _aggregator.IsSuccess(() => _api.UpdateGlobalConfig(new UpdateGlobalConfigApiCommand(new GlobalConfig(ConfigContent, ConfigInfo)))))
                        _aggregator.PublishSuccess("Konfiguration gespeichert");
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }
    }
}