using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewAppConfigModel : ObservableObject, IInitable
    {
        private readonly IServerConfigurationApi _api;
        private bool _isEditing;
        private bool _isLoading;
        private IEnumerable<SpecificConfig> _appConfigs;

        public bool IsEditing
        {
            get => _isEditing;
            private set => SetProperty(ref _isEditing, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public IEnumerable<SpecificConfig> AppConfigs
        {
            get => _appConfigs;
            private set => SetProperty(ref _appConfigs, value);
        }

        public ConfigurationViewAppConfigModel(IServerConfigurationApi api)
        {
            _api = api;
        }

        public Task Init() => PropertyChangedComponent.Init(_api);
    }
}