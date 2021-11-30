using RestEase;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Core;

[BasePath(ApiPaths.JobsApi)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [Get(nameof(GetSortOrders))]
    Task<SortOrder[]> GetSortOrders(CancellationToken token);

    [Get(nameof(GetJobData))]
    Task<JobData> GetJobData([Query]ProjectId id, CancellationToken token);

    [Post(nameof(CreateJob))]
    Task<string> CreateJob([Body]CreateProjectCommand command, CancellationToken token);

    [Post(nameof(ChangeOrder))]
    Task<string> ChangeOrder([Body]SetSortOrder newOrder, CancellationToken token);

    [Post(nameof(UpdateJobData))]
    Task<string> UpdateJobData([Body] UpdateProjectCommand command, CancellationToken token);
    
    [Post(nameof(AttachFiles))]
    Task<string> AttachFiles([Body]ProjectAttachFilesCommand command, CancellationToken token);

    [Post(nameof(RemoveFiles))]
    Task<string> RemoveFiles(ProjectRemoveFilesCommand command, CancellationToken token);
}