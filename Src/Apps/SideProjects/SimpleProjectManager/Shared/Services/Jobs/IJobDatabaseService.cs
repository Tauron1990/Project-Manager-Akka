using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod(KeepAliveTime = 12 * 60 * 60)]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [ComputeMethod(KeepAliveTime = 12 * 60 * 60)]
    Task<SortOrder[]> GetSortOrders(CancellationToken token);

    [ComputeMethod]
    Task<JobData> GetJobData(ProjectId id, CancellationToken token);

    Task<string> DeleteJob(ProjectId id, CancellationToken token);

    Task<string> CreateJob(CreateProjectCommand command, CancellationToken token);

    Task<string> ChangeOrder(SetSortOrder newOrder, CancellationToken token);

    Task<string> UpdateJobData(UpdateProjectCommand command, CancellationToken token);

    Task<AttachResult> AttachFiles(ProjectAttachFilesCommand command, CancellationToken token);

    Task<string> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token);
}