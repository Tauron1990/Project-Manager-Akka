using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Shared.ServerApi;

namespace SimpleProjectManager.Server.Controllers;

[ApiController]
[Route(ApiPaths.PingApi)]
public class PingController : Controller
{
    [HttpGet]
    public PingResult GetPing()
        => PingResult.IsOk;
}