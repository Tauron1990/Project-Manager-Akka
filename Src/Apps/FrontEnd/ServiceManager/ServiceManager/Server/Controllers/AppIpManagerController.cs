using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.AppIpManager + "/[action]")]
    [ApiController, JsonifyErrors]
    public class AppIpManagerController : ControllerBase, IAppIpManager
    {
        private readonly IAppIpManager _manager;

        public AppIpManagerController(IAppIpManager manager) 
            => _manager = manager;

        [HttpPost]
        public Task<string> WriteIp([FromBody]WriteIpCommand command, CancellationToken token = default)
            => _manager.WriteIp(command, token);

        [HttpGet, Publish]
        public Task<AppIp> GetIp()
            => _manager.GetIp();
    }
}
