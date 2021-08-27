using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Authentication;
using Stl.Fusion.Server;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.UserManagment + "/[action]")]
    [ApiController, JsonifyErrors]
    [Authorize(Claims.UserManagmaent)]
    public class UserManagmentController : ControllerBase, IUserManagement
    {
        private readonly SignInManager<IdentityUser> _user;
        private readonly IServerSideAuthService _authService;

        public UserManagmentController(SignInManager<IdentityUser> user, IServerSideAuthService authService)
        {
            _user = user;
            _authService = authService;
        }

        [HttpGet, AllowAnonymous, Publish]
        public Task<bool> NeedSetup(CancellationToken token = default)
            => throw new System.NotImplementedException();
    }
}