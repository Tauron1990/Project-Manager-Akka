using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IServerConfigurationApi : IInternalObject
    {
        GlobalConfig GlobalConfig { get; }

        ServerConfigugration ServerConfigugration { get; }

        Task<GlobalConfig> QueryConfig();

        Task<ServerConfigugration> QueryServerConfig();

        Task<string> Update(GlobalConfig config);

        Task<string> QueryBaseConfig();

        Task<string> Update(ServerConfigugration serverConfigugration);
    }
}