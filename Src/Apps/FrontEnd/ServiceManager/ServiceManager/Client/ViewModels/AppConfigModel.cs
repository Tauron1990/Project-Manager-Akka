using ServiceHost.Client.Shared.ConfigurationServer.Data;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigModel : ObservableObject
    {
        private bool _isEditing;
        public SpecificConfig Config { get; }

        public bool IsEditing
        {
            get => _isEditing;
            private set => SetProperty(ref _isEditing, value);
        }

        public AppConfigModel(SpecificConfig config) => Config = config;
    }
}