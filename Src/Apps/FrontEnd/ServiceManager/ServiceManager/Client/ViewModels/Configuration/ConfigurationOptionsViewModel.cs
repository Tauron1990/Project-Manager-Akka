﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Akka.Util.Internal;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Configuration
{
    public sealed class ConfigurationOptionsViewModel
    {
        public static readonly OptionElement DefaultElement = new(Error: true, "Daten nicht geladen", Array.Empty<MutableConfigOption>());
        private readonly IServerConfigurationApi _api;
        private readonly HubConnection _connection;
        private readonly IDialogService _dialogService;
        private readonly ConcurrentDictionary<ConfigOpensElement, OptionElement> _elements = new();

        private readonly IEventAggregator _eventAggregator;

        public ConfigurationOptionsViewModel(
            IEventAggregator eventAggregator, IDialogService dialogService, HubConnection connection,
            IServerConfigurationApi api)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _connection = connection;
            _api = api;
        }

        public OptionElement Akka => _elements.GetOrElse(ConfigOpensElement.Akka, DefaultElement);

        public OptionElement AkkaRemote => _elements.GetOrElse(ConfigOpensElement.AkkaRemote, DefaultElement);

        public OptionElement AkkaPersistence => _elements.GetOrElse(ConfigOpensElement.AkkaPersistence, DefaultElement);

        public OptionElement AkkaCluster => _elements.GetOrElse(ConfigOpensElement.AkkaCluster, DefaultElement);

        public OptionElement AkkaStreams => _elements.GetOrElse(ConfigOpensElement.AkkaStreams, DefaultElement);

        public async Task LoadAsyncFor(ConfigOpensElement name, Action stateChange)
        {
            try
            {
                if (_elements.TryGetValue(name, out var element) && !element.Error)
                    return;

                if (_connection.State == HubConnectionState.Disconnected)
                    await _connection.StartAsync();

                var response = await _connection.InvokeAsync<ConfigOptionList>("GetConfigFileOptions", name);

                if (response == null)
                {
                    _elements[name] = new OptionElement(Error: true, "Fehler beim Laden der Daten", Array.Empty<MutableConfigOption>());

                    return;
                }

                _elements[name] = new OptionElement(Error: false, string.Empty, response.Options.Select(co => new MutableConfigOption(co)).ToArray());
            }
            catch (Exception e)
            {
                _eventAggregator.PublishError(e);
                _elements[name] = new OptionElement(Error: true, e.Message, Array.Empty<MutableConfigOption>());
            }
            finally
            {
                stateChange();
            }
        }

        public async Task ShowConfigFile(ConfigOpensElement name)
        {
            try
            {
                var result = await _api.QueryDefaultFileContent(name);
                if (result == null)
                    _eventAggregator.PublishError($"Unbkannter Fehler beim abrufen der Datei {name}");
                else
                    _dialogService.Show<ShowTextDataDialog>("Konfigurations Text", ShowTextDataDialog.GetParameters(result), new DialogOptions { FullScreen = true, CloseButton = true });
            }
            catch (Exception e)
            {
                _eventAggregator.PublishError(e);
            }
        }

        public sealed record OptionElement(bool Error, string Message, MutableConfigOption[] ConfigOptions);

        public sealed record MutableConfigOption(string Path)
        {
            public MutableConfigOption(ConfigOption option)
                : this(option.Path)
                => CurrentValue = option.DefaultValue;

            public string CurrentValue { get; set; } = string.Empty;
        }
    }
}