using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Models
{
    public class AppIpManager : ModelBase, IAppIpManager
    {
        private AppIp _ip = AppIp.Invalid;


        public AppIp Ip
        {
            get => _ip;
            private set => SetProperty(ref _ip, value);
        }

        public async Task<string> WriteIp(string ip)
        {
            var response = await Client.PostAsync(AppIpManagerApi.AppIpManager, new StringContent(ip));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public override Task Init()
            => Init(mc => mc.ForInterface<IAppIpManager>(ic => ic.OnPropertyChanged(
                                                             m => m.Ip,
                                                             ip => Ip = ip ?? AppIp.Invalid,
                                                             c => c.GetFromJsonAsync<AppIp>(AppIpManagerApi.AppIpManager))));

        public AppIpManager(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator)
            : base(client, hubConnection, aggregator)
        {
        }
    }
}