using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Client.Pages;

public partial class TaskManager
{
    protected override async Task<PendingTask[]> ComputeState(CancellationToken cancellationToken)
        => await _taskManager.GetTasks(cancellationToken);
}