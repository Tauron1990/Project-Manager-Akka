using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services.Tasks;

public interface ITaskManager : IComputeService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<TaskList> GetTasks(CancellationToken token);

    Task<SimpleResult> DeleteTask(string id, CancellationToken token);
}