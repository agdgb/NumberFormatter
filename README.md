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
*   **JSON Showcase**: See how attributes like `[HumanNumberFormat]` and `[NoHumanFormat]` transform API responses.
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

### Parsing
```csharp
HumanNumber.Parse("1.5M");
HumanNumber.TryParse("$50K", out decimal value);
```

---

## Feature Overview

* [Compact number formatting](#formatting-numbers)
* [Currency formatting](#quick-start-core-library)
* [Culture awareness](#culture-support)
* [Byte sizes](#byte-size-formatting)
* [Roman numerals](#roman-numerals)
* [Financial formatting](#financial-formatting)
* [JSON + ASP.NET integration](#aspnet-core-quick-setup)

---

## Formatting Numbers

### Custom Options
```csharp
1234.ToHuman(new HumanNumberFormatOptions { DecimalPlaces = 3 }); // 1.234K
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

## Financial Formatting

### Basis Points
```csharp
0.015m.ToBpsString(); // "150 bps"
BasisPointFormatter.TryParseBps("150 bps", out decimal value); // 0.015m
```

### Fractional Prices
```csharp
101.5m.ToFractionString(32); // "101 16/32"
FinancialRounding.TryParseFraction("101 16/32", out decimal value); // 101.5m
```

### Check Writing Numbers
```csharp
1234.56m.ToCheckWords(); // "One Thousand Two Hundred Thirty-Four Dollars and 56/100"
1234.56m.ToCheckWords("Euros", "Euro"); // "One Thousand Two Hundred Thirty-Four Euros and 56/100"
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
* JSON converters
* MVC / Minimal API support
* DI services
* TagHelpers

---

## ASP.NET Core Usage

### JSON Output Example

Before:
```json
{
  "revenue": 1500000
}
```

After:
```json
{
  "revenue": "1.50M"
}
```

### Minimal API Example

```csharp
app.MapGet("/stats", () => Results.Extensions.HumanOk(new { Revenue = 1500000.50m }));
```

### Razor TagHelpers

```html
<hn-number value="1234567" />
<hn-currency value="1234.5" currency-code="USD" />
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

---

## Supported Numeric Types

HumanNumbers provides generic extension methods for all built-in .NET numeric types:

*   **Floating Point**: `decimal`, `double`, `float`
*   **Integers**: `long`, `ulong`, `int`, `uint`, `short`, `ushort`, `byte`, `sbyte`
*   **Advanced**: Supports any type implementing `INumber<T>` (in .NET 7+)

*Note: All types are internally normalized to `decimal` for maximum precision during scaling.*

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

## Migration From NumberFormatter

> [!WARNING]
> **Deprecation Notice**: The `NumberFormatter` NuGet package is now deprecated. Future updates will only be released under the `HumanNumbers` namespace.

1.  **Update Packages**: Swap `NumberFormatter` for `HumanNumbers`.
2.  **Namespace Change**: Replace `using NumberFormatter;` with `using HumanNumbers;`.
3.  **API Mapping**:
    *   `ToShortString()` → `ToHuman()`
    *   `ToShortCurrencyString()` → `ToHumanCurrency()`
    *   `<short-number>` → `<hn-number>`
4.  **Configuration**: Use `HumanNumber.Configure()` instead of setting static defaults on the obsolete class.


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
