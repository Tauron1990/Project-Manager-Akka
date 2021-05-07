using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record SpecificConfigEntity(string Id, SpecificConfig Config);
}