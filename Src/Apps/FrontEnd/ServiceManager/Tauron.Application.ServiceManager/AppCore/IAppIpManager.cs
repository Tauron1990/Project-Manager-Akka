using System.Threading.Tasks;

namespace Tauron.Application.ServiceManager.AppCore
{
    public interface IAppIpManager
    {
        void WriteIp(string ip);

        AppIp Ip { get; }
    }
}