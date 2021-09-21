using System.Collections.Immutable;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tauron.Akka;
using Tauron.Application;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace TimeTracker.Data
{
    public sealed class AppSettingsConfiguration : ISettingProviderConfiguration
    {
        private readonly ITauronEnviroment _enviroment;

        public AppSettingsConfiguration(ITauronEnviroment enviroment)
            => _enviroment = enviroment;

        public string Scope => "App";

        public ISettingProvider Provider => new JsonProvider(Path.Combine(_enviroment.AppData(), "AppSettings.json"));
    }

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