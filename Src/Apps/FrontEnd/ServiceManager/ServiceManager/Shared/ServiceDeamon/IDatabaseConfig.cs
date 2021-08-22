using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.Fusion;

namespace ServiceManager.Shared.ServiceDeamon
{
    public sealed record SetUrlCommand(string Url) : ICommand<string>;

    public interface IDatabaseConfig
    {
        [ComputeMethod]
        Task<string> GetUrl();

        [ComputeMethod]
        Task<bool> GetIsReady();

        Task<UrlResult?> FetchUrl(CancellationToken token = default);
        
        Task<string> SetUrl(SetUrlCommand command, CancellationToken token = default);
    }
}