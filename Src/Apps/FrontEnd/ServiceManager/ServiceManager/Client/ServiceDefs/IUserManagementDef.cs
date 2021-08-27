using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.UserManagment)]
    public interface IUserManagementDef
    {
        [Get(nameof(NeedSetup))]
        Task<bool> NeedSetup(CancellationToken token = default);
    }
}