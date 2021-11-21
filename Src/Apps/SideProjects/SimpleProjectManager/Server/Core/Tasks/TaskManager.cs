using ReactiveUI;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Server.Core.Tasks;

public class TaskManager : ITaskManager, IDisposable
{
    private readonly TaskManagerCore _taskManagerCore;
    private readonly IDisposable _subscription;

    public TaskManager(TaskManagerCore taskManagerCore)
    {
        _taskManagerCore = taskManagerCore;
        _subscription = MessageBus.Current.Listen<TasksChanged>()
           .Subscribe(
                _ =>
                {
                    using (Computed.Invalidate())
                        GetTasks(default).Ignore();
                });
    }

    public async ValueTask<PendingTask[]> GetTasks(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<PendingTask>();

        var entrys = await _taskManagerCore.GetCurrentTasks(token);

        return entrys.Select(e => new PendingTask(e.JobId, e.Name, e.Info)).ToArray();
    }

    public async ValueTask<string> DeleteTask(string id, CancellationToken token)
    {
        try
        {
            var result = await _taskManagerCore.Delete(id, token);

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