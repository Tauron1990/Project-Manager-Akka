using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IServerConfigurationApi _api;

        public ConfigurationController(IServerConfigurationApi api) => _api = api;

        [HttpPost]
        [Route(nameof(ConfigurationRestApi.UpdateSpecificConfigiration))]
        public async Task<ActionResult<StringApiContent>> UpdateSpecificConfiguration([FromBody] SpecificConfigData data)
            => new StringApiContent(await _api.Update(data));
        
        [HttpDelete]
        [Route(nameof(ConfigurationRestApi.DeleteSpecificConfig) + "/{toDelete}")]
        public async Task<ActionResult<StringApiContent>> DeleteSpecificConfig(string toDelete)
            => new StringApiContent(await _api.DeleteSpecificConfig(toDelete));
        
        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GetAppConfigList))]
        public async Task<ActionResult<SpecificConfigList>> GetSpecificConfigList()
            => new SpecificConfigList(await _api.QueryAppConfig());
        
        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GlobalConfig))]
        public async Task<ActionResult<GlobalConfig>> GetGlobalConfig()
            => await _api.QueryConfig();

        [HttpPost]
        [Route(nameof(ConfigurationRestApi.GlobalConfig))]
        public async Task<ActionResult<StringApiContent>> PostGlobalConfig([FromBody] GlobalConfig config)
        {
            try
            {
                return new StringApiContent(await _api.Update(config));
            }
            catch (Exception e)
            {
                Log.Warn(e, "Error on Update Global Config");
                return new StringApiContent(e.Message);
            }
        }

        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GetConfigFile) + "/{name}")]
        public ActionResult<StringApiContent> GetConfigFile(string name)
        {
            var value = GetConfigData(name);

            if (string.IsNullOrWhiteSpace(value)) return NotFound();

            return new StringApiContent(value);
        }

        [HttpGet]
        [Route(nameof(ConfigurationRestApi.GetBaseConfig))]
        public async Task<ActionResult<StringApiContent>> GetBaseConfig() 
            => new StringApiContent(await _api.QueryBaseConfig());

        [HttpGet]
        [Route(nameof(ConfigurationRestApi.ServerConfiguration))]
        public Task<ServerConfigugration> GetServerConfiguration()
            => _api.QueryServerConfig();

        [HttpPost]
        [Route(nameof(ConfigurationRestApi.ServerConfiguration))]
        public async Task<StringApiContent> SetServerConfiguration([FromBody]ServerConfigugration serverConfigugration)
        {
            try
            {
                var result = await _api.Update(serverConfigugration);
                return new StringApiContent(result);
            }
            catch (Exception e)
            {
                Log.Warn(e, "Error on Update Server Configuration");
                return new StringApiContent(e.Message);
            }
        }

        public static string? GetConfigData(string name) => string.IsNullOrWhiteSpace(name) ? null : Resources.ResourceManager.GetString(name);
    }
}
