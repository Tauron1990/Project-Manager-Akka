using System.Threading;
using System.Threading.Tasks;
using RestEase;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;

namespace ServiceManager.Client.ServiceDefs
{
    [BasePath(ControllerName.DatabaseConfigApiBase)]
    public interface IDatabaseConfigDef
    {
        [Get(nameof(GetUrl))]
        Task<string> GetUrl();

        [Get(nameof(GetIsReady))]
        Task<bool> GetIsReady();

        [Get(nameof(FetchUrl))]
        Task<UrlResult?> FetchUrl(CancellationToken token = default);

        [Post(nameof(SetUrl))]
        Task<string> SetUrl([Body] SetUrlCommand command, CancellationToken token = default);
    }
}