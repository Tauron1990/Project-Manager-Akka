using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClusterConnectionTrackerController : ControllerBase
    {
        private readonly IClusterConnectionTracker _tracker;

        public ClusterConnectionTrackerController(IClusterConnectionTracker tracker) => _tracker = tracker;

        [HttpGet]
        public ActionResult<AppIp> GetIp()
            => _tracker.Ip;

        [HttpGet]
        [Route(nameof(ClusterConnectionTrackerApi.IsSelf))]
        public ActionResult<BoolContent> GetIsSelf()
            => new BoolContent(_tracker.IsSelf);

        [HttpGet]
        [Route(nameof(ClusterConnectionTrackerApi.IsConnected))]
        public ActionResult<BoolContent> GetIsConnected()
            => new BoolContent(_tracker.IsConnected);

        [HttpGet]
        [Route(nameof(ClusterConnectionTrackerApi.SelfUrl))]
        public ActionResult<StringApiContent> GetSelfUrl()
            => new StringApiContent(_tracker.Url);
    }
}
