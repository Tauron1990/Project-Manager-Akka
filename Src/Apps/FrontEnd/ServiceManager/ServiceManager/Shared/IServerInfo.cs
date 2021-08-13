using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;

namespace ServiceManager.Shared
{
    public sealed record RestartCommand : ICommand<Unit>;

    public interface IServerInfo
    {
        [ComputeMethod]
        Task<string> GetCurrentId(CancellationToken token = default);

        [CommandHandler]
        Task Restart(RestartCommand command, CancellationToken token = default);
    }
}