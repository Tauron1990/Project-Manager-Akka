using Microsoft.Extensions.Logging;
using Tauron.Application.Settings;
using Tauron.TAkka;

namespace ServiceManager.Server.AppCore.Settings
{
    public sealed class LocalConfiguration : ConfigurationBase, ILocalConfiguration
    {
        public LocalConfiguration(IDefaultActorRef<SettingsManager> actor, ILogger<LocalConfiguration> logger)
            : base(actor, LocalConfigurationProvider.LocalScope, logger) { }

        public string DatabaseUrl
        {
            get => GetValue(s => s, string.Empty)!;
            set => SetValue(value);
        }
    }
}