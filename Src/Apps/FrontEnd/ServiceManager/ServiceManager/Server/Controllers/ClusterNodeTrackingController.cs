using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [ApiController]
    [Route(ControllerName.ClusterNoteTracking + "/[action]")]
    [JsonifyErrors]
    public class ClusterNodeTrackingController : Controller, IClusterNodeTracking
    {
        private readonly IClusterNodeTracking _clusterNodeTracking;

        public ClusterNodeTrackingController(IClusterNodeTracking clusterNodeTracking)
            => _clusterNodeTracking = clusterNodeTracking;

        [HttpGet, Publish]
        public Task<ClusterNodeInfo> GetInfo(string url)
            => _clusterNodeTracking.GetInfo(url);

        [HttpGet, Publish]
        public Task<string[]> GetUrls()
            => _clusterNodeTracking.GetUrls();
    }
}