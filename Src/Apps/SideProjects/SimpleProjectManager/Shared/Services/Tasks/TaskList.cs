using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Tasks;

public readonly record struct TaskList(ImmutableList<PendingTask> PendingTasks);