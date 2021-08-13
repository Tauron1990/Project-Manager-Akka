using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace ServiceManager.Shared.ClusterTracking
{
    public sealed record ConnectToClusterCommand(string Url) : ICommand<string?>;
    
    public interface IClusterConnectionTracker
    {
        [ComputeMethod]
        Task<string> GetUrl();

        [ComputeMethod]
        Task<bool> GetIsConnected();

        [ComputeMethod]
        Task<bool> GetIsSelf();

        [ComputeMethod]
        Task<AppIp> Ip();

        [CommandHandler]
        Task<string?> ConnectToCluster(ConnectToClusterCommand command, CancellationToken token = default);
    }
}