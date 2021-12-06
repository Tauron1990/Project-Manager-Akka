using RestEase;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.JobsApi)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    [Get(nameof(GetSortOrders))]
    Task<SortOrder[]> GetSortOrders(CancellationToken token);

    [Get(nameof(GetJobData))]
    Task<JobData> GetJobData([Query]ProjectId id, CancellationToken token);

    [Get(nameof(CountActiveJobs))]
    Task<long> CountActiveJobs(CancellationToken token);
    
    [Post(nameof(CreateJob))]
    Task<string> CreateJob([Body]CreateProjectCommand command, CancellationToken token);

    [Post(nameof(ChangeOrder))]
    Task<string> ChangeOrder([Body]SetSortOrder newOrder, CancellationToken token);

    [Post(nameof(UpdateJobData))]
    Task<string> UpdateJobData([Body] UpdateProjectCommand command, CancellationToken token);
    
    [Post(nameof(AttachFiles))]
    Task<AttachResult> AttachFiles([Body]ProjectAttachFilesCommand command, CancellationToken token);

    [Post(nameof(RemoveFiles))]
    Task<string> RemoveFiles([Body]ProjectRemoveFilesCommand command, CancellationToken token);
}