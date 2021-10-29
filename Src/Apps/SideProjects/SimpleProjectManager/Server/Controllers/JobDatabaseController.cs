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

        [HttpGet]
        public Task<JobInfo[]> GetActiveJobs(CancellationToken token)
            => _service.GetActiveJobs(token);

        [HttpGet]
        public Task<SortOrder> GetSortOrder([FromQuery]ProjectId id, CancellationToken token)
            => _service.GetSortOrder(id, token);

        [HttpPost]
        Task<ApiResult> IJobDatabaseService.CreateJob(CreateProjectCommand command, CancellationToken token)
            => _service.CreateJob(command, token);

        [HttpPost]
        public Task<ApiResult> ChangeOrder(SetSortOrder newOrder, CancellationToken token)
            => _service.ChangeOrder(newOrder, token);
    }
}
