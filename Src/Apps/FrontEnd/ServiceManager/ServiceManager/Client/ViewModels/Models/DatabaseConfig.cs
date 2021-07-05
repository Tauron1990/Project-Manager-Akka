using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;
using StringContent = ServiceManager.Shared.Api.StringContent;

namespace ServiceManager.Client.ViewModels.Models
{
    public sealed class DatabaseConfig : ModelBase, IDatabaseConfig
    {
        private string _url = string.Empty;
        private bool _isReady;

        public DatabaseConfig(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator) 
            : base(client, hubConnection, aggregator)
        {
        }

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value);
        }

        public bool IsReady
        {
            get => _isReady;
            private set => SetProperty(ref _isReady, value);
        }

        public async Task<string> SetUrl(string url)
        {
            try
            {
                using var response = await Client.PostAsJsonAsync(DatabaseConfigApi.DatabaseConfigApiBase, new StringContent(url));
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<StringContent>();
                return result?.Content ?? "Keine Antwort vom Server";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public override Task Init()
        {
            return Init(mc =>
                        {
                            mc.ForInterface<IDatabaseConfig>(ic =>
                                                             {
                                                                 ic.OnPropertyChanged(
                                                                     dc => dc.IsReady,
                                                                     b => IsReady = b?.Content ?? false,
                                                                     c => c.GetFromJsonAsync<BoolContent>(DatabaseConfigApi.IsReady));

                                                                 ic.OnPropertyChanged(
                                                                     dc => dc.Url, 
                                                                     u => Url = u?.Content ?? string.Empty,
                                                                     c => c.GetFromJsonAsync<StringContent>(DatabaseConfigApi.DatabaseConfigApiBase));
                                                             });
                        });
        }
    }
}