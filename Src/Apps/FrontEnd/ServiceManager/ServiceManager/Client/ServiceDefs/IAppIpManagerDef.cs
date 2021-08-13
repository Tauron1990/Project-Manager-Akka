using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.AppIpManager)]
    public interface IAppIpManagerDef
    {
        [Post]
        Task<string> WriteIp([Body]WriteIpCommand command, CancellationToken token = default);
        
        [Get]
        Task<AppIp> GetIp();
    }
}