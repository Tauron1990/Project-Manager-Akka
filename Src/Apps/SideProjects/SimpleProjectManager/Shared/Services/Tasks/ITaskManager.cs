using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services.Tasks;

public interface ITaskManager
{
    [ComputeMethod]
    ValueTask<PendingTask[]> GetTasks(CancellationToken token);

    ValueTask<string> DeleteTask(string id, CancellationToken token);
}