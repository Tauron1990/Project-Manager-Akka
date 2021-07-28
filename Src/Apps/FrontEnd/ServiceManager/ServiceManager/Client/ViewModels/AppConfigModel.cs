using ServiceHost.Client.Shared.ConfigurationServer.Data;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigModel : ObservableObject
    {
        public SpecificConfig Config { get; }
        

        public AppConfigModel(SpecificConfig config) => Config = config;
    }
}