using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace ServiceManager.Shared.Identity
{
    public interface IUserManagement
    {
        [ComputeMethod]
        Task<bool> NeedSetup(CancellationToken token = default);

        [ComputeMethod]
        Task<int> GetUserCount(CancellationToken token = default);

        [ComputeMethod]
        Task<UserData?> GetUserData(string id, CancellationToken token = default);

        [ComputeMethod]
        Task<UserClaim[]> GetUserClaims(string id, CancellationToken token = default);

        Task<string> SetNewPassword(SetNewPasswordCommand command, CancellationToken token = default);

        Task<string> SetClaims(SetClaimsCommand command, CancellationToken token = default);

        Task<string> RunSetup(StartSetupCommand command, CancellationToken token = default);

        Task<string> LogIn(TryLoginCommand command, CancellationToken token = default);

        Task<string> Register(RegisterUserCommand command, CancellationToken token = default);
    }
}