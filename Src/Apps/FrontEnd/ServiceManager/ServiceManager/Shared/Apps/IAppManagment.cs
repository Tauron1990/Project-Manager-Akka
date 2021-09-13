using System.Threading.Tasks;
using Stl.Fusion;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Shared.Apps
{
    public interface IAppManagment
    {
        //public const string GridItemsQuery = nameof(GridItemsQuery);
        
        [ComputeMethod]
        Task<NeedSetupData> NeedBasicApps();

        [ComputeMethod]
        Task<AppList> QueryAllApps();

        [ComputeMethod]
        Task<AppInfo> QueryApp(string name);


        Task<string> CreateNewApp(ApiCreateAppCommand command);

        Task<string> DeleteAppCommand(ApiDeleteAppCommand command);

        Task<RunAppSetupResponse> RunAppSetup(RunAppSetupCommand command);
    }
}