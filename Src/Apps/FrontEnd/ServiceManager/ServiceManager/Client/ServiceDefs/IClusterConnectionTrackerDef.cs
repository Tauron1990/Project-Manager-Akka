using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.ClusterConnectionTracker)]
    public interface IClusterConnectionTrackerDef
    {
        [Get]
        Task<string> GetUrl();

        [Get]
        Task<bool> GetIsConnected();

        [Get]
        Task<bool> GetIsSelf();

        [Get]
        Task<AppIp> Ip();

        [Post]
        Task<string?> ConnectToCluster([Body]ConnectToClusterCommand command, CancellationToken token = default);
    }
}