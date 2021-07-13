﻿using System;
using System.Threading.Tasks;
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
        public ActionResult<BoolApiContent> GetIsSelf()
            => new BoolApiContent(_tracker.IsSelf);

        [HttpGet]
        [Route(nameof(ClusterConnectionTrackerApi.IsConnected))]
        public ActionResult<BoolApiContent> GetIsConnected()
            => new BoolApiContent(_tracker.IsConnected);

        [HttpGet]
        [Route(nameof(ClusterConnectionTrackerApi.SelfUrl))]
        public ActionResult<StringApiContent> GetSelfUrl()
            => new StringApiContent(_tracker.Url);

        [HttpPost]
        [Route(nameof(ClusterConnectionTrackerApi.ConnectToCluster))]
        public async Task<ActionResult<StringApiContent>> ConnectToCluster([FromBody] StringApiContent url)
        {
            try
            {
                await _tracker.ConnectToCluster(url.Content);
                return new StringApiContent(string.Empty);
            }
            catch (Exception e)
            {
                return new StringApiContent(e.Message);
            }
        }
    }
}
