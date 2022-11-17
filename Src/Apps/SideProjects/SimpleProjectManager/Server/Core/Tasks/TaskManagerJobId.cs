using Akkatecture.Jobs;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerJobId : JobId<TaskManagerJobId>
{
    public TaskManagerJobId(string value) : base(value) { }
}