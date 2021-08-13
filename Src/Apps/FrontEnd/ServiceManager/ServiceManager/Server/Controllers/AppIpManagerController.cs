using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.AppIpManager + "/[action]")]
    [ApiController]
    public class AppIpManagerController : ControllerBase, IAppIpManager
    {
        private readonly IAppIpManager _manager;

        public AppIpManagerController(IAppIpManager manager) 
            => _manager = manager;

        [HttpPost]
        public Task<string> WriteIp([FromBody]WriteIpCommand command, CancellationToken token = default)
            => _manager.WriteIp(command, token);

        [HttpGet]
        public Task<AppIp> GetIp()
            => _manager.GetIp();
    }
}
