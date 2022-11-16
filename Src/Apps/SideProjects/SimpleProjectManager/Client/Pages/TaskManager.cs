using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;

namespace SimpleProjectManager.Client.Pages;

public partial class TaskManager
{
    private IState<TaskList>? _pendingTasks;

    protected override void OnInitialized()
    {
        _pendingTasks = GlobalState.Tasks.ProviderFactory();

        base.OnInitialized();
    }
}