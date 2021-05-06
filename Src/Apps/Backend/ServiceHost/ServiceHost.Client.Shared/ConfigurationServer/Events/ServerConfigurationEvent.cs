using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceHost.Client.Shared.ConfigurationServer.Events
{
    public sealed record ServerConfigurationEvent(ServerConfigugration Configugration) : IConfigEvent;
}