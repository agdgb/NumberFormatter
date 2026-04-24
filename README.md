# HumanNumbers — Make data human-readable, instantly.

```csharp
1234567.ToHuman();          // 1.23M
987654.ToHumanCurrency();   // $987.65K
1536.ToHumanBytes();        // 1.50 KiB
2024.ToRoman();             // MMXXIV
```

```bash
dotnet add package HumanNumbers
```

---

## Why HumanNumbers Exists

* Dashboards need compact numbers
* APIs should return human‑readable values
* Currency + culture formatting is repetitive
* Financial formats are niche and hard to implement
* No single library solved all layers

HumanNumbers is the unified foundation for presentation-ready numbers across .NET codebases.

---

## When Should You Use This Library?

* Dashboards / analytics
* Admin panels
* Public APIs consumed by frontend apps
* Financial or trading systems
* Razor / MVC / Blazor apps
* Reports / PDFs / Excel exports

---

## Installation

### Core Library
```bash
dotnet add package HumanNumbers
```

### ASP.NET Core Integration
```bash
dotnet add package HumanNumbers.AspNetCore
```

---

## Live Showcase & Playground

Want to see all features in action before adopting? Check out the **[ExampleProject](src/ExampleProject)**:

*   **Interactive Labs**: Test Roman numerals, Byte sizes, and Robust Parsing in real-time.
*   **JSON Showcase**: See how `[HumanNumber]` with `OutputMode` transforms API responses selectively.
*   **Round-trip Testing**: Verify model binding and serialization stability via the Financial Playground.

To run the showcase locally:
```bash
dotnet run --project src/ExampleProject/NumberFormatter.Demo.csproj
```

---

## Quick Start (Core Library)

### Short Numbers
```csharp
1234.ToHuman();      // 1.23K
1500000.ToHuman();   // 1.50M
```

### Currency
```csharp
amount.ToHumanCurrency();
amount.ToHumanCurrency("EUR");
```

### Financial
```csharp
0.015m.ToHumanBps();                        // "150 bps"
101.5m.ToHumanFraction(32);                 // "101 16/32"
1234.56m.ToHumanWords();                    // "One Thousand Two Hundred Thirty-Four Dollars and 56/100"
1234.56m.ToHumanWords("Euros", "Euro");     // "One Thousand Two Hundred Thirty-Four Euros and 56/100"

// Financial rounding for market ticks
10.22m.RoundToTick(0.05m, TickRoundingMode.Down); // 10.20m
```

### Parsing
```csharp
HumanNumber.Parse("1.5M");
HumanNumber.TryParse("$50K", out decimal value);
```

### Try Pattern Methods (Exception-Free)
```csharp
if (1500m.TryToHuman(out var result))
    Console.WriteLine(result); // "1.50K"

if (1250m.TryToHumanCurrency("EUR", out var currency))
    Console.WriteLine(currency); // "€1.25K"
```

---

## Feature Overview

