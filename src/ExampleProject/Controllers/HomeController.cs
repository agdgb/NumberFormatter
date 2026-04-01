using Microsoft.AspNetCore.Mvc;
using NumberFormatter.AspNetCore;

namespace NumberFormatter.Demo.Controllers;

public class HomeController : Controller
{
    private readonly INumberFormatterService _formatterService;

    public HomeController(INumberFormatterService formatterService)
    {
        _formatterService = formatterService;
    }

    public IActionResult Index()
    {
        ViewBag.FormattedValue = _formatterService.FormatShort(1234567.89m);
        ViewBag.FormattedCurrency = _formatterService.FormatCurrency(9876543.21m, currencyCode: "EUR");
        return View();
    }
}