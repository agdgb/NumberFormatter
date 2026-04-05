using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using NumberFormatter.AspNetCore;
using NumberFormatter.AspNetCore.Financial;
using NumberFormatter.Demo.Models;
using NumberFormatter.Financial;

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

    [HttpPost("financial")]
    public IActionResult ProcessFinancialPlayground([FromBody] JsonElement rawInput)
    {
        try
        {
            var jsonString = rawInput.GetRawText();
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            
            var parsed = JsonSerializer.Deserialize<FinancialPlaygroundRequest>(jsonString, options);
            
            var roundtripString = JsonSerializer.Serialize(parsed, options);
            var roundtripJsonElement = JsonSerializer.Deserialize<JsonElement>(roundtripString);

            var node1 = System.Text.Json.Nodes.JsonNode.Parse(jsonString);
            var node2 = System.Text.Json.Nodes.JsonNode.Parse(roundtripString);
            bool isMatch = System.Text.Json.Nodes.JsonNode.DeepEquals(node1, node2);

            return Ok(new
            {
                input = rawInput,
                parsed = new { 
                   spread = parsed?.Spread,
                   treasuryPrice = parsed?.TreasuryPrice,
                   rawAmount = parsed?.RawAmount
                },
                roundtrip = roundtripJsonElement,
                roundtripMatch = isMatch,
                wordsOutput = parsed?.RawAmount != null ? parsed.RawAmount.Value.ToWords() : null
            });
        }
        catch (JsonException ex)
        {
            // Explicitly returning 400 with the JSON parse exception string exactly as requested
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "An unexpected error occurred: " + ex.Message });
        }
    }
}