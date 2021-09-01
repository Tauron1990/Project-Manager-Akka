using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Authentication;
using Stl.Fusion.Server;
using Stl.Fusion.Server.Authentication;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.UserManagment + "/[action]")]
    [ApiController, JsonifyErrors]
    [Authorize(Claims.UserManagmaent)]
    public class UserManagmentController : ControllerBase, IUserManagement
    {
        private readonly SignInManager<IdentityUser> _logInManager;
        private readonly IUserManagement _userManagement;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ServerAuthHelper _helper;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _principalFactory;
        private readonly ISessionProvider _provider;
        private readonly ISessionFactory _sessionFactory;

        public UserManagmentController(SignInManager<IdentityUser> logInManager, IUserManagement userManagement, UserManager<IdentityUser> userManager, ServerAuthHelper helper,
                                       IUserClaimsPrincipalFactory<IdentityUser> principalFactory, ISessionProvider provider, ISessionFactory sessionFactory)
        {
            _logInManager = logInManager;
            _userManagement = userManagement;
            _userManager = userManager;
            _helper = helper;
            _principalFactory = principalFactory;
            _provider = provider;
            _sessionFactory = sessionFactory;
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<string>> UpdateSession([FromQuery] string session, CancellationToken token = default)
        {
            _provider.Session = new Session(session);
            await _helper.UpdateAuthState(HttpContext, token);

            return session;
        }

        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<string>> LogoutSession(CancellationToken token = default)
        {
            await _logInManager.SignOutAsync();

            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _provider.Session = _sessionFactory.CreateSession();
            await _helper.UpdateAuthState(HttpContext, token);

            return _provider.Session.Id.Value;
        }

        [HttpGet, AllowAnonymous, Publish]
        public Task<bool> NeedSetup(CancellationToken token = default)
            => _userManagement.NeedSetup(token);

        [HttpPost, AllowAnonymous]
        public Task<string> RunSetup([FromBody]StartSetupCommand command, CancellationToken token = default)
            => _userManagement.RunSetup(command, token);

        [HttpPost, AllowAnonymous]
        public async Task<string> LogIn([FromBody]TryLoginCommand command, CancellationToken token = default)
        {
            try
            {
                var usr = await UserManagment.Create(command.UserName, _userManager);
                
                var result = await _logInManager.PasswordSignInAsync(command.UserName, command.Password, true, false);

                if (result.Succeeded && HttpContext != null)
                {
                    HttpContext.User = await _principalFactory.CreateAsync(usr.IdentityUser);
                    await _helper.UpdateAuthState(HttpContext, token);
                }

                return result.Succeeded ? string.Empty : "Login nicht erfolgreich";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [HttpPost, AllowAnonymous]
        public Task<string> Register([FromBody]RegisterUserCommand command, CancellationToken token = default)
            => _userManagement.Register(command, token);
    }
}