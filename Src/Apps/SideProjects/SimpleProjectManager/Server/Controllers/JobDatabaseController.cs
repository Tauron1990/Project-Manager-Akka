using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;
using Tauron.Operations;

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

        [HttpPost]
        public Task<OperationResult> CreateJob(CreateProjectCommand command, CancellationToken token)
            => _service.CreateJob(command, token);
    }
}
