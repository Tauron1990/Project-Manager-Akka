using System.Threading;
using System.Threading.Tasks;
using RestEase;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Client.Services
{
    [BasePath("counter")]
    public interface ICounterServiceDef
    {
        [Get]
        Task<int> GetCounter(string key, CancellationToken token = default);

        [Post]
        Task Increment(IncrementCommand command, CancellationToken token = default);

        [Post]
        Task SetOffset(SetOffsetCommand command, CancellationToken token = default);
    }
}