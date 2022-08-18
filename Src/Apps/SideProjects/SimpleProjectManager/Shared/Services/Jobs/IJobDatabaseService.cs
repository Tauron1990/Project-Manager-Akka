using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod(MinCacheDuration = 120)]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 120)]
    Task<SortOrder[]> GetSortOrders(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<long> CountActiveJobs(CancellationToken token);

    Task<string> DeleteJob(ProjectId id, CancellationToken token);

    Task<string> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token);

    Task<string> UpdateJobData(UpdateProjectCommand command, CancellationToken token);

    Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token);

    Task<string> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token);
}