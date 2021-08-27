using Microsoft.AspNetCore.Mvc;

namespace ServiceManager.Server.Controllers
{
    public class UserManagmentController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}