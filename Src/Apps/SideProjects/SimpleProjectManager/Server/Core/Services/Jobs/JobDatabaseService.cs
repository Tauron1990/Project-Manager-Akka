using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Data.Events;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Server.Core.Services;

public class JobDatabaseService : IJobDatabaseService, IDisposable
{
    private readonly CommandProcessor _commandProcessor;
    private readonly IMongoCollection<ProjectProjection> _projects;
    private readonly IDisposable _subscription;

    public JobDatabaseService(InternalDataRepository database, CommandProcessor commandProcessor, DomainEventDispatcher dispatcher)
    {
        _commandProcessor = commandProcessor;
        _projects = database.Collection<ProjectProjection>();

        _subscription = dispatcher.Get().Subscribe(
            de =>
            {
                switch (de.GetAggregateEvent())
                {
                    case NewProjectCreatedEvent:
                        using (Computed.Invalidate()) 
                            GetActiveJobs(default).Ignore();
                        break;
                    case ProjectDeadLineChangedEvent:
                        using(Computed.Invalidate())
                            GetSortOrders(default).Ignore();
                        break;
                }
            });
    }

    public virtual async Task<JobInfo[]> GetActiveJobs(CancellationToken token)
    {
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Status, ProjectStatus.Finished);

        var result = await _projects.Find(Builders<ProjectProjection>.Filter.Not(filter))
           .Project(p => new JobInfo(p.Id, p.JobName, p.Deadline, p.Status, p.ProjectFiles.Count != 0))
           .ToListAsync(token);

        return result.ToArray();
    }

    public virtual async Task<SortOrder[]> GetSortOrders(CancellationToken token)
    {
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
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Id, id);
        var result = await _projects.Find(filter).FirstAsync(token);

        return new JobData(result.Id, result.JobName, result.Status, result.Ordering, result.Deadline, result.ProjectFiles);
    }

    public virtual async Task<string> CreateJob(CreateProjectCommand command, CancellationToken token)
        => (await _commandProcessor.RunCommand(command)).Error ?? string.Empty;

    public virtual async Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token)
    {
        var result = await _projects.FindOneAndUpdateAsync(
            Builders<ProjectProjection>.Filter.Eq(p => p.Id, newOrder.SortOrder.Id),
            Builders<ProjectProjection>.Update.Set(p => p.Ordering, newOrder.SortOrder),
            cancellationToken: token);

        if (result != null)
        {
            using(Computed.Invalidate())
                GetSortOrders(CancellationToken.None).Ignore();
        }

        return result != null 
            ? string.Empty 
            : "Das Element wurde nicht gefunden";
    }

    public void Dispose()
        => _subscription.Dispose();
}