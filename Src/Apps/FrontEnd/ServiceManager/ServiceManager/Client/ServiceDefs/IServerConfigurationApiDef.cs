using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.AppConfiguration)]
    public interface IServerConfigurationApiDef
    {
        [Get(nameof(GlobalConfig))]
        Task<GlobalConfig> GlobalConfig();

        [Get(nameof(ServerConfigugration))]
        Task<ServerConfigugration> ServerConfigugration();

        [Get(nameof(QueryAppConfig))]
        Task<ImmutableList<SpecificConfig>> QueryAppConfig();

        [Get(nameof(QueryBaseConfig))]
        Task<string> QueryBaseConfig();

        [Get(nameof(QueryDefaultFileContent))]
        Task<string?> QueryDefaultFileContent([Query] ConfigOpensElement element);

        [Post(nameof(UpdateGlobalConfig))]
        Task<string> UpdateGlobalConfig([Body] UpdateGlobalConfigApiCommand command, CancellationToken token = default);

        [Post(nameof(UpdateServerConfig))]
        Task<string> UpdateServerConfig([Body] UpdateServerConfiguration command, CancellationToken token = default);

        [Post(nameof(DeleteSpecificConfig))]
        Task<string> DeleteSpecificConfig([Body] DeleteSpecificConfigCommand command, CancellationToken token = default);

        [Post(nameof(UpdateSpecificConfig))]
        Task<string> UpdateSpecificConfig([Body] UpdateSpecifcConfigCommand command, CancellationToken token = default);
    }
}