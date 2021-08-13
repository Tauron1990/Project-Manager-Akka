using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.ClusterNoteTracking)]
    public interface IClusterNodeTrackingDef
    {
        [Get(nameof(IClusterNodeTracking.GetInfo))]
        public Task<ClusterNodeInfo> GetInfo(string url);

        [Get(nameof(IClusterNodeTracking.GetUrls))]
        public Task<string[]> GetUrls();
    }
}