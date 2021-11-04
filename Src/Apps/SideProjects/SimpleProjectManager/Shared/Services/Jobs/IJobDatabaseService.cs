using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [ComputeMethod]
    Task<SortOrder[]> GetSortOrders(CancellationToken token);

    [ComputeMethod]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    Task<string> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token);
}