using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services.Tasks;

public interface ITaskManager
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<Tasks> GetTasks(CancellationToken token);

    Task<SimpleResult> DeleteTask(string id, CancellationToken token);
}