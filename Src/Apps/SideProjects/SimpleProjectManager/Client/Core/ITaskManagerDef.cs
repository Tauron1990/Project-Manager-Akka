using RestEase;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.TaskApi)]
public interface ITaskManagerDef
{
    [Get(nameof(GetTasks))]
    Task<PendingTask[]> GetTasks(CancellationToken token);

    [Post(nameof(DeleteTask))]
    Task<string> DeleteTask([Body]string id, CancellationToken token);
}