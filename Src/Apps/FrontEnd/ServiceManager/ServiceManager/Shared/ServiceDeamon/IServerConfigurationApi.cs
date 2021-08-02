using System.Collections.Immutable;
using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IServerConfigurationApi : IInternalObject
    {
        GlobalConfig GlobalConfig { get; }

        ServerConfigugration ServerConfigugration { get; }

        Task<ImmutableList<SpecificConfig>> QueryAppConfig();

        Task<GlobalConfig> QueryConfig();

        Task<ServerConfigugration> QueryServerConfig();

        Task<string> Update(GlobalConfig config);

        Task<string> QueryBaseConfig();

        Task<string> Update(ServerConfigugration serverConfigugration);

        Task<string> DeleteSpecificConfig(string name);

        Task<string> Update(SpecificConfigData data);
    }
}