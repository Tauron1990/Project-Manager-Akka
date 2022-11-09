using Microsoft.AspNetCore.Mvc;

namespace SimpleProjectManager.Server.Controllers;

[Controller]
[Route("/")]
public class IndexController : Controller
{
    // GET
    public IActionResult Index()
        => View("_Host");

    public IActionResult NothingToSee()
        => View();
}