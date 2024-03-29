﻿using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Identity;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.UserManagment)]
    public interface IUserManagementDef
    {
        [Get(nameof(GetUserCount))]
        Task<int> GetUserCount(CancellationToken token = default);

        [Get(nameof(NeedSetup))]
        Task<bool> NeedSetup(CancellationToken token = default);

        [Get(nameof(GetUserData))]
        Task<UserData> GetUserData([Query] string id, CancellationToken token = default);

        [Get(nameof(GetUserClaims))]
        Task<UserClaim[]> GetUserClaims([Query] string id, CancellationToken tokem = default);

        [Get(nameof(GetUserIdByName))]
        Task<string> GetUserIdByName([Query] string name, CancellationToken token = default);

        [Post(nameof(SetNewPassword))]
        Task<string> SetNewPassword([Body] SetNewPasswordCommand command, CancellationToken token = default);

        [Post(nameof(SetClaims))]
        Task<string> SetClaims([Body] SetClaimsCommand command, CancellationToken token = default);

        [Post(nameof(RunSetup))]
        Task<string> RunSetup([Body] StartSetupCommand command, CancellationToken cancellation = default);

        [Post(nameof(LogIn))]
        Task<string> LogIn([Body] TryLoginCommand command, CancellationToken token = default);

        [Post(nameof(Logout))]
        Task<string> Logout([Body] LogOutCommand command, CancellationToken token = default);

        [Post(nameof(Register))]
        Task<string> Register([Body] RegisterUserCommand command, CancellationToken token = default);

        [Post(nameof(DeleteUser))]
        Task<string> DeleteUser([Body] DeleteUserCommand command, CancellationToken token = default);
    }
}