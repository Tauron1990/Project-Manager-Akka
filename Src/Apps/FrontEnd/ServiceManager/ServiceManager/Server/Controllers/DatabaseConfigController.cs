using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseConfigController : ControllerBase
    {
        private readonly IDatabaseConfigOld _config;

        public DatabaseConfigController(IDatabaseConfigOld config) => _config = config;

        [HttpGet]
        public ActionResult<StringApiContent> OnGetUrl() => new StringApiContent(_config.Url);

        [HttpGet]
        [Route(nameof(DatabaseConfigApi.IsReady))]
        public ActionResult<BoolApiContent> OnGetIsReady() => new BoolApiContent(_config.IsReady);

        [HttpPost]
        public async Task<ActionResult<StringApiContent>> OnSetDb([FromBody] StringApiContent url) 
            => new StringApiContent(await _config.SetUrl(url.Content));

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
