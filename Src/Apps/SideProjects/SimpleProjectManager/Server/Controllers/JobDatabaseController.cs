using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers
{
    [ApiController, JsonifyErrors, Route(ApiPaths.JobsApi + "/[action]")]
    public class JobDatabaseController : Controller, IJobDatabaseService
    {
        private readonly IJobDatabaseService _service;

        public JobDatabaseController(IJobDatabaseService service)
            => _service = service;

        [HttpGet, Publish]
        public Task<JobInfo[]> GetActiveJobs(CancellationToken token)
            => _service.GetActiveJobs(token);

        [HttpGet, Publish]
        public Task<SortOrder[]> GetSortOrders(CancellationToken token)
            => _service.GetSortOrders(token);

        [HttpGet, Publish]
        public Task<JobData> GetJobData([FromQuery] ProjectId id, CancellationToken token)
            => _service.GetJobData(id, token);

        [HttpGet, Publish]
        public Task<long> CountActiveJobs(CancellationToken token)
            => _service.CountActiveJobs(token);

        public Task<string> DeleteJob(ProjectId id, CancellationToken token)
            => _service.DeleteJob(id, token);

        [HttpPost]
        public Task<string> CreateJob([FromBody] CreateProjectCommand command, CancellationToken token)
            => _service.CreateJob(command, token);


        [HttpPost]
        public Task<string> ChangeOrder([FromBody] SetSortOrder newOrder, CancellationToken token)
            => _service.ChangeOrder(newOrder, token);

        [HttpPost]
        public Task<string> UpdateJobData([FromBody] UpdateProjectCommand command, CancellationToken token)
            => _service.UpdateJobData(command, token);

        [HttpPost]
        public Task<AttachResult> AttachFiles([FromBody] ProjectAttachFilesCommand command, CancellationToken token)
            => _service.AttachFiles(command, token);

        [HttpPost]
        public Task<string> RemoveFiles([FromBody] ProjectRemoveFilesCommand command, CancellationToken token)
            => _service.RemoveFiles(command, token);
    }
}
