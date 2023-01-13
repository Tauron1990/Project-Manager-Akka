using Akkatecture.Jobs;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerDeleteEntryManager : JobManager<TaskManagerScheduler, TaskManagerJobRunner, TaskManagerDeleteEntry, TaskManagerJobId>
{
    public TaskManagerDeleteEntryManager(MappingDatabase<DbTaskManagerEntry, TaskManagerEntry> collection, CriticalErrorHelper errorHelper, IEventAggregator aggregator)
        : base(() => new TaskManagerScheduler(), () => new TaskManagerJobRunner(collection, errorHelper, aggregator)) { }
}