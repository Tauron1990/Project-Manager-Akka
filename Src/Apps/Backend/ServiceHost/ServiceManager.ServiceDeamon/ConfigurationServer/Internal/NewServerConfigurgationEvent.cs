using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Internal
{
    public sealed record NewServerConfigurgationEvent(ServerConfigugration Target);
}