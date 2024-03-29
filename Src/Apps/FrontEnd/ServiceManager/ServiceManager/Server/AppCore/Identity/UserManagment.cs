﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Identity;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Authentication.Commands;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.EntityFramework.Authentication;
using Stl.Fusion.Server.Authentication;
using Stl.Text;
using Tauron;

namespace ServiceManager.Server.AppCore.Identity
{
    [UsedImplicitly]
    public class UserManagment : DbServiceBase<UsersDatabase>, IUserManagement
    {
        private readonly IDbUserRepo<UsersDatabase, FusionUserEntity, string> _authService;

        public UserManagment(IServiceProvider services, IDbUserRepo<UsersDatabase, FusionUserEntity, string> authService)
            : base(services)
            => _authService = authService;

        public virtual async Task<bool> NeedSetup(CancellationToken token = default)
        {
            if (Computed.IsInvalidating()) return false;

            using var scope = Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            return !await repo.Users.AnyAsync(token);
        }

        public virtual Task<int> GetUserCount(CancellationToken token = default)
        {
            return UserFunc(
                async (_, userManager) =>
                {
                    if (Computed.IsInvalidating()) return 0;

                    return await userManager.Users.CountAsync(token);
                });
        }

        public virtual Task<UserData?> GetUserData(string id, CancellationToken token = default)
        {
            return UserFunc(
                async (_, userManager) =>
                {
                    var mem = await userManager.FindByIdAsync(id);

                    return mem == null ? null : new UserData(mem.Id, mem.UserName, (await userManager.GetClaimsAsync(mem)).Count);
                });
        }

        public virtual Task<UserClaim[]> GetUserClaims(string id, CancellationToken token = default)
        {
            return UserFunc(
                async (_, userManager) =>
                {
                    var usr = await userManager.FindByIdAsync(id);

                    if (usr == null) return Array.Empty<UserClaim>();

                    token.ThrowIfCancellationRequested();

                    var claims = await userManager.GetClaimsAsync(usr);

                    return claims.Where(c => Claims.AllClaims.Contains(c.Type)).Select((c, i) => new UserClaim(i, id, c.Type)).ToArray();
                });
        }

        public virtual Task<string> GetUserIdByName(string name, CancellationToken token = default)
        {
            return UserFunc(
                async (_, repo) =>
                {
                    try
                    {
                        var usr = await repo.FindByNameAsync(name);

                        return usr == null ? string.Empty : usr.Id;
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, "Error on Processing User Find");

                        return string.Empty;
                    }
                });
        }

