using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.ClusterConnectionTracker + "/[action]")]
    [ApiController]
    public class ClusterConnectionTrackerController : ControllerBase, IClusterConnectionTracker
    {
        private readonly IClusterConnectionTracker _tracker;

        public ClusterConnectionTrackerController(IClusterConnectionTracker tracker)
            => _tracker = tracker;

        [HttpGet, Publish]
        public Task<string> GetUrl()
            => _tracker.GetUrl();

        [HttpGet, Publish]
        public Task<bool> GetIsConnected()
            => _tracker.GetIsConnected();

        [HttpGet, Publish]
        public Task<bool> GetIsSelf()
            => _tracker.GetIsSelf();

        [HttpGet, Publish]
        public Task<AppIp> Ip()
            => _tracker.Ip();

        [HttpPost]
        public Task<string?> ConnectToCluster([FromBody]ConnectToClusterCommand command, CancellationToken token = default)
            => _tracker.ConnectToCluster(command, token);
    }
}
