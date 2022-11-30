using Microsoft.AspNetCore.Mvc;

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
        => View();
}