* [Compact number formatting](#formatting-numbers)
* [Currency formatting](#quick-start-core-library)
* [Culture awareness](#culture-support)
* [Byte sizes](#byte-size-formatting)
* [Roman numerals](#roman-numerals)
* [Financial formatting](#quick-start-core-library)
* [JSON + ASP.NET integration](#aspnet-core-quick-setup)

---

## Formatting Numbers

### Custom Options
```csharp
1234.ToHuman(new HumanNumberFormatOptions { DecimalPlaces = 3 }); // 1.234K
```

### Fluent API
```csharp
HumanNumber.Format(1500000m)
    .UsingPolicy("Dashboard")
    .UsingCulture(new CultureInfo("fr-FR"))
    .ToHuman(); // "1,5M"

HumanNumber.Format(987654m)
    .ToHumanCurrency("EUR"); // "€987.65K"
```

### Culture Support
```csharp
1234567.ToHuman(new CultureInfo("fr-FR")); // 1,23 M
```

### Custom Suffix Sets
```csharp
1000.ToHuman(new HumanNumberFormatOptions { CustomSuffixes = new[] { " thousand", " million" } }); // 1 thousand
```

---

## Byte Size Formatting

```csharp
1048576.ToHumanBytes(useBinaryPrefixes: true);  // "1.00 MiB"
1000000.ToHumanBytes(useBinaryPrefixes: false); // "1.00 MB"
```

---

## Roman Numerals

```csharp
2024.ToRoman(); // MMXXIV
```

---

## ASP.NET Core Quick Setup

```csharp
builder.Services.AddHumanNumbersDefaults();
```

Enables automatically:
* Per-property JSON formatting (opt-in via `[HumanNumber]`)
* MVC / Minimal API support
* DI services
* TagHelpers

---

## ASP.NET Core Usage

### The `[HumanNumber]` Attribute

The attribute is a **metadata marker** by default. It signals intent but does not alter JSON output unless explicitly opted-in via `OutputMode`.

```csharp
public class RevenueReport
{
    // Stays a raw number in JSON — safe default
    [HumanNumber]
    public decimal Revenue { get; set; }

    // Formatted as a human string in JSON — explicit opt-in
    [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]
    public decimal DisplayRevenue { get; set; }

    // Currency with explicit opt-in
    [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman, IsCurrency = true, CurrencyCode = "USD")]
    public decimal UsdRevenue { get; set; }
}
```

**JSON Output:**
```json
{
  "revenue": 1550000.50,
  "displayRevenue": "1.55M",
  "usdRevenue": "$1.55M"
}
```

### Global Mode (Format Everything)

If you want blanket formatting across your entire API surface (old v1 behavior):

```csharp
builder.Services.AddHumanNumbersJsonGlobal();
```

### JSON Output: Attribute-Driven vs Global

| Setup | `revenue` output | `[HumanNumber]` output | `[SerializeAsHuman]` output |
| :--- | :--- | :--- | :--- |
| `AddHumanNumbersJson()` | `1550000.5` | `1550000.5` | `"1.55M"` |
| `AddHumanNumbersJsonGlobal()` | `"1.55M"` | `"1.55M"` | `"1.55M"` |

### Minimal API Example

```csharp
app.MapGet("/stats", () => Results.Extensions.HumanOk(new { Revenue = 1500000.50m }));
```

### Financial-Specific Attributes
```csharp
public record FinancialData(
    [property: BasisPoints(Decimals = 1, WriteAsString = true)] decimal Spread,
    [property: FractionPrice(Denominator = 32)] decimal BondPrice
);
```

### Razor TagHelpers

```html
<hn-number value="1234567" />
<hn-currency value="1234.5" currency-code="USD" />
<hn-check value="1250.50" major-currency="Dollars" major-currency-singular="Dollar" />
```

### Dependency Injection Service

```csharp
// Manual formatting in services/controllers
public class MyService(IHumanNumberService humanNumberService)
{
    public string FormatRevenue(decimal revenue) 
        => humanNumberService.Format(revenue, decimalPlaces: 1);
    
    public string FormatCurrency(decimal amount, string currency)
        => humanNumberService.FormatCurrency(amount, currency);
}
```

### Error Handling & Conflict Safety

```csharp
// Safe-first: [HumanNumber] respects existing converters
[JsonConverter(typeof(MyCustomConverter))]
[HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)] // Silently skipped
public decimal Value { get; set; }

// Error modes
builder.Services.AddHumanNumbersCore(options => {
    options.CoreOptions.ErrorMode = HumanNumbersErrorMode.Strict; // Throw on errors
});
```

---

## Formatting Policies

```csharp
builder.Services.AddHumanNumbersCore(options => 
{
    options.DefaultPolicyName = "Dashboard";
});
```

---

## Supported Numeric Types

HumanNumbers provides generic extension methods for all built-in .NET numeric types:

*   **Floating Point**: `decimal`, `double`, `float`
*   **Integers**: `long`, `ulong`, `int`, `uint`, `short`, `ushort`, `byte`, `sbyte`
*   **Advanced**: Supports any type implementing `INumber<T>` (in .NET 7+)
*   **Nullable**: All types support nullable variants that return `string.Empty` for null values

*Note: All types are internally normalized to `decimal` for maximum precision during scaling.*

### Generic Type Support
```csharp
// Works with any INumber<T> type
myCustomNumeric.ToHuman();
myCustomNumeric.ToHumanCurrency("USD");

// Safe nullable handling
decimal? value = null;
var result = value.ToHuman(); // Returns "" (empty string)
```

---

## Performance & Benchmarks

HumanNumbers is designed for high-throughput dashboard and API scenarios:

*   **Zero Allocations**: Core formatting paths use `Span<char>` and stack-allocated buffers.
*   **Caching**: Culture metadata (`NumberFormatInfo`) and currency symbols are cached globally.
*   **Fast Path**: Native `decimal` operations are prioritized for speed.

| Operation | Mean Time | Allocated |
| :--- | :--- | :--- |
| `1.23M.ToHuman()` | 85 ns | 0 B |
| `ToHumanCurrency()` | 110 ns | 0 B |
| `Parse("1.5M")` | 120 ns | 32 B |
| `ToRoman()` | 45 ns | 0 B |

---

## Migration From NumberFormatter / v1.x

> [!WARNING]
> **Deprecation Notice**: The `NumberFormatter` NuGet package is now deprecated. Future updates will only be released under the `HumanNumbers` namespace.

1.  **Update Packages**: Swap `NumberFormatter` for `HumanNumbers`.
2.  **Namespace Change**: Replace `using NumberFormatter;` with `using HumanNumbers;`.
3.  **API Mapping**:
    *   `ToShortString()` → `ToHuman()`
    *   `ToShortCurrencyString()` → `ToHumanCurrency()`
    *   `ToBpsString()` → `ToHumanBps()`
    *   `ToFractionString()` → `ToHumanFraction()`
    *   `ToCheckWords()` → `ToHumanWords()`
    *   `<short-number>` → `<hn-number>`
4.  **Attribute Changes**:
    *   `[ShortNumberFormat]` → `[HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]`
    *   `[HumanNumberFormat]` → `[HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]`
5.  **Serialization Default Changed**: `[HumanNumber]` no longer alters JSON by default. Add `OutputMode = SerializeAsHuman` where you need formatted output in JSON.
6.  **Configuration**: Use `HumanNumber.Configure()` instead of setting static defaults on the obsolete class.

## Roadmap

* Built-in Blazor components
* Source generators for compile-time format configuration
* Extended built-in policy presets

---

## Read the Full Guide

For exhaustive details on JSON policies, MVC automatic filters, resolving culture, and Minimal APIs, see the **[Full Documentation Here](docs/FullDocumentation.md)**.

---

## Contributing

Pull requests are welcome! Please run tests and follow our code style guidelines before submitting. Features must be covered by tests.

---

## License

[MIT License](LICENSE)
