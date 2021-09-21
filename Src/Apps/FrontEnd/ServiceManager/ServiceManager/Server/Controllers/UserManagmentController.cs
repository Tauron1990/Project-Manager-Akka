using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GridMvc.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;
using Stl.Fusion.Authentication;
using Stl.Fusion.Server;
using Stl.Fusion.Server.Authentication;

namespace ServiceManager.Server.Controllers
{
    [Route(ControllerName.UserManagment + "/[action]")]
    [ApiController]
    [JsonifyErrors]
    [Authorize(Claims.UserManagmaentClaim)]
    public class UserManagmentController : ControllerBase, IUserManagement
    {
        private readonly ServerAuthHelper _helper;
        private readonly ILogger<UserManagmentController> _log;
        private readonly SignInManager<IdentityUser> _logInManager;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _principalFactory;
        private readonly ISessionProvider _provider;
        private readonly ISessionFactory _sessionFactory;
        private readonly IUserManagement _userManagement;
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagmentController(
            SignInManager<IdentityUser> logInManager, IUserManagement userManagement, UserManager<IdentityUser> userManager, ServerAuthHelper helper,
            IUserClaimsPrincipalFactory<IdentityUser> principalFactory, ISessionProvider provider, ISessionFactory sessionFactory, ILogger<UserManagmentController> log)
        {
            _logInManager = logInManager;
            _userManagement = userManagement;
            _userManager = userManager;
            _helper = helper;
            _principalFactory = principalFactory;
            _provider = provider;
            _sessionFactory = sessionFactory;
            _log = log;
        }

        [HttpGet]
        [AllowAnonymous]
        [Publish]
        public Task<bool> NeedSetup(CancellationToken token = default)
            => _userManagement.NeedSetup(token);

        [HttpGet]
        [Publish]
        public Task<int> GetUserCount(CancellationToken token = default)
            => _userManagement.GetUserCount(token);

        [HttpGet]
        public Task<UserData?> GetUserData([FromQuery] string id, CancellationToken token = default)
            => _userManagement.GetUserData(id, token);

        [HttpGet]
        [Publish]
        public Task<UserClaim[]> GetUserClaims([FromQuery] string id, CancellationToken token)
            => _userManagement.GetUserClaims(id, token);

        [HttpGet]
        [Publish]
        public Task<string> GetUserIdByName([FromQuery] string name, CancellationToken token = default)
            => _userManagement.GetUserIdByName(name, token);

        [HttpPost]
        public Task<string> SetNewPassword([FromBody] SetNewPasswordCommand command, CancellationToken token = default)
            => _userManagement.SetNewPassword(command, token);

        [HttpPost]
        public Task<string> SetClaims([FromBody] SetClaimsCommand command, CancellationToken token = default)
            => _userManagement.SetClaims(command, token);

        [HttpPost]
        [AllowAnonymous]
        public Task<string> RunSetup([FromBody] StartSetupCommand command, CancellationToken token = default)
            => _userManagement.RunSetup(command, token);

        [HttpPost]
        [AllowAnonymous]
        public async Task<string> LogIn([FromBody] TryLoginCommand command, CancellationToken token = default)
        {
            try
            {
                var (_, identityUser) = await UserManagment.Create(command.UserName, _userManager);

                var result = await _logInManager.PasswordSignInAsync(command.UserName, command.Password, isPersistent: true, lockoutOnFailure: false);

                if (!result.Succeeded || HttpContext == null) return result.Succeeded ? string.Empty : "Login nicht erfolgreich";

                HttpContext.User = await _principalFactory.CreateAsync(identityUser);
                await _helper.UpdateAuthState(HttpContext, token);

                return result.Succeeded ? string.Empty : "Login nicht erfolgreich";
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error on Log in User");

                return e.Message;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public Task<string> Logout([FromBody] LogOutCommand command, CancellationToken token = default)
            => _userManagement.Logout(command, token);

        [HttpPost]
        [AllowAnonymous]
        public Task<string> Register([FromBody] RegisterUserCommand command, CancellationToken token = default)
            => _userManagement.Register(command, token);

        [HttpPost]
        public Task<string> DeleteUser([FromBody] DeleteUserCommand command, CancellationToken token = default)
            => _userManagement.DeleteUser(command, token);

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = new List<UserData>();

            foreach (var user in _userManager.Users)
            {
                var claimsCount = (await _userManager.GetClaimsAsync(user)).Count;
                users.Add(new UserData(user.Id, user.UserName, claimsCount));
            }

            var server = new GridServer<UserData>(users, Request.Query, renderOnlyRows: true, "UsersGrid");

            return Ok(server.ItemsToDisplay);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserClaims(string userId)
        {
            var claims = await _userManager.GetClaimsAsync(await _userManager.FindByIdAsync(userId));

            var server = new GridServer<UserClaim>(
                claims
                   .Where(c => Claims.AllClaims.Contains(c.Type))
                   .Select((c, i) => new UserClaim(i, userId, c.Type)),
                Request.Query,
                renderOnlyRows: true,
                "UserClaimGrid");

            return Ok(server.ItemsToDisplay);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<string>> UpdateSession([FromQuery] string session, CancellationToken token = default)
        {
            _provider.Session = new Session(session);
            await _helper.UpdateAuthState(HttpContext, token);

            return HttpContext.User.Identity?.IsAuthenticated == true ? session : string.Empty;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<string>> LogoutSession(CancellationToken token = default)
        {
            await _logInManager.SignOutAsync();

            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _provider.Session = _sessionFactory.CreateSession();
            await _helper.UpdateAuthState(HttpContext, token);

            return _provider.Session.Id.Value;
        }
    }
}