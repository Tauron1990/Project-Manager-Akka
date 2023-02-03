using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tauron.Application.Settings;
using Tauron.TAkka;

namespace TimeTracker.Data
{
    public sealed class AppSettings : ConfigurationBase
    {
        public AppSettings(IDefaultActorRef<SettingsManager> actor, ILogger<AppSettings> logger)
            : base(actor, "App", logger) { }

        public ImmutableList<string> AllProfiles
        {
            get => GetValue(JsonConvert.DeserializeObject<ImmutableList<string>>) ?? ImmutableList<string>.Empty;
            set => SetValue(JsonConvert.SerializeObject(value));
        }

        public string CurrentProfile
        {
            get => GetValue(s => s, string.Empty) ?? string.Empty;
            set => SetValue(value);
        }
    }
}