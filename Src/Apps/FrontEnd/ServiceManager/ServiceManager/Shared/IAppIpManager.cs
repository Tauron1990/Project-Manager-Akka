using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using ServiceManager.Shared.ClusterTracking;

namespace ServiceManager.Shared
{
    public interface IAppIpManager : INotifyPropertyChanged
    {
        Task<string> WriteIp(string ip);

        AppIp Ip { get; }
    }
}