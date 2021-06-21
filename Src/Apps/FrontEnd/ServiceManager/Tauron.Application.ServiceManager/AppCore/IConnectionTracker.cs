namespace Tauron.Application.ServiceManager.AppCore
{
    public interface IConnectionTracker
    {
        bool IsConnected { get; }

        bool IsSelf { get; }
    }
}