using Akka.Actor;
using Akka.Hosting;
using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Client.Operations.Shared.Clustering;
using SimpleProjectManager.Server.Core.Clustering;
using SimpleProjectManager.Shared.ServerApi;
using SimpleProjectManager.Shared.Services.LogFiles;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[Route(ApiPaths.LogsApi + "/[action]")]
public sealed class LogsController : Controller
{
    private readonly IActorRef _actor;

    public LogsController(IRequiredActor<ClusterLogManager> logManager) 
        => _actor = logManager.ActorRef;

    [HttpGet]
    public async ValueTask<LogFilesData> GetFileNames()
        => await _actor.Ask<LogFilesData>(new QueryLogFileNames(), TimeSpan.FromSeconds(20), cancellationToken: HttpContext.RequestAborted);

    [HttpPost]
    public async ValueTask<LogFileContent> GetLogFileContent([FromBody] LogFileRequest request)
        => await _actor.Ask<LogFileContent>(request, TimeSpan.FromSeconds(20), cancellationToken: HttpContext.RequestAborted);
}