        public virtual Task<string> SetNewPassword(SetNewPasswordCommand command, CancellationToken token = default)
        {
            return UserFunc(
                async (_, repo) =>
                {
                    try
                    {
                        var usr = await repo.FindByIdAsync(command.UserId);

                        if (usr == null) return "Benutzer nicht gefunden";
                        if (token.IsCancellationRequested) return "Abgebrochen";

                        var result = await repo.ChangePasswordAsync(usr, command.OldPassword, command.NewPassword);

                        return result.Succeeded ? string.Empty : result.Errors.First().Description;
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, "Error on Set new Password for User: {User}", command.UserId);

                        return e.Message;
                    }
                });
        }

        public virtual Task<string> SetClaims(SetClaimsCommand command, CancellationToken token = default)
        {
            return UserFunc(
                async (s, repo) =>
                {
                    try
                    {
                        var usr = await repo.FindByIdAsync(command.UserId);

                        if (usr == null) return "Benutzer nicht gefunden";
                        if (token.IsCancellationRequested) return "Abgebrochen";

                        List<Claim> toRemove = new();
                        List<Claim> toAdd = new();

                        var claims = await repo.GetClaimsAsync(usr);

                        if (token.IsCancellationRequested) return "Abgebrochen";

                        foreach (var claim in Claims.AllClaims)
                            if (claims.Any(c => c.Type == claim))
                            {
                                if (!command.Claims.Contains(claim))
                                    toRemove.Add(new Claim(claim, string.Empty));
                            }
                            else
                            {
                                if (command.Claims.Contains(claim))
                                    toAdd.Add(new Claim(claim, string.Empty));
                            }

                        foreach (var claim in toRemove)
                            await repo.RemoveClaimAsync(usr, claim);

                        foreach (var claim in toAdd)
                            await repo.AddClaimAsync(usr, claim);

                        var acessor = s.GetRequiredService<IHttpContextAccessor>();
                        if (acessor.HttpContext?.User.Identity?.IsAuthenticated == true && acessor.HttpContext.User.Identity?.Name == usr.UserName)
                            await s.GetRequiredService<ServerAuthHelper>().UpdateAuthState(acessor.HttpContext, token);

                        return string.Empty;
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, "Error on Set Claims for User: {User}", command.UserId);

                        return e.Message;
                    }
                });
        }

        public virtual Task<string> RunSetup(StartSetupCommand command, CancellationToken token = default)
        {
            return UserFunc(
                async (_, repo) =>
                {
                    string id;

                    try
                    {
                        await using var db = CreateDbContext(readWrite: true);

                        if (await db.Users.CountAsync(token) != 0)
                            return "Es sind schon User Registriert. Kein Setup erforderlich";

                        var (adminName, adminPassword) = command;

                        var newUser = new IdentityUser(adminName);
                        id = newUser.Id;

                        var result = await repo.CreateAsync(newUser, adminPassword);

                        if (!result.Succeeded)
                            return string.Join(',', result.Errors.Select(ie => ie.Description));

                        try
                        {
                            token.ThrowIfCancellationRequested();

                            var claimresult = await repo.AddClaimsAsync(newUser, Claims.AllClaims.Select(s => new Claim(s, string.Empty)));

                            if (!claimresult.Succeeded)
                                #pragma warning disable EX006
                                throw new InvalidOperationException(string.Join(',', claimresult.Errors.Select(ie => ie.Description)));
                            #pragma warning restore EX006

                            var user = await Create(command.AdminName, repo);
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
                                Log.LogError(exception, "Error on Delete Setup User while Setup Error");

                                return exception.Message;
                            }

                            throw;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, "Error on Run Setup");

                        return e.Message;
                    }

                    using (Computed.Invalidate())
                    {
                        GetUserCount(token).Ignore();
                        NeedSetup(token).Ignore();
                        GetUserData(id, token).Ignore();
                    }

                    return string.Empty;
                });
        }

        public Task<string> LogIn(TryLoginCommand command, CancellationToken token = default)
            => throw new InvalidOperationException("Only Support with Controller");

        public async Task<string> Logout(LogOutCommand command, CancellationToken token = default)
        {
            try
            {
                return await UserFunc(
                    async (s, _) =>
                    {
                        var login = s.GetRequiredService<SignInManager<IdentityUser>>();
                        var authService = s.GetRequiredService<IAuth>();
                        var contetxt = s.GetRequiredService<IHttpContextAccessor>().HttpContext;

                        if (contetxt == null) return "Kein Request";

                        if (login.IsSignedIn(contetxt.User))
                        {
                            await authService.SignOut(new SignOutCommand(command.Session, true), token);
                            await login.SignOutAsync();

                            return string.Empty;
                        }

                        return "Nicht eingeloggt";
                    });
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error on Logout User");

                return e.Message;
            }
        }

        public Task<string> Register(RegisterUserCommand command, CancellationToken token = default)
        {
            return UserFunc(
                async (_, repo) =>
                {
                    try
                    {
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
                        {
                            GetUserData(newUser.Id, token).Ignore();
                            GetUserCount(token).Ignore();
                            GetUserIdByName(command.UserName, token).Ignore();
                        }

                        return string.Empty;
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e, "Error on Register User: {User}", command.UserName);

                        return e.Message;
                    }
                });
        }

        public async Task<string> DeleteUser(DeleteUserCommand command, CancellationToken token = default)
        {
            try
            {
                return await UserFunc(
                    async (_, repo) =>
                    {
                        var usr = await repo.FindByIdAsync(command.UserId);

                        if (usr == null)
                            return "Benutzer nicht gefunden";

                        var result = await repo.DeleteAsync(usr);

                        return result.Succeeded ? string.Empty : result.Errors.First().Description;
                    });
            }
            catch (Exception e)
            {
                Log.LogError(e, "Error on Delete User: {User}", command.UserId);

                return e.Message;
            }
        }

        public static async Task<(User FusionUser, IdentityUser IdentityUser)> Create(string name, UserManager<IdentityUser> manager)
        {
            var user = await manager.FindByNameAsync(name);
            var usr = new User(new Symbol(user.Id), user.UserName)
               .WithIdentity(new UserIdentity(IdentityConstants.ApplicationScheme, user.Id));

            return ((await manager.GetClaimsAsync(user)).Aggregate(usr, (current, claim) => current.WithClaim(claim.Type, claim.Value)), user);
        }

        private async Task<TReturn> UserFunc<TReturn>(Func<IServiceProvider, UserManager<IdentityUser>, Task<TReturn>> runner)
        {
            using var scope = Services.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            return await runner(scope.ServiceProvider, manager);
        }
    }
}