using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Shared.Api;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationOptionsViewModel : ObservableObject
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly HttpClient _client;
        private readonly ConcurrentDictionary<string>

        public ConfigurationOptionsViewModel(IEventAggregator eventAggregator, HttpClient client)
        {
            _eventAggregator = eventAggregator;
            _client = client;
        }

        public async Task LoadAsyncFor(string name, IOperationManager manager)
        {
            try
            {
                using (manager.Start())
                {
                    
                }
            }
            finally
            {
                OnPropertyChangedExplicit("All");
            }
        }

        public sealed record OptionElement(bool Error, string Message, ConfigOption[] ConfigOptions);
    }
}