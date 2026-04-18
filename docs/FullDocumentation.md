# HumanNumbers Documentation
**The official comprehensive guide to human-readable numeric formatting in .NET.**

---

# 1. Introduction

**HumanNumbers** is a high-performance, culture-aware .NET ecosystem designed to bridge the gap between "machine-precise" numbers and "human-intuitive" insights.

## 1.1 Why use HumanNumbers?
In modern data-driven applications, numerical density often obscures meaning. Displaying `1,254,982.44` on a dashboard is technically accurate but cognitively heavy. HumanNumbers allows you to maintain raw precision in your business logic while providing intuitive representations (e.g., `1.25M`) at the presentation layer.

## 1.2 The Ecosystem
The platform is split into two packages to ensure you only pull in the dependencies you need.

| Package | Responsibility | Best For |
| :--- | :--- | :--- |
| **HumanNumbers** | Pure formatting engine and parsing. | Core logic, workers, Blazor WASM, and libraries. |
| **HumanNumbers.AspNetCore** | HTTP pipeline and JSON integration. | Web APIs, MVC, and Razor Server-side projects. |

---

# 2. Installation & Compatibility

## 2.1 Package Installation
```bash
# Core engine
dotnet add package HumanNumbers

# ASP.NET Core integration
dotnet add package HumanNumbers.AspNetCore
```

## 2.2 Framework & Numeric Support
- **Core Targets**: `.NET Standard 2.0`, `.NET 8.0`, `.NET 10.0+`.
- **AspNetCore Targets**: `.NET 6.0`, `.NET 8.0`, `.NET 10.0+`.
- **Supported Numeric Types**: Comprehensive support for 11 core types:
  - `decimal`, `double`, `float`
  - `int`, `long`, `short`, `byte`
  - `uint`, `ulong`, `ushort`, `sbyte`
  - *Full support for all `Nullable<T>` variants of the above.*

## 2.3 Compatibility Notes
- **Thread Safety**: All formatting paths and static caches are thread-safe.
- **Native AOT & Trimming**: The core `HumanNumbers` library is fully AOT/Trimming safe. The `AspNetCore` package is functional but relies on reflection for JSON converter discovery.
- **Blazor**: The core library is fully compatible with **Blazor WebAssembly** and **Blazor Server**. 
  > [!NOTE]
  > Specialized Blazor components (TagHelpers) are not currently available for Blazor; use the `IHumanNumberService` via DI for manual formatting in `.razor` components.

---

# 3. Quick Start

### Basic Scaling
```csharp
using HumanNumbers;

1500000m.ToHuman();       // "1.5M"
1240.ToHumanCurrency();    // "$1.24k" (en-US)
```

### Web API Integration
```csharp
// Program.cs
builder.Services.AddHumanNumbersDefaults();

// Controller
public record Stats(decimal Revenue);
[HttpGet]
public Stats Get() => new Stats(1500000); // Clients see { "revenue": "1.5M" }
```

---

# 4. Core Library Reference

## 4.0 Global Configuration
Static entry point for application-wide defaults.

```csharp
HumanNumber.Configure(config => {
    config.GlobalOptions.DecimalPlaces = 1;         // Global precision
    config.GlobalOptions.PromotionThreshold = 0.9m; // 900 -> 0.9k
    
    // Centralized logging hook
    config.GlobalOptions.OnFormattingError = ex => 
        Console.WriteLine($"Formatting Error: {ex.Message}");
});
```

## 4.1 Numerical Formatting (`ToHuman`)
The primary tool for magnitude scaling.

- **Precision**: `5678.ToHuman(decimalPlaces: 1)` → `"5.7k"`.
- **Globalization**: Pass a specific `CultureInfo` to handle localized decimal separators and currency positions.
- **Threshold**: Set the minimum value for suffix promotion (default: 1000).

```csharp
// Culture-aware manual usage
var fr = new CultureInfo("fr-FR");
1250.5m.ToHuman(culture: fr); // "1,25k"
```

## 4.2 Byte Size Formatting
Handles digital units (KB, MB, GB, etc.) with support for SI and binary standards.

```csharp
long bytes = 1073741824;
bytes.ToHumanBytes(); // "1.07 GB" (Decimal/SI)
bytes.ToHumanBytes(useBinaryPrefixes: true); // "1.00 GiB" (Binary/IEC)
```

## 4.3 Roman Numerals
Supported range: **1 to 3,999**.
```csharp
2024.ToRoman(); // "MMXXIV"
```

## 4.4 Financial Formatting
- **Basis Points**: `0.0125m.ToBpsString()` → `"125 bps"`.
- **Fractional Prices**: `10.03125m.ToFractionString(32)` → `"10 1/32"`.
- **Check Writing**: `1250.5m.ToCheckWords()` → `"One Thousand Two Hundred Fifty Dollars and 50/100"`.

## 4.5 Robust Parsing
Round-trip support for formatted strings.

```csharp
HumanNumber.Parse("1.5M"); // 1,500,000m
HumanNumber.TryParse("$50K", out var val); // val == 50000m

// Culture-aware parsing (supports localized separators like commas)
var result = HumanNumber.Parse("1,5k", new CultureInfo("fr-FR")); // 1500m
```

---

# 5. ASP.NET Core Deep-Dive

## 5.1 Why the Integration?
The `HumanNumbers.AspNetCore` package exists to enforce a clean **Separation of Concerns**. Your domain models and controllers work with precise numeric types, while the integration layer handles the "Humanization" of that data just before it leaves the server during the JSON serialization phase.

