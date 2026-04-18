using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Globalization;
using System.Linq;
using HumanNumbers;
using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Financial;
using HumanNumbers.Demo.Models;
using HumanNumbers.Financial;
using HumanNumbers.Roman;
using HumanNumbers.Bytes;

namespace HumanNumbers.Demo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly IHumanNumberService _formatter;

    public DemoController(IHumanNumberService formatter)
    {
        _formatter = formatter;
    }

    public class ParseResult
    {
        public string Input { get; set; } = string.Empty;
        public string Culture { get; set; } = string.Empty;
        
        [NoHumanFormat]
        public decimal ParsedValue { get; set; }
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
        return Ok(new { formatted = _formatter.Format(value) });
    }

    [HttpGet("showcase")]
    public IActionResult GetShowcase()
    {
        return Ok(new ShowcaseModel());
    }

    [HttpGet("roman/{value:int}")]
    public IActionResult ToRoman(int value)
    {
        try
        {
            return Ok(new { value, roman = value.ToRoman() });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("bytes/{value:long}")]
    public IActionResult FormatBytes(long value, [FromQuery] bool binary = false)
    {
        return Ok(new { 
            value, 
            formatted = value.ToHumanBytes(useBinaryPrefixes: binary),
            type = binary ? "Binary (IEC)" : "Decimal (SI)"
        });
    }

    [HttpGet("parse")]
    public IActionResult ParseSnippet([FromQuery] string input, [FromQuery] string? culture = null)
    {
        try
        {
            var cultureInfo = string.IsNullOrEmpty(culture) ? CultureInfo.InvariantCulture : new CultureInfo(culture);
            var result = HumanNumber.Parse(input, cultureInfo);
            return Ok(new ParseResult { Input = input, Culture = cultureInfo.Name, ParsedValue = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Parsing failed: " + ex.Message });
        }
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