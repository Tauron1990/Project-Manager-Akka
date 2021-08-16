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
        [Get(nameof(GetUrl))]
        Task<string> GetUrl();

        [Get(nameof(GetIsConnected))]
        Task<bool> GetIsConnected();

        [Get(nameof(GetIsSelf))]
        Task<bool> GetIsSelf();

        [Get(nameof(Ip))]
        Task<AppIp> Ip();

        [Post(nameof(ConnectToCluster))]
        Task<string?> ConnectToCluster([Body]ConnectToClusterCommand command, CancellationToken token = default);
    }
}