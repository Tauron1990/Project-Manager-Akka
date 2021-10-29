using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.JobsApi)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [Post(nameof(CreateJob))]
    Task<OperationResult> CreateJob(CreateProjectCommand command, CancellationToken token);
}