using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController, JsonifyErrors, Route(ApiPaths.ErrorApi + "/[action]")]
public class ErrorsController : Controller, ICriticalErrorService
{
    private readonly ICriticalErrorService _errorService;

    public ErrorsController(ICriticalErrorService errorService)
        => _errorService = errorService;

    [HttpGet, Publish]
    public Task<long> CountErrors(CancellationToken token)
        => _errorService.CountErrors(token);

    [HttpGet, Publish]
    public Task<CriticalError[]> GetErrors(CancellationToken token)
        => _errorService.GetErrors(token);

    [HttpPost]
    public Task<string> DisableError([FromQuery] string id, CancellationToken token)
        => _errorService.DisableError(id, token);

    [HttpPost]
    public Task WriteError([FromBody] CriticalError error, CancellationToken token)
        => _errorService.WriteError(error, token);
}