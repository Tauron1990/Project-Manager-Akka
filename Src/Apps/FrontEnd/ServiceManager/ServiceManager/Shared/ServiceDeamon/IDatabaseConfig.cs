namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IDatabaseConfig : IInternalObject
    {
        public string Url { get; }

        public bool IsReady { get; }

        bool SetUrl(string url);
    }
}