using System.Reactive.Linq;
using Akkatecture.Aggregates;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;

namespace SimpleProjectManager.Server.Core.Services;

public class JobDatabaseService : IJobDatabaseService, IDisposable
{
    private readonly CommandProcessor _commandProcessor;
    private readonly IDatabaseCollection<ProjectProjection> _projects;
    private readonly IDisposable _subscription;

    public JobDatabaseService(IInternalDataRepository database, CommandProcessor commandProcessor, DomainEventDispatcher dispatcher)
    {
        _commandProcessor = commandProcessor;
        _projects = database.Collection<ProjectProjection>();

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

    public virtual async Task<JobInfo[]> GetActiveJobs(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<JobInfo>();

        var filter = _projects.Operations.Eq(p => p.Status, ProjectStatus.Finished);
        #if DEBUG
        var projectionData = await _projects.Find(filter).ToArrayAsync(token);
        
        return projectionData.Select(d => new JobInfo(d.Id, d.JobName, d.Deadline, d.Status, d.ProjectFiles.Count != 0)).ToArray();
        #else
        return await _projects.Find(filter.Not)
           .Project(p => new JobInfo(p.Id, p.JobName, p.Deadline, p.Status, p.ProjectFiles.Count != 0))
           .ToArrayAsync(token);
        #endif
    }

    public virtual async Task<SortOrder[]> GetSortOrders(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<SortOrder>();
        
        var filter = _projects.Operations.Or
            (
                _projects.Operations.Eq(p => p.Status, ProjectStatus.Entered),
                _projects.Operations.Eq(p => p.Status, ProjectStatus.Pending)
            );

        return await _projects.Find(filter).Project(p => p.Ordering).ToArrayAsync(token);
    }

    public virtual async Task<JobData> GetJobData(ProjectId id, CancellationToken token)
    {
        if (Computed.IsInvalidating()) return null!;
        
        var filter = _projects.Operations.Eq(p => p.Id, id);
        var result = await _projects.Find(filter).FirstAsync(token);

        return new JobData(result.Id, result.JobName, result.Status, result.Ordering, result.Deadline, result.ProjectFiles);
    }

    public virtual async Task<long> CountActiveJobs(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return 0;
        
        var filter = _projects.Operations.Eq(p => p.Status, ProjectStatus.Finished);
        var result = _projects.Find(filter.Not);

        return await result.CountAsync(token);
    }

    public async Task<string> DeleteJob(ProjectId id, CancellationToken token)
        => (await _commandProcessor.RunCommand(id, token)).Error ?? string.Empty;

    public virtual async Task<string> CreateJob(CreateProjectCommand command, CancellationToken token)
        => (await _commandProcessor.RunCommand(command, token)).Error ?? string.Empty;

    public virtual async Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token)
    {
        var (ignoreIfEmpty, sortOrder) = newOrder;

        if (sortOrder is null)
            return ignoreIfEmpty ? string.Empty : "Sortier Daten nicht zur Verfügung gestellt";

        var result = await _projects.UpdateOneAsync(
            _projects.Operations.Eq(p => p.Id, sortOrder.Id),
            _projects.Operations.Set(p => p.Ordering, sortOrder),
            cancellationToken: token);

        if(result.ModifiedCount == 0)
            return "Das Element wurde nicht gefunden";

        using(Computed.Invalidate())
            GetSortOrders(CancellationToken.None).Ignore();

        return string.Empty;
    }

    public async Task<string> UpdateJobData(UpdateProjectCommand command, CancellationToken token)
        => (await _commandProcessor.RunCommand(command, token)).Error ?? string.Empty;
    

    public async Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token)
    {
        var result = await _commandProcessor.RunCommand(command, token);

        return result.Ok
            ? result.Outcome is bool isNew
                ? new AttachResult(string.Empty, isNew)
                : new AttachResult(string.Empty, true)
            : new AttachResult(result.Error ?? string.Empty, false);
    }

    public async Task<string> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token)
        => (await _commandProcessor.RunCommand(command, token)).Error ?? string.Empty;

    public void Dispose()
    {
        _subscription.Dispose();
        GC.SuppressFinalize(this);
    }
}