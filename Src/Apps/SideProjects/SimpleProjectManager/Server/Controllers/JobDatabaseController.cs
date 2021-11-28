using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared;
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
        public ValueTask<JobInfo[]> GetActiveJobs(CancellationToken token)
            => _service.GetActiveJobs(token);

        [HttpGet, Publish]
        public ValueTask<SortOrder[]> GetSortOrders(CancellationToken token)
            => _service.GetSortOrders(token);

        [HttpGet, Publish]
        public ValueTask<JobData> GetJobData([FromQuery]ProjectId id, CancellationToken token)
            => _service.GetJobData(id, token);

        public ValueTask<string> DeleteJob(ProjectId id, CancellationToken token)
            => _service.DeleteJob(id, token);

        [HttpPost]
        public ValueTask<string> CreateJob([FromBody]CreateProjectCommand command, CancellationToken token)
            => _service.CreateJob(command, token);


        [HttpPost]
        public ValueTask<string> ChangeOrder([FromBody]SetSortOrder newOrder, CancellationToken token)
            => _service.ChangeOrder(newOrder, token);

        [HttpPost]
        public ValueTask<string> UpdateJobData([FromBody]UpdateProjectCommand command, CancellationToken token)
            => _service.UpdateJobData(command, token);

        [HttpPost]
        public ValueTask<AttachResult> AttachFiles([FromBody]ProjectAttachFilesCommand command, CancellationToken token)
            => _service.AttachFiles(command, token);

        [HttpPost]
        public ValueTask<string> RemoveFiles([FromBody]ProjectRemoveFilesCommand command, CancellationToken token)
            => _service.RemoveFiles(command, token);
    }
}
