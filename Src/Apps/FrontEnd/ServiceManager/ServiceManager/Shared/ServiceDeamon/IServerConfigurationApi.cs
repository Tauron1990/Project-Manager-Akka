using System.Threading.Tasks;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IServerConfigurationApi : IInternalObject
    {
        GlobalConfig GlobalConfig { get; set; }

        Task<GlobalConfig> QueryConfig();

        Task<string> Update(GlobalConfig config);
    }
}