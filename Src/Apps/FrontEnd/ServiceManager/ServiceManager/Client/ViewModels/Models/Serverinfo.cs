using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Client.Components;
using ServiceManager.Client.ViewModels.Events;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using Tauron.Application;
using StringContent = System.Net.Http.StringContent;

namespace ServiceManager.Client.ViewModels.Models
{
    public sealed class Serverinfo : IServerInfo, IInitable
    {
        private readonly HttpClient _client;
        private readonly HubConnection _connection;
        private readonly IEventAggregator _aggregator;
        private bool _init;
        private Guid _current;

        public Serverinfo(HttpClient client, HubConnection connection, IEventAggregator aggregator)
        {
            _client = client;
            _connection = connection;
            _aggregator = aggregator;
        }

        public async Task Restart()
        {
            var response = await _client.PostAsync(ServerInfoApi.ServerInfo, new StringContent(string.Empty));
            response.EnsureSuccessStatusCode();
        }

        public async Task Init()
        {
            if(_init) return;
            _init = true;
            try
            {
                _current = await _client.GetFromJsonAsync<Guid>(ServerInfoApi.ServerInfo);
                if(_connection.State == HubConnectionState.Disconnected)
                    await _connection.StartAsync();
                _connection.On(HubEvents.RestartServer, () => _aggregator.GetEvent<ReloadAllEvent, Unit>().Publish(Unit.Default));
            }
            catch
            {
                _init = false;
                throw;
            }
        }
    }
}