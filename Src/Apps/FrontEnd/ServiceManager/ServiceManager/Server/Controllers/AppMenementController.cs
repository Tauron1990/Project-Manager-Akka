using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Server;

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
    }
}