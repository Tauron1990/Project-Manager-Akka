using System.Threading.Tasks;
using ServiceManager.Client.Components;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewAppConfigModel : ObservableObject, IInitable
    {
        private readonly IServerConfigurationApi _api;
        private bool _isEditing;

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public ConfigurationViewAppConfigModel(IServerConfigurationApi api)
        {
            _api = api;
        }

        public Task Init() => PropertyChangedComponent.Init(_api);
    }
}