using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Models
{
    public sealed class ConfigurationApiModel : ModelBase, IServerConfigurationApi
    {
        public const string NoDataLabel = "Keine Daten";

        public GlobalConfig GlobalConfig { get; set; } = new(string.Empty, NoDataLabel);

        public ConfigurationApiModel(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator) 
            : base(client, hubConnection, aggregator)
        {
        }


        public Task<GlobalConfig> QueryConfig() => Task.FromResult(GlobalConfig);

        public async Task<string> Update(GlobalConfig config)
        {
            var response = await Client.PostAsJsonAsync(ConfigurationRestApi.GlobalConfig, config);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<StringApiContent>();

            return result == null
                ? "Unbekannter Fehler beim Update"
                : result.Content;
        }

        public override async Task Init()
        {
            await Init(c => c.ForInterface<IServerConfigurationApi>(
                           ic => ic.OnPropertyChanged(
                               sca => sca.GlobalConfig,
                               d => GlobalConfig = d ?? new GlobalConfig(string.Empty, NoDataLabel),
                               c => c.GetFromJsonAsync<GlobalConfig>(ConfigurationRestApi.GlobalConfig))));
        }
    }
}