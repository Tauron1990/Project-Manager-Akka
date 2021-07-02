using ServiceManager.Shared;

namespace ServiceManager.Server.AppCore.Settings
{
    public interface ILocalConfiguration : IInternalObject
    {
        string DatabaseUrl { get; set; }
    }
}