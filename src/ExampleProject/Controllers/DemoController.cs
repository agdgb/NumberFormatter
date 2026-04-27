using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Globalization;
using System.Linq;
using HumanNumbers;
using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Financial;
using HumanNumbers.Suffixes;
using HumanNumbers.Formatting;
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

    [HttpGet("policies")]
    public IActionResult GetPolicies()
    {
        // Add "Default" to the list since it's the baseline
        var policies = new List<string> { "Default" };
        policies.AddRange(HumanNumbersConfig.Instance.GetPolicyNames());
        return Ok(policies);
    }

    [HttpGet("format-policy")]
    public IActionResult FormatWithPolicy(
        [FromQuery] decimal value,
        [FromQuery] string? policy = null,
        [FromQuery] string? mode = "number",
        [FromQuery] string? currency = "USD",
        [FromQuery] bool? alwaysShowSuffix = null,
        [FromQuery] decimal? threshold = null)
    {
        var activePolicyName = string.IsNullOrEmpty(policy) ? "Default" : policy;

        // Start with policy options
        HumanNumbersConfig.Instance.TryGetPolicy(activePolicyName, out var options);
        if (activePolicyName == "Default" || options == null) options = HumanNumbersConfig.Instance.GlobalOptions;

        // Override with explicit parameters if provided
        if (alwaysShowSuffix.HasValue) options.AlwaysShowSuffix = alwaysShowSuffix.Value;
        if (threshold.HasValue) options.Threshold = threshold.Value;

        var context = HumanNumber.Format(value).UsingOptions(options);

        // Find the options for technical detail display again if it was Default
        var policyOptions = options;

        string result = mode?.ToLower() switch
        {
            "currency" => context.ToHumanCurrency(currency),
            "words" => value.ToHumanWords(),
            _ => context.ToHuman()
        };

        return Ok(new
        {
            formatted = result,
            policy = activePolicyName,
            details = new
            {
                threshold = policyOptions.Threshold,
                decimalPlaces = policyOptions.DecimalPlaces,
                suffixes = policyOptions.CachedCustomSuffixes?.Select(s => new { s.Threshold, s.Suffix })
                           ?? StandardSuffixSets.Default.Select(s => new { s.Threshold, s.Suffix })
            }
        });
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
        return Ok(new
        {
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
                parsed = new
                {
                    spread = parsed?.Spread,
                    treasuryPrice = parsed?.TreasuryPrice,
                    rawAmount = parsed?.RawAmount
                },
                roundtrip = roundtripJsonElement,
                roundtripMatch = isMatch,
                wordsOutput = parsed?.RawAmount != null ? parsed.RawAmount.Value.ToHumanWords() : null
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

    [HttpGet("bps")]
    public IActionResult ToBps([FromQuery] decimal value, [FromQuery] int decimals = 0)
    {
        return Ok(new
        {
            value,
            formatted = value.ToHumanBps(decimals),
            bps = value.ToBps()
        });
    }

    [HttpGet("fraction")]
    public IActionResult ToFraction([FromQuery] decimal value, [FromQuery] int denominator = 32)
    {
        return Ok(new
        {
            value,
            formatted = value.ToHumanFraction(denominator),
            denominator
        });
    }

    [HttpGet("words")]
    public IActionResult ToWords([FromQuery] decimal value, [FromQuery] string? major = null, [FromQuery] string? singular = null)
    {
        return Ok(new
        {
            value,
            formatted = value.ToHumanWords(major, singular)
        });
    }
}