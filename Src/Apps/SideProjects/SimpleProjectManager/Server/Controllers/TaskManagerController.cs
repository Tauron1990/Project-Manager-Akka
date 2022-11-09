using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[Route(ApiPaths.TaskApi + "/[action]")]
[JsonifyErrors]
public sealed class TaskManagerController : Controller, ITaskManager
{
    private readonly ITaskManager _taskManager;

    public TaskManagerController(ITaskManager taskManager)
        => _taskManager = taskManager;

    [HttpGet]
    [Publish]
    public Task<Tasks> GetTasks(CancellationToken token)
        => _taskManager.GetTasks(token);

    [HttpPost]
    public Task<SimpleResult> DeleteTask([FromBody] string id, CancellationToken token)
        => _taskManager.DeleteTask(id, token);
}