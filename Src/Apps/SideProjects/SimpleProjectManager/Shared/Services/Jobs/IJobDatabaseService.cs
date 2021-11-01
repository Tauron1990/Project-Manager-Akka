using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [ComputeMethod]
    Task<SortOrder> GetSortOrder(ProjectId id, CancellationToken token);

    [ComputeMethod]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    Task<ApiResult> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<ApiResult> ChangeOrder(SetSortOrder newOrder, CancellationToken token);
}