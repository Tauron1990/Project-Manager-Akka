using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.ServerInfo + "/[action]")]
    [ApiController, JsonifyErrors]
    [Authorize(Claims.ServerInfoClaim)]
    public class ServerInfoController : ControllerBase, IServerInfo
    {
        private readonly IServerInfo _serverInfo;

        public ServerInfoController(IServerInfo serverInfo)
            => _serverInfo = serverInfo;

        [HttpGet, Publish]
        public Task<string> GetCurrentId(CancellationToken token)
            => _serverInfo.GetCurrentId(token);

        [HttpPost]
        public Task Restart(RestartCommand command, CancellationToken token = default)
            => _serverInfo.Restart(command, token);
    }
}
