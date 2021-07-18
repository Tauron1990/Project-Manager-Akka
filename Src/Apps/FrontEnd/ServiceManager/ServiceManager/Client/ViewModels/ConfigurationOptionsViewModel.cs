using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Util.Internal;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Shared.Api;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationOptionsViewModel : ObservableObject
    {
        public static readonly OptionElement DefaultElement = new(true, "Daten nicht geladen", Array.Empty<MutableConfigOption>());

        private readonly IEventAggregator _eventAggregator;
        private readonly HttpClient _client;
        private readonly IDialogService _dialogService;
        private readonly HubConnection _connection;
        private readonly ConcurrentDictionary<string, OptionElement> _elements = new();

        public OptionElement Akka => _elements.GetOrElse(ConfigurationRestApi.ModuleName.Akka, DefaultElement);

        public OptionElement AkkaRemote => _elements.GetOrElse(ConfigurationRestApi.ModuleName.AkkaRemote, DefaultElement);

        public OptionElement AkkaPersistence => _elements.GetOrElse(ConfigurationRestApi.ModuleName.AkkaPersistence, DefaultElement);

        public OptionElement AkkaCluster => _elements.GetOrElse(ConfigurationRestApi.ModuleName.AkkaCluster, DefaultElement);

        public OptionElement AkkaStreams => _elements.GetOrElse(ConfigurationRestApi.ModuleName.AkkaStreams, DefaultElement);

        public ConfigurationOptionsViewModel(IEventAggregator eventAggregator, HttpClient client, IDialogService dialogService, HubConnection connection)
        {
            _eventAggregator = eventAggregator;
            _client = client;
            _dialogService = dialogService;
            _connection = connection;
        }

        public async Task LoadAsyncFor(string name, IOperationManager manager)
        {
            OnPropertyChangedExplicit("All");
            try
            {
                if (_elements.TryGetValue(name, out var element) && !element.Error)
                    return;

                using (manager.Start())
                {
                    var response = await _connection.InvokeAsync<ConfigOptionList>(nameof(ConfigurationRestApi.GetConfigFileOptions), name);

                    if (response == null)
                    {
                        _elements[name] = new OptionElement(true, "Fehler beim Laden der Daten", Array.Empty<MutableConfigOption>());
                        return;
                    }

                    _elements[name] = new OptionElement(false, string.Empty, response.Options.Select(co => new MutableConfigOption(co)).ToArray());
                }
            }
            catch (Exception e)
            {
                _eventAggregator.PublishError(e);
                _elements[name] = new OptionElement(true, e.Message, Array.Empty<MutableConfigOption>());
            }
            finally
            {
                OnPropertyChangedExplicit("All");
            }
        }

        public async Task ShowConfigFile(string name)
        {
            try
            {
                var result = await _client.GetFromJsonAsync<StringApiContent>(ConfigurationRestApi.GetConfigFile + "/" + name);
                if(result == null)
                    _eventAggregator.PublishError($"Unbkannter Fehler beim abrufen der Datei {name}");
                else
                    _dialogService.Show<ShowTextDataDialog>("Konfigurations Text", ShowTextDataDialog.GetParameters(result.Content), new DialogOptions {FullScreen = true, CloseButton = true});
            }
            catch (Exception e)
            {
                _eventAggregator.PublishError(e);
            }
        }

        public sealed record OptionElement(bool Error, string Message, MutableConfigOption[] ConfigOptions);

        public sealed record MutableConfigOption(string Path)
        {
            public string CurrentValue { get; set; } = string.Empty;

            public MutableConfigOption(ConfigOption option)
                : this(option.Path)
                => CurrentValue = option.DefaultValue;
        }
    }
}