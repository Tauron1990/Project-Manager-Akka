using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod(KeepAliveTime = 12 * 60 * 60)]
    ValueTask<JobInfo[]> GetActiveJobs(CancellationToken token);

    [ComputeMethod(KeepAliveTime = 12 * 60 * 60)]
    ValueTask<SortOrder[]> GetSortOrders(CancellationToken token);

    [ComputeMethod]
    ValueTask<JobData> GetJobData(ProjectId id, CancellationToken token);

    ValueTask<string> CreateJob(CreateProjectCommand command, CancellationToken token);

    ValueTask<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token);

    ValueTask<string> UpdateJobData(UpdateProjectCommand command, CancellationToken token);
}