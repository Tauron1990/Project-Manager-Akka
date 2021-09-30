using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Shared.Apps
{
    public interface IAppManagment
    {
        //public const string GridItemsQuery = nameof(GridItemsQuery);

        [ComputeMethod]
        Task<NeedSetupData> NeedBasicApps(CancellationToken token);

        [ComputeMethod]
        Task<AppList> QueryAllApps(CancellationToken token);

        [ComputeMethod]
        Task<AppInfo> QueryApp(string name, CancellationToken token);

        Task<QueryRepositoryResult> QueryRepository(string name, CancellationToken token);

        Task<string> CreateNewApp(ApiCreateAppCommand command, CancellationToken token);

        Task<string> DeleteAppCommand(ApiDeleteAppCommand command, CancellationToken token);

        Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command, CancellationToken token);
    }
}