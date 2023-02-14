using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.ServerApi;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[Route(ApiPaths.LogsApi + "/[action]")]
public sealed class LogsController : Controller
{
    
}