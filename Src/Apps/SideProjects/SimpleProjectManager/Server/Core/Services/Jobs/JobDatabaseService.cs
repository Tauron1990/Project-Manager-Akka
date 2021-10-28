using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Server.Core.Services;

public class JobDatabaseService : IJobDatabaseService
{
    public Task<JobInfo[]> GetActiveJobs(CancellationToken token)
        => throw new NotImplementedException();

    public Task<OperationResult> CreateJob(CreateProjectCommand command, CancellationToken token)
        => throw new NotImplementedException();
}