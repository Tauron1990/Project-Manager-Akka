using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigurationViewModel : ObservableObject, IDisposable
    {
        private readonly IEventAggregator _aggregator;
        private readonly IServerConfigurationApi _api;
        private readonly IDisposable _subscription;

        private bool _monitorChanges;
        private bool _restartServices;

        public bool MonitorChanges
        {
            get => _monitorChanges;
            set => SetProperty(ref _monitorChanges, value);
        }

        public bool RestartServices
        {
            get => _restartServices;
            set => SetProperty(ref _restartServices, value);
        }

        public AppConfigurationViewModel(IServerConfigurationApi api, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _api = api;

            _subscription = api.PropertyChangedObservable
                               .Where(s => s == nameof(api.ServerConfigugration))
                               .Subscribe(_ =>
                                          {
                                              MonitorChanges = api.ServerConfigugration.MonitorChanges;
                                              RestartServices = api.ServerConfigugration.RestartServices;
                                          });

            Set();
        }

        public async Task SetMonitorChanges(bool value)
        {
            try
            {
                var result = await _api.Update(_api.ServerConfigugration with {MonitorChanges = value});
                if(string.IsNullOrWhiteSpace(result)) return;

                _aggregator.PublishWarnig($"Fehler beim Setzen: {result}");
                Set();
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
                Set();
            }
        }

        public async Task SetRestartServices(bool value)
        {
            try
            {
                var result = await _api.Update(_api.ServerConfigugration with { RestartServices = value });
                if (string.IsNullOrWhiteSpace(result)) return;

                _aggregator.PublishWarnig($"Fehler beim Setzen: {result}");
                Set();
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
                Set();
            }
        }

        private void Set()
        {
            RestartServices = _api.ServerConfigugration.RestartServices;
            MonitorChanges = _api.ServerConfigugration.MonitorChanges;
        }

        public void Dispose() => _subscription.Dispose();
    }
}