using Microsoft.AspNetCore.Mvc;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Shared.ViewModels;

namespace SimpleProjectManager.Server.Controllers;

[Controller]
[Route("/")]
public class IndexController : Controller
{
    // GET
    [Route("App")]
    public IActionResult Index()
        => Redirect("/");
    
    public IActionResult App()
        => View("_Host");

    [Route("Unkowen")]
    public IActionResult NothingToSee()
    {
        string path = HttpContext.Request.Path.Value ?? string.Empty;

        return PageNavigation.All.Any(s => path.StartsWith(s, StringComparison.Ordinal)) ? View("_Host") : View();

    }
}