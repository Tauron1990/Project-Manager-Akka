using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.AppConfiguration)]
    public interface IServerConfigurationApiDef
    {
        [Get]
        Task<GlobalConfig> GlobalConfig();

        [Get]
        Task<ServerConfigugration> ServerConfigugration();

        [Get]
        Task<ImmutableList<SpecificConfig>> QueryAppConfig();

        [Get]
        Task<string> QueryBaseConfig();
        
        [Post]
        Task<string> UpdateGlobalConfig([Body]UpdateGlobalConfigApiCommand command, CancellationToken token = default);
        
        [Post]
        Task<string> UpdateServerConfig([Body] UpdateServerConfiguration command, CancellationToken token = default);

        [Post]
        Task<string> DeleteSpecificConfig([Body] DeleteSpecificConfigCommand command, CancellationToken token = default);

        [Post]
        Task<string> UpdateSpecificConfig([Body] UpdateSpecifConfigCommand command, CancellationToken token = default);
    }
}