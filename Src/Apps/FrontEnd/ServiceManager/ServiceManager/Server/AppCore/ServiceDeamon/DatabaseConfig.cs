using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServiceManager.Server.AppCore.Settings;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ObservableObject, IDatabaseConfig
    {
        private readonly ILocalConfiguration _configuration;
        private readonly IPropertyChangedNotifer _notifer;
        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value);
        }

        public bool IsReady { get; }

        public DatabaseConfig(ILocalConfiguration configuration, IPropertyChangedNotifer notifer)
        {
            _configuration = configuration;
            _notifer = notifer;
            Url = _configuration.DatabaseUrl;
            IsReady = !string.IsNullOrWhiteSpace(Url);
        }

        public async Task<string> SetUrl(string url)
        {
            try
            {
                if(url.Equals(_configuration.DatabaseUrl, StringComparison.Ordinal)) return "Datenbank ist gleich";

                var murl = new MongoUrl(url);

                Url = url;
                var targetFile = AkkaConfigurationBuilder.Main.FileInAppDirectory();
                var config = targetFile.ReadTextIfExis();

                config = AkkaConfigurationBuilder.ApplyMongoUrl(config, Resources.BaseConfig, murl.ToString());
                await File.WriteAllTextAsync(targetFile, config);

                _configuration.DatabaseUrl = url;

                await _notifer.SendPropertyChanged<IDatabaseConfig>(nameof(Url));

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}