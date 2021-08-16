using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.AppConfiguration + "/[action]")]
    [ApiController, JsonifyErrors]
    public class ConfigurationController : ControllerBase, IServerConfigurationApi
    {
        private readonly IServerConfigurationApi _apiOld;

        public ConfigurationController(IServerConfigurationApi apiOld) => _apiOld = apiOld;



        public static string?            GetConfigData(string name) => string.IsNullOrWhiteSpace(name) ? null : Resources.ResourceManager.GetString(name);
        
        [HttpGet, Publish]
        public        Task<GlobalConfig> GlobalConfig()
            => _apiOld.GlobalConfig();

        [HttpGet, Publish]
        public Task<ServerConfigugration> ServerConfigugration()
            => _apiOld.ServerConfigugration();

        [HttpGet, Publish]
        public Task<ImmutableList<SpecificConfig>> QueryAppConfig()
            => _apiOld.QueryAppConfig();

        [HttpGet, Publish]
        public Task<string> QueryBaseConfig()
            => _apiOld.QueryBaseConfig();

        [HttpPost]
        public Task<string> UpdateGlobalConfig([FromBody]UpdateGlobalConfigApiCommand command, CancellationToken token = default)
            => _apiOld.UpdateGlobalConfig(command, token);

        [HttpPost]
        public Task<string> UpdateServerConfig([FromBody] UpdateServerConfiguration command, CancellationToken token = default)
            => _apiOld.UpdateServerConfig(command, token);

        [HttpPost]
        public Task<string> DeleteSpecificConfig([FromBody] DeleteSpecificConfigCommand command, CancellationToken token = default)
            => _apiOld.DeleteSpecificConfig(command, token);

        [HttpPost]
        public Task<string> UpdateSpecificConfig([FromBody] UpdateSpecifConfigCommand command, CancellationToken token = default)
            => _apiOld.UpdateSpecificConfig(command, token);
    }
}
