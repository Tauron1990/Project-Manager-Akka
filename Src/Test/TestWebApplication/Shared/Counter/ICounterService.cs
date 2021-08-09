using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace TestWebApplication.Shared.Counter
{
    public sealed record IncrementCommand(string Key) : ICommand;

    public sealed record SetOffsetCommand(int Offset) : ICommand;
    
    public interface ICounterService
    {
        [ComputeMethod]
        Task<int> GetCounter(string key, CancellationToken token = default);

        [CommandHandler]
        Task Increment(IncrementCommand command, CancellationToken token = default);

        [CommandHandler]
        Task SetOffset(SetOffsetCommand command, CancellationToken token = default);
    }
}