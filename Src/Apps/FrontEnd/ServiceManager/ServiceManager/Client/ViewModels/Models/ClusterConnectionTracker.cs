using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Models
{
    public sealed class ClusterConnectionTracker : ModelBase, IClusterConnectionTracker
    {
        private bool _isConnected;
        private bool _isSelf;
        private AppIp _ip = AppIp.Invalid;
        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set => SetProperty(ref _isConnected, value);
        }

        public bool IsSelf
        {
            get => _isSelf;
            private set => SetProperty(ref _isSelf, value);
        }

        public AppIp Ip
        {
            get => _ip;
            private set => SetProperty(ref _ip, value);
        }

        public ClusterConnectionTracker(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator) 
            : base(client, hubConnection, aggregator)
        {
        }

        public override Task Init()
            => Init(mc => mc.ForInterface<IClusterConnectionTracker>(ic =>
                                                                     {
                                                                         ic.OnPropertyChanged(
                                                                             t => t.Ip,
                                                                             ip => Ip = ip ?? AppIp.Invalid,
                                                                             c => c.GetFromJsonAsync<AppIp>(ClusterConnectionTrackerApi.ClusterConnectionTracker));

                                                                         ic.OnPropertyChanged(
                                                                             t => t.IsConnected,
                                                                             b => IsConnected = b,
                                                                             c => c.GetFromJsonAsync<bool>(ClusterConnectionTrackerApi.IsConnected));

                                                                         ic.OnPropertyChanged(
                                                                             t => t.IsSelf,
                                                                             b => IsSelf = b,
                                                                             c => c.GetFromJsonAsync<bool>(ClusterConnectionTrackerApi.IsSelf));

                                                                         ic.OnPropertyChanged(
                                                                             t => t.Url,
                                                                             s => Url = s ?? string.Empty,
                                                                             c => c.GetFromJsonAsync<string>(ClusterConnectionTrackerApi.SelfUrl));
                                                                     }));
    }
}