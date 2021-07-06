using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using MongoDB.Driver;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.Settings;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ObservableObject, IDatabaseConfig, IDisposable
    {
        private readonly ILocalConfiguration _configuration;
        private readonly IPropertyChangedNotifer _notifer;
        private readonly ConfigurationApi _configurationApi;
        private readonly IClusterConnectionTracker _tracker;
        private readonly ActorSystem _system;
        private readonly IDisposable _subscription;

        private string _url = string.Empty;

        public string Url
        {
            get => _url;
            private set => SetProperty(ref _url, value);
        }

        public bool IsReady { get; }

        public DatabaseConfig(ILocalConfiguration configuration, IPropertyChangedNotifer notifer, ConfigurationApi configurationApi, IClusterConnectionTracker tracker, ActorSystem system,
            ConfigEventDispatcher eventDispatcher)
        {
            _subscription = eventDispatcher.Get().OfType<ServerConfigurationEvent>().Select(evt => evt.Configugration)
                                           .Where(sc => !_url.Equals(sc.Database, StringComparison.Ordinal))
                                           .AutoSubscribe(sc => Url = sc.Database);

            _configuration = configuration;
            _notifer = notifer;
            _configurationApi = configurationApi;
            _tracker = tracker;
            _system = system;
            Url = _configuration.DatabaseUrl;
            IsReady = !string.IsNullOrWhiteSpace(Url);
        }

        public async Task<string> SetUrl(string url)
        {
            try
            {
                if(url.Equals(_configuration.DatabaseUrl, StringComparison.Ordinal)) return "Datenbank ist gleich";
                
                var murl = new MongoUrl(url);

                if (_tracker.IsConnected && !_tracker.IsSelf)
                {
                    var sc = await _configurationApi.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));
                    await _configurationApi.Command(new UpdateServerConfigurationCommand(sc with {Database = murl.ToString()}), TimeSpan.FromSeconds(10));
                }

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

        public async Task<UrlResult?> FetchUrl()
        {
            try
            {
                var response = await _configurationApi.QueryIsAlive(_system, TimeSpan.FromSeconds(10));
                if (!response.IsAlive) return new UrlResult("Die Api ist nicht ereichbar", false);

                var (_, _, database) = await _configurationApi.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));
                return string.IsNullOrWhiteSpace(database) ? new UrlResult("Keine Datenbank Hinterlegt", false) : new UrlResult(database, true);
            }
            catch (Exception e)
            {
                return new UrlResult(e.Message, false);
            }
        }

        public void Dispose() => _subscription.Dispose();
    }
}