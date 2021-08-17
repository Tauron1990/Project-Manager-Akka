using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Shared.Api;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace ServiceManager.Shared.ServiceDeamon
{
    public sealed record UpdateGlobalConfigApiCommand(GlobalConfig Config) : ICommand<string>;

    public sealed record UpdateServerConfiguration(ServerConfigugration ServerConfigugration) : ICommand<string>;

    public sealed record DeleteSpecificConfigCommand(string Name) : ICommand<string>;

    public sealed record UpdateSpecifConfigCommand(SpecificConfigData Data) : ICommand<string>;
    
    public interface IServerConfigurationApi
    {
        [ComputeMethod]
        Task<GlobalConfig> GlobalConfig();

        [ComputeMethod]
        Task<ServerConfigugration> ServerConfigugration();

        [ComputeMethod]
        Task<ImmutableList<SpecificConfig>> QueryAppConfig();

        [ComputeMethod]
        Task<string> QueryBaseConfig();

        [ComputeMethod]
        Task<string?> QueryDefaultFileContent(ConfigOpensElement element);
        
        [CommandHandler]
        Task<string> UpdateGlobalConfig(UpdateGlobalConfigApiCommand command, CancellationToken token = default);
        
        [CommandHandler]
        Task<string> UpdateServerConfig(UpdateServerConfiguration command, CancellationToken token = default);

        [CommandHandler]
        Task<string> DeleteSpecificConfig(DeleteSpecificConfigCommand command, CancellationToken token = default);

        [CommandHandler]
        Task<string> UpdateSpecificConfig(UpdateSpecifConfigCommand command, CancellationToken token = default);
    }
}