using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Server.AppCore
{
    public interface IAppIpManager
    {
        void WriteIp(string ip);

        AppIp Ip { get; }
    }
}