using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppIpManagerController : ControllerBase
    {
        private readonly IAppIpManager _manager;

        public AppIpManagerController(IAppIpManager manager) 
            => _manager = manager;

        [HttpGet]
        public ActionResult<AppIp> OnGet()
            => _manager.Ip;

        [HttpPost]
        public async Task<IActionResult> OnWrite([FromBody] string ip)
        {
            try
            {
                return Ok(await _manager.WriteIp(ip));
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        public async Task<IActionResult>
    }
}
