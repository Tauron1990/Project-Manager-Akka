using System;
using System.Threading.Tasks;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Configuration
{
    public sealed class AppConfigurationViewModel
    {
        private readonly IEventAggregator _aggregator;
        private readonly IServerConfigurationApi _api;

        public AppConfigurationViewModel(IServerConfigurationApi api, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _api = api;
        }

        public async Task SetMonitorChanges(ServerConfigugration serverConfigugration, bool value)
        {
            try
            {
                var result = await _api.UpdateServerConfig(new UpdateServerConfiguration(serverConfigugration with { MonitorChanges = value }));

                if (string.IsNullOrWhiteSpace(result)) return;

                _aggregator.PublishWarnig($"Fehler beim Setzen: {result}");
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public async Task SetRestartServices(ServerConfigugration serverConfigugration, bool value)
        {
            try
            {
                var result = await _api.UpdateServerConfig(new UpdateServerConfiguration(serverConfigugration with { RestartServices = value }));

                if (string.IsNullOrWhiteSpace(result)) return;

                _aggregator.PublishWarnig($"Fehler beim Setzen: {result}");
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }
    }
}