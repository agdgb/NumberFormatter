# NumberFormatter

A powerful .NET library for formatting numbers into short, human-readable strings with support for suffixes (K, M, B, T, Qa, Qi), currency symbols, and culture-aware formatting. Includes ASP.NET Core integration for JSON serialization, Razor Tag Helpers, and view components.

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## Features

* **Short number formatting** – Convert numbers like `1,234,567` into `1.23M`
* **Parsing support** – Parse formatted strings safely back to numbers without Regex
* **Currency support** – Display currency symbols with suffix logic (`$1.23M`, `€987.65K`)
* **Culture-aware** – Respects decimal separators and formatting rules of the specified culture
* **Digital byte sizes** – Format numbers as digital storage (e.g., `MB`, `MiB`, `GB`)
* **Roman numerals** – Convert integers to Roman numeral strings
* **Customizable suffixes** – Use your own suffixes (e.g., "Thousand", "Million", "Lac", "Crore")
* **Flexible options** – Control decimal places, plus sign, negative patterns, and more
* **High performance** – Caches formatting options, uses zero-allocation span parsers and efficient logic
* **ASP.NET Core integration** – JSON converters, DI service, Tag Helper, View Component, and MVC/Razor Pages extensions
* **Round-trip JSON** – Serialize and deserialize formatted numbers directly securely

---

## Installation

### Core Library

```bash
dotnet add package agdgb.NumberFormatter
```

### ASP.NET Core Integration

```bash
dotnet add package agdgb.NumberFormatter.AspNetCore
```

---

## Usage

### Basic Number Formatting

```csharp
using NumberFormatter;

decimal value = 1234567.89m;
Console.WriteLine(value.ToShortString());   // "1.23M"
Console.WriteLine(value.ToShortString(3));  // "1.235M"

int count = 1500;
Console.WriteLine(count.ToShortString());   // "1.50K"
```

### Currency Formatting

```csharp
decimal amount = 987654.32m;

// Using current culture's currency symbol
Console.WriteLine(amount.ToShortCurrencyString()); // "$987.65K"

// Specifying a currency code
Console.WriteLine(amount.ToShortCurrencyString("EUR")); // "€987.65K"

// Using a custom currency symbol
var options = new ShortNumberFormatOptions
{
    CurrencySymbol = "¥",
    CurrencyPosition = CurrencyPosition.After
};

Console.WriteLine(amount.ToShortString(options)); // "987.65K¥"
```

### Custom Options

```csharp
var options = new ShortNumberFormatOptions
{
    DecimalPlaces = 1,
    ShowPlusSign = true,
    NegativePattern = "(n)",
    CustomSuffixes = new[] { "", "Thousand", "Million", "Billion" }
};

Console.WriteLine(1234.56m.ToShortString(options));   // "+1.2Thousand"
Console.WriteLine((-1234.56m).ToShortString(options)); // "(1.2Thousand)"
```

### Culture-Aware Formatting

```csharp
using System.Globalization;

var germanCulture = new CultureInfo("de-DE");
Console.WriteLine(1234.56m.ToShortString(2, germanCulture)); // "1,23K"
```

### Working with Different Numeric Types

The library supports any type implementing `INumber<T>`, including `int`, `long`, `double`, and `float`.

```csharp
long bigNumber = 1234567890123L;
Console.WriteLine(bigNumber.ToShortString()); // "1.23T"
```

### Parsing Short Numbers

You can parse formatted short strings back into numeric values using `TryParse` and `Parse`. This safely strips currency symbols and understands suffixes.

```csharp
if (NumberFormatter.TryParse("$1.5M", out decimal result))
{
    Console.WriteLine(result); // 1500000
}

decimal value = NumberFormatter.Parse("50K"); // 50000
```

### Byte Size Formatting

Format digital storage sizes with `ToShortByteString`. Supports both binary (base-1024, e.g. MiB) and decimal (base-1000, e.g. MB) prefixes.

```csharp
long memory = 1536 * 1024;
Console.WriteLine(memory.ToShortByteString(decimalPlaces: 2, useBinaryPrefixes: true));  // "1.50 MiB"
Console.WriteLine(memory.ToShortByteString(decimalPlaces: 2, useBinaryPrefixes: false)); // "1.57 MB"
```

### Roman Numerals

Convert integers (between 1 and 3999) to Roman numerals.

```csharp
int year = 2024;
Console.WriteLine(year.ToRomanNumeral()); // "MMXXIV"
```

---

## Breaking Changes

If you are upgrading from an earlier version, please note the following breaking changes:

* **Object Allocation Removal (`ShortNumberFormatOptions`)**: To significantly improve performance and eliminate heap allocations during formatting, `ShortNumberFormatOptions` has been changed from a `class` to a `record struct`. If your code passed this configuration object between methods and modified it expecting reference type semantics, you must update your code to either pass by `ref` or assign the modified struct back to your variable.
* **Deprecation of Public Parsing Expositions**: The public static dictionaries `SuffixMultipliers` and `CurrencySymbols` previously exposed on `ShortNumberJsonConverter<T>` and `CurrencyDictionaryConverter` have been removed. This logic is now efficiently managed and centralized within the core library's `NumberFormatter.TryParse` method, avoiding duplicate Regex and allocation overhead.

