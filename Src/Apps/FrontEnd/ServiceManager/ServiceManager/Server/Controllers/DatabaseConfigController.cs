﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseConfigController : ControllerBase
    {
        private readonly IDatabaseConfig _config;

        public DatabaseConfigController(IDatabaseConfig config) => _config = config;

        [HttpGet]
        public ActionResult<StringContent> OnGetUrl() => new StringContent(_config.Url);

        [HttpGet]
        [Route(nameof(DatabaseConfigApi.IsReady))]
        public ActionResult<BoolContent> OnGetIsReady() => new BoolContent(_config.IsReady);

        [HttpPost]
        public async Task<ActionResult<StringContent>> OnSetDb([FromBody] StringContent url) 
            => new StringContent(await _config.SetUrl(url.Content));

        [HttpGet]
        [Route(nameof(DatabaseConfigApi.FetchUrl))]
        public async Task<ActionResult<UrlResult>> OnGetUrlFromCluster()
        {
            var result = await _config.FetchUrl();
            if (result == null)
                return NotFound();
            return result;
        }
    }
}