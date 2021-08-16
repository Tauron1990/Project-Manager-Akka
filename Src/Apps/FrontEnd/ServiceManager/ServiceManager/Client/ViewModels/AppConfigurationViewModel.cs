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
        private readonly IServerConfigurationApiOld _apiOld;
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

        public AppConfigurationViewModel(IServerConfigurationApiOld apiOld, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _apiOld = apiOld;

            _subscription = apiOld.PropertyChangedObservable
                               .Where(s => s == nameof(apiOld.ServerConfigugration))
                               .Subscribe(_ =>
                                          {
                                              MonitorChanges = apiOld.ServerConfigugration.MonitorChanges;
                                              RestartServices = apiOld.ServerConfigugration.RestartServices;
                                          });

            Set();
        }

        public async Task SetMonitorChanges(bool value)
        {
            try
            {
                var result = await _apiOld.Update(_apiOld.ServerConfigugration with {MonitorChanges = value});
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
                var result = await _apiOld.Update(_apiOld.ServerConfigugration with { RestartServices = value });
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
            RestartServices = _apiOld.ServerConfigugration.RestartServices;
            MonitorChanges = _apiOld.ServerConfigugration.MonitorChanges;
        }

        public void Dispose() => _subscription.Dispose();
    }
}