using Microsoft.AspNetCore.Mvc;
using NumberFormatter.AspNetCore;
using NumberFormatter.Demo.Models;

namespace NumberFormatter.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly INumberFormatterService _formatter;

    public DemoController(INumberFormatterService formatter)
    {
        _formatter = formatter;
    }

    [HttpGet("data")]
    public IActionResult GetFinancialData()
    {
        var data = new FinancialData
        {
            Revenue = 12_345_678.90m,
            EuroRevenue = 9_876_543.21m,
            GrowthRate = 0.1567m,
            PageViews = 1_234_567,
            InternationalRevenue = new Dictionary<string, decimal>
            {
                ["USA"] = 5_000_000m,
                ["EUR"] = 4_000_000m,
                ["GBP"] = 3_500_000m
            }
        };

        // Manually format dictionary values because the global converter doesn't know the keys
        var formattedInternational = data.InternationalRevenue
            .ToDictionary(kv => kv.Key, kv => _formatter.FormatCurrency(kv.Value, currencyCode: kv.Key));

        return Ok(new
        {
            data.Revenue,
            data.EuroRevenue,
            data.GrowthRate,
            data.PageViews,
            InternationalRevenue = formattedInternational
        });
    }

    [HttpGet("manual/{value:decimal}")]
    public IActionResult FormatManual(decimal value, [FromQuery] string? currency = null)
    {
        if (!string.IsNullOrEmpty(currency))
        {
            return Ok(new { formatted = _formatter.FormatCurrency(value, currencyCode: currency) });
        }
        return Ok(new { formatted = _formatter.FormatShort(value) });
    }
}