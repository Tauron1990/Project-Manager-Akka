using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GridBlazor;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Shared.Identity;
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Text;

namespace ServiceManager.Server.AppCore.Identity
{
    public class UserManagment : Stl.Fusion.EntityFramework.DbServiceBase<UsersDatabase>, IUserManagement
    {
        public static async Task<(User FusionUser, IdentityUser IdentityUser)> Create(string name, UserManager<IdentityUser> manager)
        {
            var user = await manager.FindByNameAsync(name);
            var usr = new User(new Symbol(user.Id), user.UserName)
                     .WithIdentity(new UserIdentity(IdentityConstants.ApplicationScheme, user.Id));

            return ((await manager.GetClaimsAsync(user)).Aggregate(usr, (current, claim) => current.WithClaim(claim.Type, claim.Value)), user);
        }

        private readonly IDbUserRepo<UsersDatabase, FusionUserEntity, string> _authService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagment(IServiceProvider services, IDbUserRepo<UsersDatabase, FusionUserEntity, string> authService, UserManager<IdentityUser> userManager)
            : base(services)
        {
            _authService = authService;
            _userManager = userManager;
        }
        
        public virtual async Task<bool> NeedSetup(CancellationToken token = default)
        {
            if (Computed.IsInvalidating()) return false;

            using var scope = Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            return !await repo.Users.AnyAsync(token);
        }

        public virtual async Task<int> GetUserCount(CancellationToken token = default)
        {
            if (Computed.IsInvalidating()) return 0;

            return await _userManager.Users.CountAsync(token);
        }

        public virtual async Task<string> RunSetup(StartSetupCommand command, CancellationToken token = default)
        {
            using var scope = Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            try
            {
                await using var db = CreateDbContext(true);

                if (await AsyncEnumerable.CountAsync(db.Users, token) != 0)
                    return "Es sind schon User Registriert. Kein Setup erforderlich";

                var (adminName, adminPassword) = command;

                var newUser = new IdentityUser(adminName);

                var result = await repo.CreateAsync(newUser, adminPassword);

                if (!result.Succeeded)
                    return string.Join(',', result.Errors.Select(ie => ie.Description));

                try
                {

                    token.ThrowIfCancellationRequested();

                    var claimresult = await repo.AddClaimsAsync(newUser, Claims.AllClaims.Select(s => new Claim(s, string.Empty)));

                    if (!claimresult.Succeeded)
                        throw new InvalidOperationException(string.Join(',', claimresult.Errors.Select(ie => ie.Description)));

                    var user = await Create(command.AdminName, _userManager);
                    await _authService.Create(db, user.FusionUser, token);
                }
                catch
                {
                    try
                    {
                        await repo.DeleteAsync(newUser);
                    }
                    catch (Exception exception)
                    {
                        return exception.Message;
                    }

                    throw;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            using (Computed.Invalidate())
            {
                GetUserCount(token).Ignore();
                NeedSetup(token).Ignore();
            }

            return string.Empty;
        }

        public Task<string> LogIn(TryLoginCommand command, CancellationToken token = default)
            => throw new InvalidOperationException("Only Support with Controller");

        public async Task<string> Register(RegisterUserCommand command, CancellationToken token = default)
        {
            try
            {
                using var scope = Services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var duplicate = await repo.FindByNameAsync(command.UserName);

                if (duplicate != null)
                    return "User schon Registriert";
                token.ThrowIfCancellationRequested();

                var newUser = new IdentityUser(command.UserName);
                var result = await repo.CreateAsync(newUser, command.Password);

                if (!result.Succeeded)
                    return string.Join(",", result.Errors.Select(ie => ie.Description));

                await repo.AddClaimsAsync(newUser, new[] { new Claim(Claims.ClusterNodeClaim, string.Empty), new Claim(Claims.ClusterConnectionClaim, string.Empty) });

                using (Computed.Invalidate())
                    GetUserCount(token).Ignore();

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
    }
}