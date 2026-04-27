using HumanNumbers;
using System.Globalization;

// ── Basic compact formatting ────────────────────────────────────────────────
Console.WriteLine(1234m.ToHuman());               // "1.23K"
Console.WriteLine(1234567m.ToHuman(1));           // "1.2M"
Console.WriteLine(1234567890m.ToHuman());         // "1.23B"

// ── Currency formatting ────────────────────────────────────────────────────
Console.WriteLine(1234567m.ToHumanCurrency());        // "$1.23M" (based on CurrentCulture)
Console.WriteLine(1234567m.ToHumanCurrency("EUR"));   // "€1.23M"
Console.WriteLine(1234567m.ToHumanCurrency("ETB"));   // "Br1.23M"

// ── Culture-aware formatting ───────────────────────────────────────────────
var germanCulture = new CultureInfo("de-DE");
Console.WriteLine(1234.56m.ToHuman(2, germanCulture)); // "1,23K"

// ── Fluent API ─────────────────────────────────────────────────────────────
Console.WriteLine(
    HumanNumber.Format(1500000m)
        .UsingCulture(new CultureInfo("fr-FR"))
        .ToHumanCurrency("EUR")
); // "€1,50M"

// ── Financial formatting ───────────────────────────────────────────────────
Console.WriteLine(0.0125m.ToHumanBps());              // "125 bps"
Console.WriteLine(101.5m.ToHumanFraction(32));        // "101 16/32"
Console.WriteLine(1234.56m.ToHumanWords());           // "One Thousand Two Hundred Thirty-Four Dollars and 56/100"
Console.WriteLine(1234.56m.ToHumanWords("Euros", "Euro")); // "One Thousand Two Hundred Thirty-Four Euros and 56/100"

// ── Byte sizes ─────────────────────────────────────────────────────────────
long bytes = 1073741824;
Console.WriteLine(bytes.ToHumanBytes());                        // "1.07 GB"
Console.WriteLine(bytes.ToHumanBytes(useBinaryPrefixes: true)); // "1.00 GiB"

// ── Roman numerals ─────────────────────────────────────────────────────────
Console.WriteLine(2024.ToRoman()); // "MMXXIV"

// ── Parsing ────────────────────────────────────────────────────────────────
Console.WriteLine(HumanNumber.Parse("1.5M"));                            // 1500000
Console.WriteLine(HumanNumber.TryParse("$50K", out decimal val) ? val : 0); // 50000

// ── ASP.NET Core Model Example (HumanNumbers.AspNetCore) ───────────────────
//
// using HumanNumbers.AspNetCore;
//
// public class FinancialReportModel
// {
//     // Metadata only — JSON preserves raw number (Safe-First default)
//     [HumanNumber]
//     public decimal Revenue { get; set; }
//
//     // Opt-in: JSON becomes formatted string ("$1.23M")
//     [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, IsCurrency = true)]
//     public decimal DisplayRevenue { get; set; }
//
//     // 1 decimal place, formatted in JSON
//     [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, DecimalPlaces = 1)]
//     public decimal Growth { get; set; }
//
//     public decimal RawValue { get; set; } // No attribute — untouched
// }
//
// Razor TagHelper Usage:
// @addTagHelper *, HumanNumbers.AspNetCore
//
// <hn-number value="1234567" decimal-places="2" />
// <hn-currency value="1234567" currency-code="USD" />
// <hn-check value="1234.56" />