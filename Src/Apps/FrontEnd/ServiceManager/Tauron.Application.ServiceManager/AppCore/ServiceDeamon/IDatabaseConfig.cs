namespace Tauron.Application.ServiceManager.AppCore.ServiceDeamon
{
    public interface IDatabaseConfig : IInternalObject
    {
        public string Url { get; }

        public bool IsReady { get; }

        void SetUrl(string url);
    }
}