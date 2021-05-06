namespace ServiceManager.HostInstaller.Phases
{
    public interface IHasTimeout
    {
        bool IsTimeedOut { get; }
    }
}