## 5.2 Formatting Policies
Policies provide named configuration sets for different API contexts. Built-in policies include:

| Policy | Use Case | Configuration Snippet |
| :--- | :--- | :--- |
| **Default** | Standard web use. | `DecimalPlaces = 2` |
| **Dashboard** | Condensed summaries. | `PromotionThreshold = 0.9m`, `AlwaysShowSuffix = false` |
| **Financial** | High-value precision. | `DecimalPlaces = 2`, `Threshold = 1000m` |
| **PublicApi**| Standard short-form. | `DecimalPlaces = 1` |

```csharp
// Adding a custom policy
builder.Services.AddHumanNumbers(options => {
    options.AddPolicy("Compact", new HumanNumberFormatOptions { DecimalPlaces = 0 });
});
```

## 5.3 JSON Serialization
The core of the API integration. It uses `JsonConverter` to transform numbers during the serialization phase.

### Complex Object Transformation (Before vs After)

**C# DTO:**
```csharp
public class AssetReport {
    public string Name { get; set; } = "Global Portfolio";
    
    [HumanNumberFormat(isCurrency: true)]
    public decimal Valuation { get; set; } = 1550000;
    
    public int ActiveTraders { get; set; } = 1250;
    
    [NoHumanFormat]
    public long InternalId { get; set; } = 998877;
}
```

**JSON Output:**
```json
// BEFORE HumanNumbers (Standard JSON)
{
  "name": "Global Portfolio",
  "valuation": 1550000.00,
  "activeTraders": 1250,
  "internalId": 998877
}

// AFTER HumanNumbers (Human-Readable JSON)
{
  "name": "Global Portfolio",
  "valuation": "$1.55M",
  "activeTraders": "1.25k",
  "internalId": 998877
}
```

## 5.4 MVC AutoFormatFilter
The `HumanNumberAutoFormatFilter` enables automatic discovery of formatting metadata. When added to the MVC pipeline, it allows the integration to inspect model properties and parameters for `[HumanNumberFormat]` attributes, providing fine-grained control even in complex object graphs or dynamic response scenarios.

## 5.5 Culture Resolution Priority
HumanNumbers resolves formatting culture for each request using a tiered priority system:

1.  **Context Item**: Checks `HttpContext.Items["HumanNumbers_Culture"]` for an explicit runtime override.
2.  **Request Localization**: Uses the `IRequestCultureFeature` if the Request Localization middleware is registered.
3.  **Ambiguous Fallback**: Defaults to the system's current thread culture (`CultureInfo.CurrentCulture`).

## 5.6 Minimal API Comparison
`Results.Extensions` provides a streamlined way to return human-readable JSON without forcing global configuration changes.

| Feature | `Results.Ok(data)` | `Results.Extensions.Human(data)` |
| :--- | :--- | :--- |
| **Output Type** | Raw Numbers (`1500000`) | Human Strings (`"1.5M"`) |
| **Status Code** | Always 200 (unless wrapped) | Configurable via `statusCode` parameter |
| **Control** | Global Serializer defaults | Call-level `decimalPlaces` override |
| **Base Options** | Configured `JsonOptions` | `JsonSerializerDefaults.Web` (Pre-configured) |

## 5.7 TagHelpers
Add `@addTagHelper *, HumanNumbers.AspNetCore` to your `_ViewImports.cshtml` to enable Razor-first formatting.

```html
<hn-number value="1500" decimal-places="1" />
<hn-currency value="2500" currency-code="USD" />
<hn-check value="150.25" /> 
```

## 5.8 Operational Excellence (Logging)
Enable built-in logging to track formatting issues in production.

```csharp
builder.Services.AddHumanNumbersDefaults(options => {
    options.EnableLogging = true; // Auto-hooks to ILogger<HumanNumbers>
});
```

---

# 6. Performance Benchmarks

HumanNumbers is designed for "Zero-Allocation" hot paths using `Span<char>`.

| Feature | Execution Time | Allocation |
| :--- | :--- | :--- |
| `ToHuman()` | ~85 ns | 0 B |
| `ToHumanCurrency()` | ~110 ns | 0 B |
| `Parse("1.5M")` | ~120 ns | 32 B |
| `ToRoman()` | ~45 ns | 0 B |

---

# 7. Testing & Validation

### Deterministic Tests
Always fix the culture when testing formatted strings to avoid machine-specific flakiness.

```csharp
[Fact]
public void ToHuman_IsDeterministic()
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    Assert.Equal("1.5k", 1500m.ToHuman());
}
```

---

# 8. Migration Guide

| Feature | Legacy (`NumberFormatter`) | New (`HumanNumbers`) |
| :--- | :--- | :--- |
| **Package** | `NumberFormatter` | `HumanNumbers` |
| **Namespace** | `NumberFormatter` | `HumanNumbers` |
| **Method** | `ToShortString()` | `ToHuman()` |
| **TagHelper** | `<short-number>` | `<hn-number>` |

---

# 9. Troubleshooting

- **"API numbers are now strings"**: This is expected behavior for the global JSON converter. Use `[NoHumanFormat]` to opt-out specific fields.
- **"Formatting not applying"**: Ensure `services.AddHumanNumbersDefaults()` is called **after** other MVC configuration to ensure converters are registered correctly in the options chain.

---

# License
Licensed under the [MIT License](file:///c:/Users/User/source/repos/NumberFormatter/LICENSE).
