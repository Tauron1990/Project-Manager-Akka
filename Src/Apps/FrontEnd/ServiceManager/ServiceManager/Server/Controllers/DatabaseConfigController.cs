using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<string> OnGetUrl() => new JsonResult(_config.Url);

        [HttpGet]
        [Route("IsReady")]
        public ActionResult<bool> OnGetIsReady() => new JsonResult(_config.IsReady);

        [HttpPost]
        public async Task<ActionResult<string>> OnSetDb([FromBody] string url) => Ok(await _config.SetUrl(url));
    }
}
