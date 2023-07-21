using System.Collections.Immutable;
using Akkatecture.Jobs;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerJobRunner : JobRunner<TaskManagerDeleteEntry, TaskManagerJobId>, IRun<TaskManagerDeleteEntry>
{
    private readonly IEventAggregator _aggregator;
    private readonly MappingDatabase<DbTaskManagerEntry, TaskManagerEntry> _collection;
    private readonly CriticalErrorHelper _errorHelper;

    public TaskManagerJobRunner(MappingDatabase<DbTaskManagerEntry, TaskManagerEntry> collection, CriticalErrorHelper errorHelper, IEventAggregator aggregator)
    {
        _collection = collection;
        _errorHelper = errorHelper;
        _aggregator = aggregator;
    }

    public bool Run(TaskManagerDeleteEntry job)
    {
        _errorHelper.Try(
                "AutoDeleteTask",
                async () => await RunDeltation(job).ConfigureAwait(false),
                default,
                () => ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty(PropertyName.From("Task to Delete"), PropertyValue.From(job.EntryId))))
           .AsTask()
           .ContinueWith(
                t =>
                {
                    if(t.IsCanceled) return;

                    if(t.IsCompletedSuccessfully)
                    {
                        if(!t.Result.IsError())
                            _errorHelper.Logger.LogWarning("Delete Job {Id} was not Successfull. {Casue}", job.EntryId, t.Result);
                    }
                    else if(t is { IsFaulted: true, Exception: not null })
                    {
                        _errorHelper.Logger.LogError(t.Exception.Unwrap()!, "Error on Try Delete Job {Id}", job.EntryId);
                    }
                })
           .Ignore();

        return true;
    }

    private async ValueTask<SimpleResult> RunDeltation(TaskManagerDeleteEntry job)
    {
        var filter = _collection.Operations.Eq(m => m.JobId, job.EntryId);
        DbOperationResult result = await _collection.DeleteOneAsync(filter).ConfigureAwait(false);

        bool success = result is { IsAcknowledged: true, DeletedCount: 1 };
        if(success) _aggregator.Publish(TasksChanged.Inst);

        return success ? SimpleResult.Success() : SimpleResult.Failure("task nicht gelöscht");
    }
}