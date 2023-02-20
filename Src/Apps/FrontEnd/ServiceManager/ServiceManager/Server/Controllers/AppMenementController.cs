using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Server.Controllers
{
    [ApiController]
    [JsonifyErrors]
    [Authorize(Claims.AppMenegmentClaim)]
    [Route(ControllerName.AppManagment + "/[action]")]
    public class AppMenementController : Controller, IAppManagment
    {
        private readonly IAppManagment _appManagment;

        public AppMenementController(IAppManagment appManagment)
            => _appManagment = appManagment;

        [HttpGet]
        [Publish]
        [AllowAnonymous]
        public Task<NeedSetupData> NeedBasicApps(CancellationToken token)
            => _appManagment.NeedBasicApps(token);

        [HttpGet]
        [Publish]
        public Task<AppList> QueryAllApps(CancellationToken token)
            => _appManagment.QueryAllApps(token);

        [HttpGet]
        [Publish]
        public Task<AppInfo> QueryApp([FromQuery]AppName name, CancellationToken token) => _appManagment.QueryApp(name, token);

        [HttpPost]
        [Publish]
        public Task<QueryRepositoryResult> QueryRepository(RepositoryName name, CancellationToken token) => _appManagment.QueryRepository(name, token);
        

        [HttpPost]
        public Task<string> CreateNewApp([FromBody] ApiCreateAppCommand command, CancellationToken token)
            => _appManagment.CreateNewApp(command, token);

        [HttpPost]
        public Task<string> DeleteAppCommand([FromBody] ApiDeleteAppCommand command, CancellationToken token)
            => _appManagment.DeleteAppCommand(command, token);

        [HttpPost]
        public Task<RunAppSetupResponse> RunAppSetup([FromBody] RunAppSetupCommand command, CancellationToken token)
            => _appManagment.RunAppSetup(command, token);
    }
}