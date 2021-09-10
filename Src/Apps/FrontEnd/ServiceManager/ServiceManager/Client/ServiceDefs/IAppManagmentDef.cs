using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.Apps;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.AppManagment)]
    public interface IAppManagmentDef
    {
        [Get(nameof(NeedBasicApps))]
        Task<NeedSetupData> NeedBasicApps(CancellationToken token = default);

        [Get(nameof(QueryAllApps))]
        Task<AppList> QueryAllApps(CancellationToken token = default);
        
        [Post(nameof(RunAppSetup))]
        Task<RunAppSetupResponse> RunAppSetup([Body]RunAppSetupCommand command, CancellationToken token);
    }
}