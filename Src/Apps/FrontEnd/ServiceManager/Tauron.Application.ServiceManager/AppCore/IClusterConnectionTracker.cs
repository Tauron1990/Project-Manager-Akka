namespace Tauron.Application.ServiceManager.AppCore
{
    public interface IClusterConnectionTracker : IInternalObject
    {
        bool IsConnected { get; }

        bool IsSelf { get; }

        AppIp Ip { get; }
    }
}