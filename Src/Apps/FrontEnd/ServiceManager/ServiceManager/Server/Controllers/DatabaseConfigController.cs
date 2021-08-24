using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.DatabaseConfigApiBase + "/[action]")]
    [ApiController, JsonifyErrors]
    [Authorize(Claims.DatabaseClaim)]
    public class DatabaseConfigController : ControllerBase, IDatabaseConfig
    {
        private readonly IDatabaseConfig _config;

        public DatabaseConfigController(IDatabaseConfig config) => _config = config;

        [HttpGet, Publish]
        public Task<string> GetUrl()
            => _config.GetUrl();

        [HttpGet, Publish, AllowAnonymous]
        public Task<bool> GetIsReady()
            => _config.GetIsReady();
        [HttpGet]
        public Task<UrlResult?> FetchUrl(CancellationToken token = default)
            => _config.FetchUrl(token);

        [HttpPost]
        public Task<string> SetUrl(SetUrlCommand command, CancellationToken token = default)
            => _config.SetUrl(command, token);
    }
}
