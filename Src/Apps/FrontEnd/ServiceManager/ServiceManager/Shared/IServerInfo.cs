using System.Threading.Tasks;

namespace ServiceManager.Shared
{
    public interface IServerInfo
    {
        Task Restart();

        Task<string?> TryReconnect();
    }
}