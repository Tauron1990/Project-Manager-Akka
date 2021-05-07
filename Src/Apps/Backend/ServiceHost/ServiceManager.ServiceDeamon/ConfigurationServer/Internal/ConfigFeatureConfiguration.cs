using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.ServiceDeamon.ConfigurationServer.Data;
using SharpRepository.Repository;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed record ConfigFeatureConfiguration(ServerConfigugration Configugration, IRepository<GlobalConfigEntity, string> GlobalRepository, IRepository<SeedUrlEntity, string> Seeds, 
        IRepository<SpecificConfigEntity, string> Apps);
}