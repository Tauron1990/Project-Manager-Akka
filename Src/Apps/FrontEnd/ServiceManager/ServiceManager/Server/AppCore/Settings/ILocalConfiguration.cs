using ServiceManager.Shared;

namespace ServiceManager.Server.AppCore.Settings
{
    public interface ILocalConfiguration
    {
        string DatabaseUrl { get; set; }
    }
}