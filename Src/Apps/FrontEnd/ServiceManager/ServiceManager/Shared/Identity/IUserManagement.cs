using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace ServiceManager.Shared.Identity
{
    public interface IUserManagement
    {
        [ComputeMethod]
        Task<bool> NeedSetup(CancellationToken token = default);

        Task<string> RunSetup(StartSetupCommand command, CancellationToken token = default);

        Task<string> LogIn(TryLoginCommand command, CancellationToken token = default);

        Task<string> Register(RegisterUserCommand command, CancellationToken token = default);
    }
}