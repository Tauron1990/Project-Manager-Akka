using System.Threading.Tasks;
using Stl.Fusion;

namespace ServiceManager.Shared.ClusterTracking
{
    public interface IClusterNodeTracking
    {
        [ComputeMethod]
        Task<ClusterNodeInfo> GetInfo(string url);

        [ComputeMethod]
        Task<string[]> GetUrls();
    }
}