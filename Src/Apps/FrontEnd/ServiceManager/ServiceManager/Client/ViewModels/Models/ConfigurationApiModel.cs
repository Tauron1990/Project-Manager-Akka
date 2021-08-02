using System;
using System.Collections.Immutable;
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

        public ServerConfigugration ServerConfigugration { get; set; } = new(false, false, string.Empty);

        public SpecificConfigList AppConfigs { get; set; } = new(ImmutableList<SpecificConfig>.Empty);

        public ConfigurationApiModel(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator)
            : base(client, hubConnection, aggregator) { }


        public Task<ImmutableList<SpecificConfig>> QueryAppConfig()
            => Task.FromResult(AppConfigs.ConfigList);

        public async Task<string> DeleteSpecificConfig(string name)
        {
            try
            {
                var response = await Client.DeleteAsync($"{ConfigurationRestApi.DeleteSpecificConfig}/{name}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<StringApiContent>();

                return result == null ? "Unbekannter Fehler" : result.Content;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> Update(SpecificConfigData data)
        {
            var result = await Client.PostJson<SpecificConfigData, StringApiContent>(ConfigurationRestApi.UpdateSpecificConfigiration, data);

            return result?.Content ?? "Unbekannter Fehler";
        }

        public Task<GlobalConfig> QueryConfig() => Task.FromResult(GlobalConfig);

        public Task<ServerConfigugration> QueryServerConfig() => Task.FromResult(ServerConfigugration);

        public async Task<string> Update(GlobalConfig config)
            => await Client.PostJsonDefaultError(ConfigurationRestApi.GlobalConfig, config);

        public async Task<string> QueryBaseConfig()
        {
            var result = await Client.GetFromJsonAsync<StringApiContent>(ConfigurationRestApi.GetBaseConfig);

            return result == null ? string.Empty : result.Content;
        }

        public Task<string> Update(ServerConfigugration serverConfigugration)
            => Client.PostJsonDefaultError(ConfigurationRestApi.ServerConfiguration, serverConfigugration);

        public override async Task Init()
        {
            await Init(
                c => c.ForInterface<IServerConfigurationApi>(
                    ic =>
                    {
                        ic.OnPropertyChanged(
                            HubEvents.AppsConfigChanged,
                            l => AppConfigs = l ?? new SpecificConfigList(ImmutableList<SpecificConfig>.Empty),
                            c => c.GetFromJsonAsync<SpecificConfigList>(ConfigurationRestApi.GetAppConfigList));
                        ic.OnPropertyChanged(
                            sca => sca.GlobalConfig,
                            d => GlobalConfig = d ?? new GlobalConfig(string.Empty, NoDataLabel),
                            client => client.GetFromJsonAsync<GlobalConfig>(ConfigurationRestApi.GlobalConfig));

                        ic.OnPropertyChanged(
                            sca => sca.ServerConfigugration,
                            d => ServerConfigugration = d ?? new ServerConfigugration(false, false, string.Empty),
                            client => client.GetFromJsonAsync<ServerConfigugration>(ConfigurationRestApi.ServerConfiguration));
                    }));
        }
    }
}