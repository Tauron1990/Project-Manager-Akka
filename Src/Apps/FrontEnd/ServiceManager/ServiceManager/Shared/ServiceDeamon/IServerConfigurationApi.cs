using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IServerConfigurationApi
    {
        GlobalConfig GlobalConfig { get; set; }
    }
}