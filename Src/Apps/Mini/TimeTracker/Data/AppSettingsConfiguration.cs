using System.IO;
using Tauron.Application;
using Tauron.Application.Settings;
using Tauron.Application.Settings.Provider;

namespace TimeTracker.Data;

public sealed class AppSettingsConfiguration : ISettingProviderConfiguration
{
    private readonly ITauronEnviroment _enviroment;

    public AppSettingsConfiguration(ITauronEnviroment enviroment)
        => _enviroment = enviroment;

    public string Scope => "App";

    public ISettingProvider Provider => new JsonProvider(Path.Combine(_enviroment.AppData(), "AppSettings.json"));
}