using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Shared.Apps
{
    public interface IAppManagment
    {
        public const string GridItemsQuery = nameof(GridItemsQuery);
        
        [ComputeMethod]
        Task<NeedSetupData> NeedBasicApps(CancellationToken token = default);

        [ComputeMethod]
        Task<AppList> QueryAllApps(CancellationToken token = default);

        Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command, CancellationToken token = default);
    }
}