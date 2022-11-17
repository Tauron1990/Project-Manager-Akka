using System.Collections.Immutable;
using System.Reactive.Linq;
using Akkatecture.Aggregates;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Services;

public class JobDatabaseService : IJobDatabaseService, IDisposable
{
    private readonly CommandProcessor _commandProcessor;
    private readonly MappingDatabase<DbProjectProjection, ProjectProjection> _projects;
    private readonly IDisposable _subscription;

    public JobDatabaseService(IInternalDataRepository database, CommandProcessor commandProcessor, DomainEventDispatcher dispatcher)
    {
        _commandProcessor = commandProcessor;
        _projects = new MappingDatabase<DbProjectProjection, ProjectProjection>(
            database.Databases.ProjectProjections,
            database.Databases.Mapper);

        _subscription = dispatcher.Get()
           .OfType<IDomainEvent<Project, ProjectId>>()
           .Subscribe(
                de =>
                {
                    using (Computed.Invalidate())
                    {
                        switch (de.GetAggregateEvent())
                        {
                            case ProjectFilesRemovedEvent:
                            case ProjectFilesAttachedEvent:
                                GetActiveJobs(default).Ignore();
                                GetJobData(de.AggregateIdentity, default).Ignore();

                                break;
                            case ProjectDeletedEvent:
                            case NewProjectCreatedEvent:
                                CountActiveJobs(default).Ignore();
                                GetActiveJobs(default).Ignore();
                                GetSortOrders(default).Ignore();

                                break;
                            case ProjectStatusChangedEvent:
                            case ProjectDeadLineChangedEvent:
                            case ProjectNameChangedEvent:
                                GetActiveJobs(default).Ignore();
                                GetSortOrders(default).Ignore();
                                CountActiveJobs(default).Ignore();
                                GetJobData(de.AggregateIdentity, default).Ignore();

                                break;
                        }
                    }
                });
    }

    public void Dispose()
    {
        _subscription.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual async Task<JobData> GetJobData(ProjectId id, CancellationToken token)
    {
        if(Computed.IsInvalidating()) return null!;

        var filter = _projects.Operations.Eq(p => p.Id, id.Value);
        ProjectProjection result = await _projects.ExecuteFirstAsync(_projects.Find(filter), token).ConfigureAwait(false);

        return new JobData(result.Id, result.JobName, result.Status, result.Ordering, result.Deadline, result.ProjectFiles.ToImmutableList());
    }


    public async Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token)
    {
        IOperationResult result = await _commandProcessor.RunCommand(command, token).ConfigureAwait(false);

        return result.Ok
            ? result.Outcome is bool isNew
                ? new AttachResult(SimpleResult.Success(), isNew)
                : new AttachResult(SimpleResult.Success(), IsNew: true)
            : new AttachResult(SimpleResult.Failure(result.Error ?? string.Empty), IsNew: false);
    }

    public virtual async Task<Jobs> GetActiveJobs(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return new Jobs(ImmutableList<JobInfo>.Empty);

        var filter = _projects.Operations.Eq(p => p.Status, ProjectStatus.Finished);
        // #if DEBUG
        // var projectionData = await _projects.ExecuteArray(_projects.Find(filter.Not), token);
        //
        // return new TaskList(projectionData.Select(d => new JobInfo(d.Id, d.JobName, d.Deadline, d.Status, d.ProjectFiles.Count != 0)).ToImmutableList());
        // #else
        var jobs = _projects.Find(filter.Not)
           .Select(p => new JobInfo(
                       new ProjectId(p.Id),
                       new ProjectName(p.JobName), 
                       ProjectDeadline.FromDateTime(p.Deadline),
                       p.Status,
                       p.ProjectFiles.Count != 0))
           .ToAsyncEnumerable(token);
        //#endif

        return new Jobs(await jobs.ToImmutableList(token).ConfigureAwait(false));
    }

    public virtual async Task<SortOrders> GetSortOrders(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return new SortOrders(ImmutableList<SortOrder>.Empty);

        var filter = _projects.Operations.Or(
            _projects.Operations.Eq(p => p.Status, ProjectStatus.Entered),
            _projects.Operations.Eq(p => p.Status, ProjectStatus.Pending)
        );

        var orders = _projects.ExecuteAsyncEnumerable<DbSortOrder, SortOrder>(_projects.Find(filter).Select(p => p.Ordering), token);

        return new SortOrders(await orders.ToImmutableList(token).ConfigureAwait(false));
    }

    public virtual async Task<ActiveJobs> CountActiveJobs(CancellationToken token)
    {
        if(Computed.IsInvalidating()) return ActiveJobs.From(0);

        var filter = _projects.Operations.Eq(p => p.Status, ProjectStatus.Finished);
        var result = _projects.Find(filter.Not);

        return ActiveJobs.From(await result.CountAsync(token).ConfigureAwait(false));
    }

    public async Task<SimpleResult> DeleteJob(ProjectId id, CancellationToken token)
        => SimpleResult.FromOperation(await _commandProcessor.RunCommand(id, token).ConfigureAwait(false));

    public virtual async Task<SimpleResult> CreateJob(CreateProjectCommand command, CancellationToken token)
        => SimpleResult.FromOperation(await _commandProcessor.RunCommand(command, token).ConfigureAwait(false));

    public virtual async Task<SimpleResult> ChangeOrder(SetSortOrder newOrder, CancellationToken token)
    {
        (bool ignoreIfEmpty, SortOrder? sortOrder) = newOrder;

        if(sortOrder is null)
            return ignoreIfEmpty ? SimpleResult.Success() : SimpleResult.Failure("Sortier Daten nicht zur Verfügung gestellt");

        DbOperationResult result = await _projects.UpdateOneAsync(
            _projects.Operations.Eq(p => p.Id, sortOrder.Id.Value),
            _projects.Operations.Set(p => p.Ordering, _projects.Mapper.Map<DbSortOrder>(sortOrder)),
            token).ConfigureAwait(false);

        if(result.ModifiedCount == 0)
            return SimpleResult.Failure("Das Element wurde nicht gefunden");

        using (Computed.Invalidate())
            GetSortOrders(CancellationToken.None).Ignore();

        return SimpleResult.Success();
    }

    public async Task<SimpleResult> UpdateJobData(UpdateProjectCommand command, CancellationToken token)
        => SimpleResult.FromOperation(await _commandProcessor.RunCommand(command, token).ConfigureAwait(false));

    public async Task<SimpleResult> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token)
        => SimpleResult.FromOperation(await _commandProcessor.RunCommand(command, token).ConfigureAwait(false));
}