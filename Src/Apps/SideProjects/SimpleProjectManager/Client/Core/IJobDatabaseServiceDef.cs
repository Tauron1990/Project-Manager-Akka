using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.JobsApi)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [Post(nameof(GetSortOrder))]
    Task<SortOrder> GetSortOrder([Body]ProjectId id, CancellationToken token);

    [Post(nameof(GetJobData))]
    Task<JobData> GetJobData([Body]ProjectId id, CancellationToken token);

    [Post(nameof(CreateJob))]
    Task<string> CreateJob([Body]CreateProjectCommand command, CancellationToken token);

    [Post(nameof(ChangeOrder))]
    Task<string> ChangeOrder([Body]SetSortOrder newOrder, CancellationToken token);
}