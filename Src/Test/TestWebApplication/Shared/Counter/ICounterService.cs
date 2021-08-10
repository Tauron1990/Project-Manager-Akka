using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace TestWebApplication.Shared.Counter
{
    public sealed record IncrementCommand(string Key) : ICommand<Unit>;

    public sealed record SetOffsetCommand(int Offset) : ICommand<Unit>;
    
    public interface ICounterService
    {
        [ComputeMethod]
        Task<int> GetCounter(string key, CancellationToken token = default);
        
        Task Increment(IncrementCommand command, CancellationToken token = default);
        
        Task SetOffset(SetOffsetCommand command, CancellationToken token = default);
    }
}