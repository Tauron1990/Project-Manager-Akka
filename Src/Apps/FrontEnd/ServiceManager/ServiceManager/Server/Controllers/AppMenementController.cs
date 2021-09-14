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
        public Task<NeedSetupData> NeedBasicApps()
            => _appManagment.NeedBasicApps();

        [HttpGet, Publish]
        public Task<AppList> QueryAllApps()
            => _appManagment.QueryAllApps();

        [HttpGet, Publish]
        public Task<AppInfo> QueryApp([FromQuery]string name)
            => _appManagment.QueryApp(name);

        [HttpPost]
        public Task<string> CreateNewApp([FromBody]ApiCreateAppCommand command)
            => _appManagment.CreateNewApp(command);

        [HttpPost]
        public Task<string> DeleteAppCommand([FromBody]ApiDeleteAppCommand command)
            => _appManagment.DeleteAppCommand(command);

        [HttpPost]
        public Task<RunAppSetupResponse> RunAppSetup([FromBody]RunAppSetupCommand command)
            => _appManagment.RunAppSetup(command);
    }
}