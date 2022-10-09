using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Tasks;

public class TaskManager : ITaskManager, IDisposable
{
    private readonly TaskManagerCore _taskManagerCore;
    private readonly IDisposable _subscription;

    public TaskManager(TaskManagerCore taskManagerCore, IEventAggregator aggregator)
    {
        _taskManagerCore = taskManagerCore;
        _subscription = aggregator.SubscribeTo<TasksChanged>()
           .Subscribe(
                _ =>
                {
                    using (Computed.Invalidate())
                        GetTasks(default).Ignore();
                });
    }

    public virtual async Task<PendingTask[]> GetTasks(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<PendingTask>();

        var entrys = await _taskManagerCore.GetCurrentTasks(token);

        return entrys.Select(e => new PendingTask(e.JobId, e.Name, e.Info)).ToArray();
    }

    public async Task<string> DeleteTask(string id, CancellationToken token)
    {
        try
        {
            var result = await _taskManagerCore.DeleteTask(id, token);

            if (!result.Ok)
                return result.Error ?? "Unbkannter Fehler";

            return string.Empty;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    public void Dispose()
        => _subscription.Dispose();
}