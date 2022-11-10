using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod(MinCacheDuration = 120)]
    Task<Jobs> GetActiveJobs(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 120)]
    Task<SortOrders> GetSortOrders(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<ActiveJobs> CountActiveJobs(CancellationToken token);

    Task<SimpleResult> DeleteJob(ProjectId id, CancellationToken token);

    Task<SimpleResult> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<SimpleResult> ChangeOrder(SetSortOrder newOrder, CancellationToken token);

    Task<SimpleResult> UpdateJobData(UpdateProjectCommand command, CancellationToken token);

    Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token);

    Task<SimpleResult> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token);
}