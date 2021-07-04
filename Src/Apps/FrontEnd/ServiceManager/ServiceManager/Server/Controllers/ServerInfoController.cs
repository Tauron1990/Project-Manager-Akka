using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Api;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerInfoController : ControllerBase
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRestartHelper _restart;
        private readonly IHubContext<ClusterInfoHub> _hub;
        private static readonly Guid CurrentInstance = Guid.NewGuid();

        public ServerInfoController(IHostApplicationLifetime lifetime, IRestartHelper restart, IHubContext<ClusterInfoHub> hub)
        {
            _lifetime = lifetime;
            _restart = restart;
            _hub = hub;
        }

        [HttpGet]
        public IActionResult GetId()
            => new JsonResult(CurrentInstance);

        [HttpPost]
        public async Task<IActionResult> OnRestart()
        {
            await _hub.Clients.All.SendAsync(HubEvents.RestartServer);
            _restart.Restart = true;
            _lifetime.StopApplication();
            return Ok();
        }
    }
}
