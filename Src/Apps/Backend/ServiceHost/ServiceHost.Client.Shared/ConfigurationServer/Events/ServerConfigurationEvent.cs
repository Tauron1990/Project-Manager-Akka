using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Events;

public sealed record ServerConfigurationEvent(ServerConfigugration Configugration) : IConfigEvent;