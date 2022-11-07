using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services.Tasks;

public readonly record struct Tasks(ImmutableList<PendingTask> PendingTasks);