using System.Threading;
using System.Threading.Tasks;
using RestEase;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Client.Services
{
    [BasePath("counter")]
    public interface ICounterServiceDef
    {
        [Get(nameof(GetCounter))]
        Task<int> GetCounter(string key, CancellationToken token = default);

        [Post(nameof(Increment))]
        Task Increment([Body]IncrementCommand command, CancellationToken token = default);

        [Post(nameof(SetOffset))]
        Task SetOffset([Body]SetOffsetCommand command, CancellationToken token = default);
    }
}