using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.AppIpManager + "/[action]")]
    [ApiController, JsonifyErrors]
    [Authorize(Claims.AppIpClaim)]
    public class AppIpManagerController : ControllerBase, IAppIpManager
    {
        private readonly IAppIpManager _manager;

        public AppIpManagerController(IAppIpManager manager) 
            => _manager = manager;

        [HttpPost]
        public Task<string> WriteIp([FromBody]WriteIpCommand command, CancellationToken token = default)
            => _manager.WriteIp(command, token);

        [HttpGet, Publish, AllowAnonymous]
        public Task<AppIp> GetIp()
            => _manager.GetIp();
    }
}
