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


        [Get(nameof(QueryApp))]
        Task<AppInfo> QueryApp([Query] string name, CancellationToken token);

        [Post(nameof(CreateNewApp))]
        Task<string> CreateNewApp([Body] ApiCreateAppCommand command, CancellationToken token);

        [Post(nameof(CreateNewApp))]
        Task<string> DeleteAppCommand([Body] ApiDeleteAppCommand command, CancellationToken token);

        [Post(nameof(RunAppSetup))]
        Task<RunAppSetupResponse> RunAppSetup([Body] RunAppSetupCommand command, CancellationToken token);
    }
}