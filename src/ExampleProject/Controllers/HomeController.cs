using Microsoft.AspNetCore.Mvc;

namespace NumberFormatter.Demo.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}