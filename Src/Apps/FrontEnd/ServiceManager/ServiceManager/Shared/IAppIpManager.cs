using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Shared.ClusterTracking;
using Stl.CommandR;
using Stl.Fusion;

namespace ServiceManager.Shared
{
    public sealed record WriteIpCommand(string Ip) : ICommand<string>;

    public interface IAppIpManager
    {
        Task<string> WriteIp(WriteIpCommand command, CancellationToken token = default);

        [ComputeMethod]
        Task<AppIp> GetIp();
    }
}