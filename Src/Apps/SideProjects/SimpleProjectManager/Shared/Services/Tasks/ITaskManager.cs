using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services.Tasks;

public interface ITaskManager
{
    [ComputeMethod]
    Task<PendingTask[]> GetTasks(CancellationToken token);

    Task<string> DeleteTask(string id, CancellationToken token);
}