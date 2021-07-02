namespace Tauron.Application.ServiceManager.AppCore.Settings
{
    public interface ILocalConfiguration : IInternalObject
    {
        string DatabaseUrl { get; set; }
    }
}