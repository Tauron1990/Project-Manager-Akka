using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.AppManagment)]
    public interface IAppManagmentDef
    {
        [Get(nameof(NeedBasicApps))]
        Task<NeedSetupData> NeedBasicApps(CancellationToken token = default);
    }
}