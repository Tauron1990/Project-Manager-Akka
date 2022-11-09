using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.JobsApi)]
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface IJobDatabaseServiceDef
{
    [Get(nameof(GetActiveJobs))]
    Task<Jobs> GetActiveJobs(CancellationToken token);

    [Get(nameof(GetSortOrders))]
    Task<SordOrders> GetSortOrders(CancellationToken token);

    [Get(nameof(GetJobData))]
    Task<JobData> GetJobData([Query] ProjectId id, CancellationToken token);

    [Get(nameof(CountActiveJobs))]
    Task<ActiveJobs> CountActiveJobs(CancellationToken token);

    [Post(nameof(CreateJob))]
    Task<SimpleResult> CreateJob([Body] CreateProjectCommand command, CancellationToken token);

    [Post(nameof(ChangeOrder))]
    Task<SimpleResult> ChangeOrder([Body] SetSortOrder newOrder, CancellationToken token);

    [Post(nameof(UpdateJobData))]
    Task<SimpleResult> UpdateJobData([Body] UpdateProjectCommand command, CancellationToken token);

    [Post(nameof(AttachFiles))]
    Task<AttachResult> AttachFiles([Body] ProjectAttachFilesCommand command, CancellationToken token);

    [Post(nameof(RemoveFiles))]
    Task<SimpleResult> RemoveFiles([Body] ProjectRemoveFilesCommand command, CancellationToken token);
}