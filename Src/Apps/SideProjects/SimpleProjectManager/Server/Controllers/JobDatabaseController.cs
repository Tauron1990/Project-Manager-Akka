using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[JsonifyErrors]
[Route(ApiPaths.JobsApi + "/[action]")]
public class JobDatabaseController : Controller, IJobDatabaseService
{
    private readonly IJobDatabaseService _service;

    public JobDatabaseController(IJobDatabaseService service)
        => _service = service;

    [HttpGet]
    public Task<Jobs> GetActiveJobs(CancellationToken token)
        => _service.GetActiveJobs(token);

    [HttpGet]
    public Task<SortOrders> GetSortOrders(CancellationToken token)
        => _service.GetSortOrders(token);

    [HttpGet]
    public Task<JobData> GetJobData([FromQuery] ProjectId id, CancellationToken token)
        => _service.GetJobData(id, token);

    [HttpGet]
    public Task<ActiveJobs> CountActiveJobs(CancellationToken token)
        => _service.CountActiveJobs(token);

    public Task<SimpleResultContainer> DeleteJob(ProjectId id, CancellationToken token)
        => _service.DeleteJob(id, token);

    [HttpPost]
    public Task<SimpleResultContainer> CreateJob([FromBody] CreateProjectCommand command, CancellationToken token)
        => _service.CreateJob(command, token);


    [HttpPost]
    public Task<SimpleResultContainer> ChangeOrder([FromBody] SetSortOrder newOrder, CancellationToken token)
        => _service.ChangeOrder(newOrder, token);

    [HttpPost]
    public Task<SimpleResultContainer> UpdateJobData([FromBody] UpdateProjectCommand command, CancellationToken token)
        => _service.UpdateJobData(command, token);

    [HttpPost]
    public Task<AttachResult> AttachFiles([FromBody] ProjectAttachFilesCommand command, CancellationToken token)
        => _service.AttachFiles(command, token);

    [HttpPost]
    public Task<SimpleResultContainer> RemoveFiles([FromBody] ProjectRemoveFilesCommand command, CancellationToken token)
        => _service.RemoveFiles(command, token);
}