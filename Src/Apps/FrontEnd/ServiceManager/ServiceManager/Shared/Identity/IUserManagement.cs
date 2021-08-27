using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace ServiceManager.Shared.Identity
{
    public interface IUserManagement
    {
        [ComputeMethod]
        Task<bool> NeedSetup(CancellationToken token = default);
    }
}