using System;
using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components;
using ServiceManager.Client.Shared.Configuration;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewGlobalConfigModel : ObservableObject, IInitable, IDisposable
    {
        private readonly IDisposable _subscription;

        private string _configInfo = string.Empty;
        private string _configContent = string.Empty;

        public string ConfigInfo
        {
            get => _configInfo;
            set => SetProperty(ref _configInfo, value);
        }

        public string ConfigContent
        {
            get => _configContent;
            set => SetProperty(ref _configContent, value);
        }

        public ConfigurationViewGlobalConfigModel(Confige)
        {
            
        }

        public Task Init() => Task.CompletedTask;

        public void UpdateContent(OptionSelected optionSelected);

        public Task UpdateConfig();

        public void Dispose() => _subscription.Dispose();
    }
}