using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using ServiceManager.Client.Components;
using ServiceManager.Client.Shared.Dialog;
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
        private readonly IDialogService _dialogService;
        private bool _init;
        private Guid _current;

        public Serverinfo(HttpClient client, HubConnection connection, IEventAggregator aggregator, IDialogService dialogService)
        {
            _client = client;
            _connection = connection;
            _aggregator = aggregator;
            _dialogService = dialogService;
        }

        public async Task Restart()
        {
            try
            {
                var response = await _client.PostAsync(ServerInfoApi.ServerInfo, new StringContent(string.Empty));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public async Task<string?> TryReconnect()
        {
            try
            {
                using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var newId = await _client.GetFromJsonAsync<Guid>(ServerInfoApi.ServerInfo, cancel.Token);
                if (newId == _current)
                    return "Server nicht neu Gestartet";

                if (_connection.State == HubConnectionState.Disconnected)
                    // ReSharper disable once MethodSupportsCancellation
                    await _connection.StartAsync();

                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
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
                _connection.On(HubEvents.RestartServer, async () =>
                                                        {
                                                            _init = false;
                                                            await _connection.StopAsync();
                                                            _aggregator.GetEvent<ReloadAllEvent, Unit>().Publish(Unit.Default);
                                                            _dialogService.Show<AwaitRestartDialog>("Warte",
                                                                new DialogOptions
                                                                {
                                                                    DisableBackdropClick = true,
                                                                    CloseButton = false,
                                                                    FullScreen = true
                                                                });
                                                        });
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                _init = false;
                throw;
            }
        }
    }
}