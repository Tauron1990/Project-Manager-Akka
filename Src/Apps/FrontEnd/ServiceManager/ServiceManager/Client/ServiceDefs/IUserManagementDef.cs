using System.Threading;
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

        [Post(nameof(RunSetup))]
        Task<string> RunSetup([Body]StartSetupCommand command, CancellationToken cancellation = default);

        [Post(nameof(LogIn))]
        Task<string> LogIn([Body]TryLoginCommand command, CancellationToken token = default);

        [Post(nameof(Register))]
        Task<string> Register([Body]RegisterUserCommand command, CancellationToken token = default);
    }
}