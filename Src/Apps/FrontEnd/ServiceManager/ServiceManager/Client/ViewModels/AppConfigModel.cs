using System;
using System.Collections.Immutable;
using System.Linq;
using Akka.Configuration;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Shared.Configuration;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class AppConfigModel : ObservableObject
    {
        private readonly IEventAggregator _aggregator;
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

        public void OptionSelected(OptionSelected selected)
        {
            try
            {
                var config   = ConfigurationFactory.ParseString(ConfigString);
                var toUpdate = ConfigurationFactory.ParseString($"{selected.Path}: {selected.Value}");

                ConfigString = toUpdate.WithFallback(config).ToString(true);
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public SpecificConfigData CreateNew()
            => new(IsNew, Config.Id, InfoString, ConfigString,
                UpdateCondiditions 
                    ? Conditions.Select(c => new ConfigurationInfo(c.Name, ConfigDataAction.Update, c)).ToImmutableList()
                    : null);
    }
}