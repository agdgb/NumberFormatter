using Microsoft.AspNetCore.Mvc;
using HumanNumbers.AspNetCore;

namespace HumanNumbers.Demo.Controllers;

public class HomeController : Controller
{
    private readonly IHumanNumberService _formatterService;

    public HomeController(IHumanNumberService formatterService)
    {
        _formatterService = formatterService;
    }

    public IActionResult Index()
    {
        ViewBag.FormattedValue = _formatterService.Format(1234567.89m);
        ViewBag.FormattedCurrency = _formatterService.FormatCurrency(9876543.21m, currencyCode: "EUR");
        return View();
    }
}