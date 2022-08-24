using System.Reactive.Linq;
using Akkatecture.Aggregates;
using MongoDB.Driver;
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
    private readonly IMongoCollection<ProjectProjection> _projects;
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
                            DataChanged().Ignore();
                            break;
                        case NewProjectCreatedEvent:
                            CountActiveJobs(default).Ignore();
                            GetActiveJobs(default).Ignore();
                            break;
                        case ProjectDeletedEvent:
                        case ProjectDeadLineChangedEvent:
                        case ProjectNameChangedEvent:
                        case ProjectStatusChangedEvent:
                            DataChanged().Ignore();
                            break;
                    }
                }

                Task DataChanged()
                    => Task.WhenAny(
                        CountActiveJobs(default),
                        GetActiveJobs(default),
                        GetJobData(de.AggregateIdentity, default));
            });
    }

    public virtual async Task<JobInfo[]> GetActiveJobs(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<JobInfo>();
        
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Status, ProjectStatus.Finished);

        var result = await _projects.Find(Builders<ProjectProjection>.Filter.Not(filter))
           .Project(p => new JobInfo(p.Id, p.JobName, p.Deadline, p.Status, p.ProjectFiles.Count != 0))
           .ToListAsync(token);

        return result.ToArray();
    }

    public virtual async Task<SortOrder[]> GetSortOrders(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return Array.Empty<SortOrder>();
        
        var builder = Builders<ProjectProjection>.Filter;
        var filter = builder.Or
            (
                builder.Eq(p => p.Status, ProjectStatus.Entered),
                builder.Eq(p => p.Status, ProjectStatus.Pending)
            );

        var list = await _projects.Find(filter).Project(p => p.Ordering).ToListAsync(token);

        return list.ToArray();
    }

    public virtual async Task<JobData> GetJobData(ProjectId id, CancellationToken token)
    {
        if (Computed.IsInvalidating()) return null!;
        
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Id, id);
        var result = await _projects.Find(filter).FirstAsync(token);

        return new JobData(result.Id, result.JobName, result.Status, result.Ordering, result.Deadline, result.ProjectFiles);
    }

    public virtual async Task<long> CountActiveJobs(CancellationToken token)
    {
        if (Computed.IsInvalidating()) return 0;
        
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Status, ProjectStatus.Finished);
        var result = _projects.Find(Builders<ProjectProjection>.Filter.Not(filter));

        return await result.CountDocumentsAsync(token);
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

        var result = await _projects.FindOneAndUpdateAsync(
            Builders<ProjectProjection>.Filter.Eq(p => p.Id, sortOrder.Id),
            Builders<ProjectProjection>.Update.Set(p => p.Ordering, sortOrder),
            cancellationToken: token);

        if (result is not null)
        {
            using(Computed.Invalidate())
                GetSortOrders(CancellationToken.None).Ignore();
        }
        
        return result is not null 
            ? string.Empty 
            : "Das Element wurde nicht gefunden";
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