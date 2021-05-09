using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record ServerConfigurationEntity(string Id, ServerConfigugration Configugration)
    {
        public ServerConfigurationEntity()
            : this(string.Empty, new ServerConfigugration(false, false, false))
        {
            
        }
    }
}