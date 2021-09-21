using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Server.Properties;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.AppConfiguration + "/[action]")]
    [ApiController]
    [JsonifyErrors]
    [Authorize(Claims.ConfigurationClaim)]
    public class ConfigurationController : ControllerBase, IServerConfigurationApi
    {
        private readonly IServerConfigurationApi _api;

        public ConfigurationController(IServerConfigurationApi api)
            => _api = api;

        [HttpGet]
        [Publish]
        public Task<GlobalConfig> GlobalConfig()
            => _api.GlobalConfig();

        [HttpGet]
        [Publish]
        public Task<ServerConfigugration> ServerConfigugration()
            => _api.ServerConfigugration();

        [HttpGet]
        [Publish]
        public Task<ImmutableList<SpecificConfig>> QueryAppConfig()
            => _api.QueryAppConfig();

        [HttpGet]
        [Publish]
        public Task<string> QueryBaseConfig()
            => _api.QueryBaseConfig();

        [HttpGet]
        [Publish]
        public Task<string?> QueryDefaultFileContent([FromQuery] ConfigOpensElement element)
            => _api.QueryDefaultFileContent(element);

        [HttpPost]
        public Task<string> UpdateGlobalConfig([FromBody] UpdateGlobalConfigApiCommand command, CancellationToken token = default)
            => _api.UpdateGlobalConfig(command, token);

        [HttpPost]
        public Task<string> UpdateServerConfig([FromBody] UpdateServerConfiguration command, CancellationToken token = default)
            => _api.UpdateServerConfig(command, token);

        [HttpPost]
        public Task<string> DeleteSpecificConfig([FromBody] DeleteSpecificConfigCommand command, CancellationToken token = default)
            => _api.DeleteSpecificConfig(command, token);

        [HttpPost]
        public Task<string> UpdateSpecificConfig([FromBody] UpdateSpecifcConfigCommand command, CancellationToken token = default)
            => _api.UpdateSpecificConfig(command, token);


        public static string? GetConfigData(ConfigOpensElement name) => Resources.ResourceManager.GetString(name.ToString());
    }
}