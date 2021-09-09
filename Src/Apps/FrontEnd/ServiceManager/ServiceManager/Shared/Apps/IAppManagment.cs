using System.Threading;
using System.Threading.Tasks;
using Stl.Fusion;

namespace ServiceManager.Shared.Apps
{
    public interface IAppManagment
    {
        [ComputeMethod]
        Task<NeedSetupData> NeedBasicApps(CancellationToken token = default);
    }
}