using System.IO;
using Tauron.Application;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace ServiceManager.Server.AppCore.Settings
{
    public sealed class LocalConfigurationProvider : ISettingProviderConfiguration
    {
        public const string LocalScope = nameof(LocalScope);

        public string Scope => LocalScope;

        public ISettingProvider Provider => new JsonProvider(Path.Combine(TauronEnviroment.LocalApplicationData, "Service Manager", "Setting.conf"));
    }
}