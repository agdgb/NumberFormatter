using NumberFormatter;
using System.Globalization;

// Basic usage
Console.WriteLine(1234m.ToShortString());                 // "1.23K"
Console.WriteLine(1234567m.ToShortString(1));             // "1.2M"
Console.WriteLine(1234567890m.ToShortString());           // "1.23B"

// Currency formatting
Console.WriteLine(1234567m.ToShortCurrencyString());      // "$1.23M" (based on current culture)
Console.WriteLine(1234567m.ToShortCurrencyString("EUR")); // "€1.23M"
Console.WriteLine(1234567m.ToShortCurrencyString("ETB")); // "Br1.23M"

// Culture-aware formatting
var germanCulture = new CultureInfo("de-DE");
Console.WriteLine(1234.56m.ToShortString(2, germanCulture)); // "1,23K"

// Custom options
var options = new ShortNumberFormatOptions
{
    DecimalPlaces = 1,
    ShowPlusSign = true,
    CurrencySymbol = "$",
    CurrencyPosition = CurrencyPosition.BeforeWithSpace,
    NegativePattern = "(n)"
};

Console.WriteLine(1234.56m.ToShortString(options));     // "+ $1.2K"
Console.WriteLine((-1234.56m).ToShortString(options));  // "($1.2K)"

// ASP.NET Core Model Example
public class FinancialReportModel
{
    [ShortNumberFormat(2, true)] // Currency format
    public decimal Revenue { get; set; }

    [ShortNumberFormat(1)] // Plain format with 1 decimal
    public decimal Growth { get; set; }

    public decimal RawValue { get; set; }
}

// In ASP.NET Core Controller
public class ReportController : ControllerBase
{
    [HttpGet]
    public IActionResult GetReport()
    {
        var report = new
        {
            Revenue = 1234567.89m.ToShortCurrencyString(),
            Growth = 0.1234m.ToShortString(1) + "%",
            QuarterlyEarnings = 123456m.ToShortCurrencyString("EUR")
        };

        return Ok(report);
    }
}

// Razor View Usage
@addTagHelper *, NumberFormatter

<short-number value = "1234567" format = "currency" decimal-places="2" />
<short-number value="1234" decimal-places="1" css-class= "text-bold" />
<short-number value = "1234567890" currency - code = "EUR" />