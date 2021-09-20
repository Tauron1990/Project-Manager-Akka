using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Configuration;
using Microsoft.AspNetCore.Components;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Shared.BaseComponents;
using ServiceManager.Client.Shared.Configuration;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Configuration
{
    public sealed class AppConfigModel : ObservableObject, ISelectable
    {
        private readonly IEventAggregator _aggregator;
        private bool _isSelected;
        public           SpecificConfig   Config { get; }

        public bool IsNew { get; set; }

        public bool UpdateCondiditions { get; set; }
        
        public string InfoString { get; set; }

        public string ConfigString { get; set; }

        public ImmutableList<Condition> Conditions { get; set; } = ImmutableList<Condition>.Empty;
        
        public AppConfigModel(SpecificConfig config, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            Config           = config;
            InfoString       = config.Info ?? string.Empty;
            ConfigString     = config.ConfigContent;
        }

        public void OptionSelected(OptionSelected selected, Action stateHasChanged)
        {
            try
            {
                var config   = string.IsNullOrWhiteSpace(ConfigString) ? Akka.Configuration.Config.Empty : ConfigurationFactory.ParseString(ConfigString);
                var toUpdate = ConfigurationFactory.ParseString($"{selected.Path}: {selected.Value}");

                ConfigString = toUpdate.WithFallback(config).ToString(includeFallback: true);
                stateHasChanged();
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public string? Validatebasic()
            => Conditions.Count == 0 
                ? "Keine Bedingungen angegeben" 
                : string.IsNullOrWhiteSpace(ConfigString) 
                    ? "Keine Konfiguration Eingegeben" 
                    : null;

        public SpecificConfigData CreateNew()
            => new(IsNew, Config.Id, InfoString, ConfigString,
                UpdateCondiditions 
                    ? Conditions.Select(c => new ConfigurationInfo(c.Name, ConfigDataAction.Update, c)).ToImmutableList()
                    : null);

        public EventCallback Callback { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Callback.InvokeAsync().Ignore();
            }
        }
    }
}