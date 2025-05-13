using Microsoft.AspNetCore.Mvc;

namespace KomunicatorServer.Controllers;

public class AuthController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}