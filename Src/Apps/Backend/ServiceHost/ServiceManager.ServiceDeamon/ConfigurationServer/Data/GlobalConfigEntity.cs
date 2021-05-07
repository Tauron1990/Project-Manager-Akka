using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record GlobalConfigEntity(string Id, GlobalConfig Config);
}