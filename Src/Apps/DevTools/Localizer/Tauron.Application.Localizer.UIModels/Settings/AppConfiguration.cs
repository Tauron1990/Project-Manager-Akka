﻿namespace Tauron.Application.Localizer.UIModels.Settings
{
    public sealed class AppConfiguration : ISettingProviderConfiguration
    {
        public string Scope => SettingTypes.AppConfig;
        public ISettingProvider Provider => new JsonProvider("appconfig.json");
    }
}