using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigurationViewModel : ObservableObject, IInitable
    {
        private readonly IEventAggregator _aggregator;
        private readonly IServerConfigurationApi _api;

        public AppConfigurationViewModel(IServerConfigurationApi api, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _api = api;
            ServerConfigugration
        }

        public Task Init() => PropertyChangedComponent.Init(_api);

        public Task SetMonitorChanges();
        public Task SetRestartServices();
    }
}