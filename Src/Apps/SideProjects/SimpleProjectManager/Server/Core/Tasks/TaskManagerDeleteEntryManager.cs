﻿using System.Collections.Immutable;
using Akkatecture.Jobs;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerJobId : JobId<TaskManagerJobId>
{
    public TaskManagerJobId(string value) : base(value) { }
}

public sealed record TaskManagerDeleteEntry(string EntryId) : IJob;

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
                async () => await RunDeltation(job),
                default,
                () => ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty("Task to Delete", job.EntryId)))
           .AsTask().ContinueWith(
                t =>
                {
                    if(t.IsCanceled) return;

                    if(t.IsCompletedSuccessfully)
                    {
                        if(!t.Result.Ok)
                            _errorHelper.Logger.LogWarning("Delete Job {Id} was not Successfull", job.EntryId);
                    }
                    else if(t.IsFaulted && t.Exception != null)
                    {
                        _errorHelper.Logger.LogError(t.Exception.Unwrap()!, "Error on Try Delete Job {Id}", job.EntryId);
                    }
                });

        return true;
    }

    private async ValueTask<IOperationResult> RunDeltation(TaskManagerDeleteEntry job)
    {
        var filter = _collection.Operations.Eq(m => m.JobId, job.EntryId);
        DbOperationResult result = await _collection.DeleteOneAsync(filter);

        bool success = result.IsAcknowledged && result.DeletedCount == 1;
        if(success) _aggregator.Publish(TasksChanged.Inst);

        return success ? OperationResult.Success() : OperationResult.Failure();
    }
}

public sealed class TaskManagerScheduler : JobScheduler<TaskManagerScheduler, TaskManagerDeleteEntry, TaskManagerJobId> { }

public sealed class TaskManagerDeleteEntryManager : JobManager<TaskManagerScheduler, TaskManagerJobRunner, TaskManagerDeleteEntry, TaskManagerJobId>
{
    public TaskManagerDeleteEntryManager(MappingDatabase<DbTaskManagerEntry, TaskManagerEntry> collection, CriticalErrorHelper errorHelper, IEventAggregator aggregator)
        : base(() => new TaskManagerScheduler(), () => new TaskManagerJobRunner(collection, errorHelper, aggregator)) { }
}