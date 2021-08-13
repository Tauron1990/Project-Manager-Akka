using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.ServerInfo)]
    public interface IServerInfoDef
    {
        [Get]
        Task<string> GetCurrentId();

        [Post]
        Task Restart([Body]RestartCommand command, CancellationToken token = default);
    }
}