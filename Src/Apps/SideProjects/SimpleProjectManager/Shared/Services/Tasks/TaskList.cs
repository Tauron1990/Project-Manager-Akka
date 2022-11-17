using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Tasks;

public readonly record struct TaskList(ImmutableList<PendingTask> PendingTasks)
{
    public static readonly TaskList Empty = new(ImmutableList<PendingTask>.Empty);
}