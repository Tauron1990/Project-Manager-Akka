using Akkatecture.Jobs;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerScheduler : JobScheduler<TaskManagerScheduler, TaskManagerDeleteEntry, TaskManagerJobId> { }