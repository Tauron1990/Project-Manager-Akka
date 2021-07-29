using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using MongoDB.Driver;
using NLog;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.Settings;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ResourceHoldingObject, IDatabaseConfig
    {
        private readonly ILocalConfiguration _configuration;
        private readonly IPropertyChangedNotifer _notifer;
        private readonly ConfigurationApi _configurationApi;
        private readonly IClusterConnectionTracker _tracker;
        private readonly ActorSystem _system;
        private readonly IProcessServiceHost _processServiceHost;
        private readonly IResource<string> _url;

        public string Url => _url.Get();

        public bool IsReady { get; }

        public DatabaseConfig(ILocalConfiguration configuration, IPropertyChangedNotifer notifer, ConfigurationApi configurationApi, IClusterConnectionTracker tracker, ActorSystem system,
            ConfigEventDispatcher eventDispatcher, IProcessServiceHost processServiceHost)
        {
            _url = CreateResource(configuration.DatabaseUrl, nameof(Url));

            (from evt in eventDispatcher.Get().OfType<ServerConfigurationEvent>()
             let config = evt.Configugration
             where !Url.Equals(config.Database, StringComparison.Ordinal)
             from r in _url.Set(config.Database)
             select r).AutoSubscribe()
                      .DisposeWith(this);

            _configuration = configuration;
            _notifer = notifer;
            _configurationApi = configurationApi;
            _tracker = tracker;
            _system = system;
            _processServiceHost = processServiceHost;
            IsReady = !string.IsNullOrWhiteSpace(Url);
        }

        public async Task<string> SetUrl(string url)
        {
            try
            {
                if(url.Equals(_configuration.DatabaseUrl, StringComparison.Ordinal)) return "Datenbank ist gleich";
                
                var murl = new MongoUrl(url);

                if (_tracker.IsConnected)
                {
                    var canSend = false;

                    try
                    {
                        var result = await _processServiceHost.TryStart(url);
                        canSend = result.IsRunning;
                    }
                    catch (Exception e)
                    {
                        LogManager.GetCurrentClassLogger().Error(e, "Error on Try Start api");
                    }

                    if (canSend)
                    {
                        var sc = await _configurationApi.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));
                        await _configurationApi.Command(new UpdateServerConfigurationCommand(sc with {Database = murl.ToString()}), TimeSpan.FromSeconds(10));
                    }
                }

                await _url.Set(url);
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
                return e.Message switch
                {
                    Reporter.TimeoutError => "Die Operation hat zu lange dedauert",
                    _ => e.Message
                };
            }
        }

        public async Task<UrlResult?> FetchUrl()
        {
            try
            {
                if (_tracker.IsSelf)
                    return new UrlResult("Mit Keinem Cluster Verbunden", false);

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
    }
}