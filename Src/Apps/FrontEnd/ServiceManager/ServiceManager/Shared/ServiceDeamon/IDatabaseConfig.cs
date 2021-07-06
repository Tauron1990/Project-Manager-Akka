using System.Threading.Tasks;

namespace ServiceManager.Shared.ServiceDeamon
{
    public interface IDatabaseConfig : IInternalObject
    {
        public string Url { get; }

        public bool IsReady { get; }

        Task<string> SetUrl(string url);

        Task<UrlResult?> FetchUrl();
    }
}