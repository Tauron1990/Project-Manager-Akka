using System.Threading.Tasks;
using ServiceManager.Client.Components;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigurationViewModel : ObservableObject, IInitable
    {
        private readonly IServerConfigurationApi _api;

        public AppConfigurationViewModel(IServerConfigurationApi api)
        {
            _api = api;
        }



        public Task Init() => PropertyChangedComponent.Init(_api);
    }
}