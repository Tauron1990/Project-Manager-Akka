﻿using System;
using System.IO;
using Tauron.Application.Master.Commands.Administration.Configuration;
using Tauron.Application.ServiceManager.AppCore.Settings;
using Tauron.Application.ServiceManager.Properties;

namespace Tauron.Application.ServiceManager.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ObservableObject, IDatabaseConfig
    {
        private readonly ILocalConfiguration _configuration;
        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value);
        }

        public bool IsReady { get; }

        public DatabaseConfig(ILocalConfiguration configuration)
        {
            _configuration = configuration;
            Url = _configuration.DatabaseUrl;
            IsReady = !string.IsNullOrWhiteSpace(Url);
        }

        public bool SetUrl(string url)
        {
            if(url.Equals(_configuration.DatabaseUrl, StringComparison.Ordinal)) return false;

            Url = url;
            string targetFile = AkkaConfigurationBuilder.Main.FileInAppDirectory();
            string config = targetFile.ReadTextIfExis();

            config = AkkaConfigurationBuilder.ApplyMongoUrl(config, Resources.BaseConfig, url);
            File.WriteAllText(targetFile, config);

            _configuration.DatabaseUrl = url;

            return true;
        }
    }
}