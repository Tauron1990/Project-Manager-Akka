using System.Threading;
using System.Threading.Tasks;
using GridMvc.Server;
using GridShared.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Server.Controllers
{
    [ApiController, JsonifyErrors]
    [Authorize(Claims.AppMenegmentClaim)]
    [Route(ControllerName.AppManagment + "/[action]")]
    public class AppMenementController : Controller, IAppManagment
    {
        private readonly IAppManagment _appManagment;

        public AppMenementController(IAppManagment appManagment)
            => _appManagment = appManagment;

        [HttpGet, Publish, AllowAnonymous]
        public Task<NeedSetupData> NeedBasicApps(CancellationToken token = default)
            => _appManagment.NeedBasicApps(token);

        [HttpGet(IAppManagment.GridItemsQuery)]
        public async Task<IActionResult> GridQueryAllApps(CancellationToken token = default)
        {
            var apps = await _appManagment.QueryAllApps(token);
            var server = new GridServer<AppInfo>(apps, QueryDictionary<StringValues>.Convert(HttpContext.Request.Query), true, "AppInfoGrid");

            return Ok(server.ItemsToDisplay);
        }
        
        [HttpGet, Publish]
        public Task<AppList> QueryAllApps(CancellationToken token = default)
            => 

        [HttpPost]
        public Task<RunAppSetupResponse> RunAppSetup([FromBody]RunAppSetupCommand command, CancellationToken token = default)
            => _appManagment.RunAppSetup(command, token);
    }
}