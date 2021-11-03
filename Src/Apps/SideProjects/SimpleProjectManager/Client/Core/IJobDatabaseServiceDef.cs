using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.JobsApi)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [Get(nameof(GetSortOrder))]
    Task<SortOrder> GetSortOrder([Query]ProjectId id);

    [Get(nameof(GetJobData))]
    Task<JobData> GetJobData([Query]ProjectId id, CancellationToken token);

    [Post(nameof(CreateJob))]
    Task<string> CreateJob(CreateProjectCommand command, CancellationToken token);

    [Post(nameof(ChangeOrder))]
    Task<string> ChangeOrder(ProjectId id, SortOrder newOrder, CancellationToken token);
}