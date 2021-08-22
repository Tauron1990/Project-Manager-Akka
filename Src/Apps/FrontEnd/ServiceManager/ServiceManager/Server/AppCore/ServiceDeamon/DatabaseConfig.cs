using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
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
using Stl.Async;
using Stl.Fusion;
using Tauron;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public class DatabaseConfig : ResourceHoldingObject, IDatabaseConfig
    {
        private readonly ILocalConfiguration _configuration;
        private readonly ConfigurationApi _configurationApi;
        private readonly IClusterConnectionTracker _tracker;
        private readonly ActorSystem _system;
        private readonly IProcessServiceHost _processServiceHost;
        private readonly IResource<string> _url;
        private readonly IResource<bool> _isReady;
        
        public DatabaseConfig(ILocalConfiguration configuration, ConfigurationApi configurationApi, IClusterConnectionTracker tracker, ActorSystem system,
            ConfigEventDispatcher eventDispatcher, IProcessServiceHost processServiceHost)
        {
            _url = CreateResource(configuration.DatabaseUrl, onSet: () =>
                                                                    {
                                                                        using (Computed.Invalidate()) GetUrl().Ignore();

                                                                        return Task.CompletedTask;
                                                                    });

            (from evt in eventDispatcher.Get().OfType<ServerConfigurationEvent>()
             let config = evt.Configugration
             where !_url.Get().Equals(config.Database, StringComparison.Ordinal)
             from r in _url.Set(config.Database)
             select r).AutoSubscribe()
                      .DisposeWith(this);

            _configuration = configuration;
            _configurationApi = configurationApi;
            _tracker = tracker;
            _system = system;
            _processServiceHost = processServiceHost;
            _isReady = CreateResource(!string.IsNullOrWhiteSpace(_url.Get()), onSet: () =>
                                                                                     {
                                                                                         using (Computed.Invalidate()) GetIsReady().Ignore();

                                                                                         return Task.CompletedTask;
                                                                                     });
        }

        public virtual Task<string> GetUrl()
            => Task.FromResult(_url.Get());

        public virtual Task<bool> GetIsReady()
            => Task.FromResult(_isReady.Get());
        
        public virtual async Task<string> SetUrl(SetUrlCommand command, CancellationToken token = default)
        {
            try
            {
                command.Deconstruct(out var url);
                
                if(url.Equals(_configuration.DatabaseUrl, StringComparison.Ordinal)) return "Datenbank ist gleich";
                
                var murl = new MongoUrl(url);

                if (await _tracker.GetIsConnected())
                {
                    var canSend = false;

                    try
                    {
                        var result = await _processServiceHost.TryStart(url);

                        if (token.IsCancellationRequested) return string.Empty;
                        canSend = result.IsRunning;
                    }
                    catch (Exception e)
                    {
                        LogManager.GetCurrentClassLogger().Error(e, "Error on Try Start api");
                    }

                    if (canSend)
                    {
                        var sc = await _configurationApi.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));

                        if (token.IsCancellationRequested) return string.Empty;
                        await _configurationApi.Command(new UpdateServerConfigurationCommand(sc with {Database = murl.ToString()}), TimeSpan.FromSeconds(10));
                    }
                }

                await _url.Set(url, token);
                
                var targetFile = AkkaConfigurationBuilder.Main.FileInAppDirectory();
                var config = targetFile.ReadTextIfExis();

                config = AkkaConfigurationBuilder.ApplyMongoUrl(config, Resources.BaseConfig, murl.ToString());
                await File.WriteAllTextAsync(targetFile, config, token);

                _configuration.DatabaseUrl = url;

                await _isReady.Set(true, token);
                
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

        public virtual async Task<UrlResult?> FetchUrl(CancellationToken token)
        {
            try
            {
                if (await _tracker.GetIsSelf())
                    return new UrlResult("Mit Keinem Cluster Verbunden", false);

                if (token.IsCancellationRequested) return null;
                
                var response = await _configurationApi.QueryIsAlive(_system, TimeSpan.FromSeconds(10));
                if (!response.IsAlive) return new UrlResult("Die Api ist nicht ereichbar", false);

                if (token.IsCancellationRequested) return null;
                
                var (_, _, database) = await _configurationApi.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));

                if (token.IsCancellationRequested) return null;
                return string.IsNullOrWhiteSpace(database) ? new UrlResult("Keine Datenbank Hinterlegt", false) : new UrlResult(database, true);
            }
            catch (Exception e)
            {
                return new UrlResult(e.Message, false);
            }
        }
    }
}