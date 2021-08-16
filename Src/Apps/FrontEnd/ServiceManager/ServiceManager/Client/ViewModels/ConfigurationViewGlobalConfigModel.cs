using System;
using System.Threading.Tasks;
using Akka.Configuration;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Client.Shared.Configuration;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewGlobalConfigModel
    {
        private readonly IServerConfigurationApi _api;
        private readonly IEventAggregator _aggregator;
        private readonly IDatabaseConfig _databaseConfig;

        public string ConfigInfo { get; set; } = string.Empty;

        public string ConfigContent { get; set; } = string.Empty;

        public ConfigurationViewGlobalConfigModel(IServerConfigurationApi api, IEventAggregator aggregator, IDatabaseConfig databaseConfig)
        {
            
            _api = api;
            _aggregator = aggregator;
            _databaseConfig = databaseConfig;
        }

        public async Task GenerateDefaultConfig()
        {
            if(! await _databaseConfig.GetIsReady()) return;

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

                var newData = toUpdate.WithFallback(config).ToString(true);

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
                    var result = await _api.UpdateGlobalConfig(new UpdateGlobalConfigApiCommand(new GlobalConfig(ConfigContent, ConfigInfo)));
                    if(string.IsNullOrWhiteSpace(result)) return;

                    _aggregator.PublishWarnig(result);
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }
    }
}