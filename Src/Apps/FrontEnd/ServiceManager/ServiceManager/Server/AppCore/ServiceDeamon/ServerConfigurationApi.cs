﻿using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using NLog;
using ServiceHost.Client.Shared.ConfigurationServer;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceHost.Client.Shared.ConfigurationServer.Events;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;

namespace ServiceManager.Server.AppCore.ServiceDeamon
{
    public sealed class ServerConfigurationApi : ResourceHoldingObject, IServerConfigurationApi
    {
        private readonly ConfigurationApi _api;
        private readonly ActorSystem _system;
        private readonly IProcessServiceHost _processService;
        private readonly ILogger<ServerConfigurationApi> _log;
        private readonly IResource<GlobalConfig> _globalConfig;
        private readonly IResource<ServerConfigugration> _serverConfigugration;

        public GlobalConfig GlobalConfig => _globalConfig.Get();

        public ServerConfigugration ServerConfigugration => _serverConfigugration.Get();

        public ServerConfigurationApi(ConfigurationApi api, ActorSystem system, ConfigEventDispatcher dispatcher, IPropertyChangedNotifer notifer, 
            IProcessServiceHost processService, ILogger<ServerConfigurationApi> log)
        {
            _api = api;
            _system = system;
            IPropertyChangedNotifer notifer1 = notifer;
            _processService = processService;
            _log = log;

            _globalConfig = CreateResource(
                new GlobalConfig(string.Empty, string.Empty), 
                nameof(GlobalConfig), 
                () => notifer1.SendPropertyChanged<IServerConfigurationApi>(nameof(GlobalConfig)));

            (from evt in dispatcher.Get().OfType<GlobalConfigEvent>()
             from r in _globalConfig.Set(evt.Action == ConfigDataAction.Delete
                 ? new GlobalConfig(string.Empty, string.Empty)
                 : evt.Config)
             select r).AutoSubscribe(e => log.LogError(e, "Error on Update Global Configuration"))
                      .DisposeWith(this);


            _serverConfigugration = CreateResource(
                new ServerConfigugration(false, false, string.Empty),
                nameof(ServerConfigugration),
                () => notifer1.SendPropertyChanged<IServerConfigurationApi>(nameof(ServerConfigugration)));

            (from evt in dispatcher.Get().OfType<ServerConfigurationEvent>()
             from r in _serverConfigugration.Set(evt.Configugration)
             select r)
               .AutoSubscribe(e => log.LogError(e, "Error on"))
               .DisposeWith(this);
        }

        public Task<GlobalConfig> QueryConfig()
            => _globalConfig.Init(async () =>
                                  {

                                      try
                                      {
                                          await EnsureConfigAlive();

                                          return await _api.Query<QueryGlobalConfig, GlobalConfig>(TimeSpan.FromSeconds(10));
                                      }
                                      catch (Exception e)
                                      {
                                          _log.LogError(e, "Error on Query Global Config");
                                          if (e.Message == ConfigError.NoGlobalConfigFound)
                                              return new GlobalConfig(string.Empty, null);
                                          throw;
                                      }
                                  });

        public Task<ServerConfigugration> QueryServerConfig()
            => _serverConfigugration.Init(async () =>
                                          {
                                              try
                                              {
                                                  await EnsureConfigAlive();

                                                  return await _api.Query<QueryServerConfiguration, ServerConfigugration>(TimeSpan.FromSeconds(10));
                                              }
                                              catch (Exception e)
                                              {
                                                  _log.LogError(e, "Error on Query ServerConfiguration");
                                                  throw;
                                              }
                                          });

        private async Task EnsureConfigAlive()
        {
            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                throw new InvalidOperationException(resonse);
        }

        public async Task<string> Update(GlobalConfig config)
        {
            if (config == GlobalConfig)
                return "Die Konfiguration ist gleich";

            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            bool needUpdate;

            try
            {
                await _api.Query<QueryGlobalConfig, GlobalConfig>(TimeSpan.FromSeconds(10));
                needUpdate = true;
            }
            catch (Exception e)
            {
                if (e.Message == ConfigError.NoGlobalConfigFound)
                    needUpdate = false;
                else
                    throw;
            }
            
            await _api.Command(new UpdateGlobalConfigCommand(needUpdate ? ConfigDataAction.Update : ConfigDataAction.Create, config), TimeSpan.FromSeconds(20));
            return string.Empty;
        }

        public Task<string> QueryBaseConfig()
            => Task.FromResult(Properties.Resources.BaseConfig);

        public async Task<string> Update(ServerConfigugration serverConfigugration)
        {
            if (ServerConfigugration == serverConfigugration)
                return "Die Konfiguration ist gleich";

            var resonse = await _processService.ConfigAlive(_api, _system);
            if (!string.IsNullOrWhiteSpace(resonse))
                return "Der Service wurde nicht gestartet";

            await _api.Command(new UpdateServerConfigurationCommand(serverConfigugration), TimeSpan.FromSeconds(20));

            return string.Empty;
        }
    }
}