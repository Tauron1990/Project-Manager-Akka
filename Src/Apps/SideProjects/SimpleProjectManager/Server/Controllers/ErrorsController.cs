using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Server;

namespace SimpleProjectManager.Server.Controllers;

[ApiController, JsonifyErrors, Route(ApiPaths.ErrorApi + "/[action]")]
public class ErrorsController : ICriticalErrorService
{
    private readonly ICriticalErrorService _errorService;

    public ErrorsController(ICriticalErrorService errorService)
        => _errorService = errorService;

    [HttpGet]
    public Task<CriticalError[]> GetErrors(CancellationToken token)
        => _errorService.GetErrors(token);

    [HttpPost]
    public Task<string?> DisableError([FromQuery] string id, CancellationToken token)
        => _errorService.DisableError(id, token);

    [HttpPost]
    public Task WriteError([FromBody]CriticalError error, CancellationToken token)
        => _errorService.WriteError(error, token);
}