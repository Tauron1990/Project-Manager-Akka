using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ServiceManager.Client.Components;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewAppConfigModel : ObservableObject, IInitable, IDisposable
    {
        private readonly IServerConfigurationApi _api;
        private readonly IDisposable             _subscription;
        
        private AppConfigModel?             _toEdit;
        private bool                        _isLoading = true;
        private IEnumerable<AppConfigModel> _appConfigs;

        public AppConfigModel? ToEdit
        {
            get => _toEdit;
            private set => SetProperty(ref _toEdit, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public IEnumerable<AppConfigModel> AppConfigs
        {
            get => _appConfigs;
            private set
            {
                SetProperty(ref _appConfigs, value);
                ToEdit = null;
            }
        }

        public ConfigurationViewAppConfigModel(IServerConfigurationApi api)
        {
            _api = api;
            _appConfigs = Enumerable.Empty<AppConfigModel>();
            _subscription = (from prop in api.PropertyChangedObservable
                             where prop == HubEvents.AppsConfigChanged
                             from config in api.QueryAppConfig()
                             select (from specificConfig in config
                                     select new AppConfigModel(specificConfig))
                                .ToImmutableList())
               .AutoSubscribe(config => AppConfigs = config);
        }

        public async Task Init()
        {
            try
            { 
                await PropertyChangedComponent.Init(_api);
                AppConfigs = (await _api.QueryAppConfig())
                   .Select(c => new AppConfigModel(c))
                   .ToImmutableList();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task NewConfig();

        public async Task DeleteConfig(AppConfigModel? model);

        public async Task EditConfig(AppConfigModel? model);
        
        public void Dispose()
            => _subscription.Dispose();
    }
}