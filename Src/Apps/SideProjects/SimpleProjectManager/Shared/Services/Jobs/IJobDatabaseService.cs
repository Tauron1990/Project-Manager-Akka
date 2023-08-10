using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService : IComputeService
{
    [ComputeMethod(MinCacheDuration = 120)]
    Task<Jobs> GetActiveJobs(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 120)]
    Task<SortOrders> GetSortOrders(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<ActiveJobs> CountActiveJobs(CancellationToken token);

    Task<SimpleResultContainer> DeleteJob(ProjectId id, CancellationToken token);

    Task<SimpleResultContainer> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<SimpleResultContainer> ChangeOrder(SetSortOrder newOrder, CancellationToken token);

    Task<SimpleResultContainer> UpdateJobData(UpdateProjectCommand command, CancellationToken token);

    Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token);

    Task<SimpleResultContainer> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token);
}