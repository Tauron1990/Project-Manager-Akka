using System.Collections.Immutable;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Tasks;

public class TaskManager : ITaskManager, IDisposable
{
    private readonly IDisposable _subscription;
    private readonly TaskManagerCore _taskManagerCore;

    public TaskManager(TaskManagerCore taskManagerCore, IEventAggregator aggregator)
    {
        _taskManagerCore = taskManagerCore;
        _subscription = aggregator.SubscribeTo<TasksChanged>()
           .Subscribe(
                _ =>
                {
                    using (Computed.Invalidate())
                    {
                        GetTasks(default).Ignore();
                    }
                });
    }

    public void Dispose()
        => _subscription.Dispose();

    public virtual async Task<TaskList> GetTasks(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return TaskList.Empty;

        var entrys = await _taskManagerCore.GetCurrentTasks(token);

        return new TaskList(entrys.Select(e => new PendingTask(e.JobId, e.Name, e.Info)).ToImmutableList());
    }

    public async Task<SimpleResult> DeleteTask(string id, CancellationToken token)
    {
        try
        {
            return await _taskManagerCore.DeleteTask(id, token);
        }
        catch (Exception e)
        {
            return SimpleResult.Failure(e);
        }
    }
}