---

## ASP.NET Core Integration

### 1. Register the JSON Converter (Global)

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ShortNumberJsonConverterFactory());
    });
```

All numeric properties in API responses will be formatted with default settings. The converter also supports **deserialization**, allowing values like `"1.23M"` to be parsed back into numeric form. Nullable types are handled automatically.

---

### 2. Per-Property Control with Attribute

```csharp
public class FinancialData
{
    [ShortNumberFormat(isCurrency: true)]
    public decimal Revenue { get; set; }

    [ShortNumberFormat(isCurrency: true, currencyCode: "EUR")]
    public decimal EuroRevenue { get; set; }

    [ShortNumberFormat(decimalPlaces: 1)]
    public decimal GrowthRate { get; set; }

    public long PageViews { get; set; }
}
```

Apply formatting globally per class:

```csharp
[ShortNumberFormatGlobally(isCurrency: true, decimalPlaces: 2)]
public class FinancialReport
{
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public string Title { get; set; }
}
```

---

### 3. Register the DI Service

```csharp
builder.Services.AddNumberFormatter();
```

```csharp
public class MyController : ControllerBase
{
    private readonly INumberFormatterService _formatter;

    public MyController(INumberFormatterService formatter)
    {
        _formatter = formatter;
    }

    public IActionResult Get()
    {
        var formatted = _formatter.FormatCurrency(1234567.89m, currencyCode: "USD");
        return Ok(new { formatted });
    }
}
```

---

### 4. Razor Tag Helper

```csharp
@addTagHelper *, NumberFormatter.AspNetCore
```

```html
<short-number value="1234567.89" format="currency" currency-code="USD" decimal-places="2" css-class="money"></short-number>
```

---

### 5. View Component

```csharp
@await Component.InvokeAsync("ShortNumber", new { value = 1234567.89m, isCurrency = true, currencyCode = "EUR" })
```

---

### 6. Currency Dictionary Serialization

```csharp
options.JsonSerializerOptions.Converters.Add(new CurrencyDictionaryConverter());
```

```csharp
public class Report
{
    public Dictionary<string, decimal> InternationalRevenue { get; set; }
}
```

```json
{
  "internationalRevenue": {
    "USA": "$5.00M",
    "EUR": "€4.00M",
    "GBP": "£3.50M"
  }
}
```

Supports **round-trip deserialization**, converting formatted strings like `"$5.00M"` back into numeric values.

---

## Options Reference

### ShortNumberFormatOptions

| Property           | Type             | Default | Description                                |
| ------------------ | ---------------- | ------- | ------------------------------------------ |
| DecimalPlaces      | int              | 2       | Number of decimal places in output         |
| ShowPlusSign       | bool             | false   | Prefix positive numbers with `+`           |
| CurrencySymbol     | string?          | null    | Overrides culture's currency symbol        |
| CurrencyPosition   | CurrencyPosition | Before  | Placement of currency symbol               |
| NegativePattern    | string           | "-n"    | Format for negatives: `-n`, `(n)`, `n-`    |
| CustomSuffixes     | string[]?        | null    | Custom suffixes in ascending magnitude     |
| AlwaysShowSuffix   | bool             | false   | Always display suffix even below threshold |
| Threshold          | decimal          | 1000    | Minimum value to apply suffix              |
| PromotionThreshold | decimal          | 0.95    | Early promotion factor (0–1)               |

---

### Understanding Key Options

* **PromotionThreshold** – Promotes values close to the next suffix. Example: `950_000 → 0.95M`
* **AlwaysShowSuffix** – Forces suffix display even below threshold. Example: `500 → 0.5K`

---

### CurrencyPosition Enum

```csharp
public enum CurrencyPosition
{
    Before,              // "$1.23K"
    After,               // "1.23K$"
    BeforeWithSpace,     // "$ 1.23K"
    AfterWithSpace       // "1.23K $"
}
```

---

## Suffix Sets

Available via `NumberSuffixes`:

* **Default** – `K`, `M`, `B`, `T`, `Qa`, `Qi`
* **Financial** – Same as default
* **Scientific** – `k`, `M`, `G`, `T`, `P`, `E`

Custom suffixes can be provided via `ShortNumberFormatOptions.CustomSuffixes`.

---

## Advanced Features

### Global Formatting Attribute

```csharp
[ShortNumberFormatGlobally(isCurrency: true, decimalPlaces: 1)]
public class Summary
{
    public decimal TotalSales { get; set; }
    public decimal AverageOrder { get; set; }
    public int CustomerCount { get; set; }
}
```

---

### Deserialization Support

All JSON converters support parsing formatted strings back into numeric values, enabling full **round-trip scenarios**.

---

### Nullable Numeric Handling

`ShortNumberNullableJsonConverter<T>` ensures proper serialization/deserialization of nullable numeric types.

---

### CurrencyDictionaryConverter Round-Trip

Handles both serialization and deserialization using consistent suffix and currency parsing logic.

---

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

---

## License

This project is licensed under the MIT License.
