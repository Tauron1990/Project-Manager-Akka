using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [ApiController]
    [Route(ControllerName.ClusterNoteTracking + "/[action]")]
    [JsonifyErrors]
    [Authorize(Claims.ClusterNodeClaim)]
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