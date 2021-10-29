using MongoDB.Driver;
using SimpleProjectManager.Server.Core.Projections;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Services;

public class JobDatabaseService : IJobDatabaseService
{
    private readonly IMongoCollection<ProjectProjection> _projects;

    public JobDatabaseService(InternalRepository database)
        => _projects = database.Collection<ProjectProjection>();

    public virtual async Task<JobInfo[]> GetActiveJobs(CancellationToken token)
    {
        var filter = Builders<ProjectProjection>.Filter.Eq(p => p.Status, ProjectStatus.Finished);

        var result = await _projects.Find(Builders<ProjectProjection>.Filter.Not(filter))
           .Project(p => new JobInfo(p.Id, p.JobName, p.Deadline, p.Ordering))
           .ToListAsync(token);

        return result.ToArray();
    }

    public virtual Task<OperationResult> CreateJob(CreateProjectCommand command, CancellationToken token)
        => throw new NotImplementedException();
}