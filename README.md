# HumanNumbers

**Human-readable numbers for .NET — a governed, high-performance, API-safe formatting engine.**

[![NuGet](https://img.shields.io/nuget/v/HumanNumbers.svg)](https://www.nuget.org/packages/HumanNumbers)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## ⚡ Quick Start

```csharp
using HumanNumbers;

1500.ToHuman();        // "1.50K"
1500000.ToHuman();     // "1.50M"
```

---

## 🎯 Why HumanNumbers?

Formatting numbers correctly at scale is deceptively complex. Naive implementations often:
- **Break at rounding boundaries**: (e.g., `999,499` rounding to `1,000K` instead of `1.00M`).
- **Allocate excessively**: Creating intermediate strings in hot telemetry or logging paths.
- **Lack consistency**: Different services formatting the same data in conflicting ways.

**HumanNumbers** provides a governed, high-performance platform for number presentation that prioritizes:
- **Correctness**: Smart suffix promotion logic that handles rounding boundaries gracefully.
  ```csharp
  999499m.ToHuman();                                      // "1.00M" (Readable)
  999499m.ToHuman(HumanNumberFormatOptions.StrictPreset); // "999.50K" (Accurate)
  ```
- **Performance**: Optimized `Span<char>`-based paths and zero-allocation parsing.
- **Safety**: Non-intrusive ASP.NET Core integration that preserves API contracts by default.
- **Governance**: A central Policy system to ensure consistent formatting across distributed services.

---

## 🚀 Comparison: Three Ways to Format

| Approach | Code | Trade-off |
| :--- | :--- | :--- |
| **Naive .NET** | `(val / 1000).ToString("F2") + "K"` | Error-prone at thresholds, high allocation, no culture support. |
| **Fluent API** | `val.ToHuman(2)` | **Balanced.** Minimal allocation with full formatting support. |
| **Span API** | `val.ToHuman(span, out _)` | **High-Performance.** Zero-allocation; ideal for telemetry and logging. |

> [!NOTE]
> The "Naive" example reflects common real-world implementations, not optimized hand-written formatters. Both APIs use the same high-performance core engine; the Span overload simply bypasses string materialization for critical paths.

---

## 🔢 Core Formatting Engine

### Readable vs. Strict Promotion
By default, we prioritize **readability**. If a number rounds up to the next threshold (e.g., `999,499` to `1.00M`), we promote the suffix. For audit-heavy scenarios, use **Strict Mode**:

```csharp
999499m.ToHuman();                                      // "1.00M" (Readable Default)
999499m.ToHuman(HumanNumberFormatOptions.StrictPreset); // "999.50K" (Strict Accuracy)
```

### Specialized Units
- **Financial**: `1234.56m.ToHumanWords(); // "One Thousand Two Hundred..."`
- **Bytes**: `1024L.ToHumanBytes(); // "1.00 KiB"` (Supports Binary/Decimal)
- **Fractions**: `1.5m.ToHumanFraction(32); // "1 16/32"`
- **Roman**: `2024.ToRoman(); // "MMXXIV"`

---

## 🌍 Internationalization & Global Support

**HumanNumbers** is built on top of the native .NET `CultureInfo` system, meaning it automatically respects global numbering rules, separators, and currency symbols out of the box.

### Verified Global Outputs (Examples)

| Culture | Scaled Human | Currency Format | Non-Scaled Grouping | Notes |
| :--- | :--- | :--- | :--- | :--- |
| **ar-SA** (Arabic) | `1٫25M` | `ر.س.‏ 1٫25M` | `1٬000٬000` | Uses unique Arabic separators. |
| **hi-IN** (Hindi) | `1.25M` | `₹1.25M` | `10,00,000` | **Lakh/Crore** grouping (10,00,000). |
| **fr-FR** (French) | `1,25M` | `1,25M €` | `1 000 000` | Space separator and trailing currency. |
| **de-DE** (German) | `1,25M` | `1,25M €` | `1.000.000` | Comma decimal and dot grouping. |
| **am-ET** (Amharic) | `1.25M` | `Br1.25M` | `1,000,000` | Custom Ethiopian currency symbol. |

### Custom Words (Financial I18n)
For non-English support in financial word-formatting (Check Writing), implement the `IWordsProvider` interface:

```csharp
public class SpanishWordsProvider : IWordsProvider
{
    public string NegativeWord => "Negativo";
    public string ConjunctionWord => "con";
    public string ToWords(decimal value) => "Mil Doscientos"; 
}

// Usage
1200m.ToHumanWords(provider: new SpanishWordsProvider()); // "Mil Doscientos"
```

---

## 🧩 Policy-Driven Formatting (Enterprise Ready)

Define formatting rules once and enforce them across your entire infrastructure. Ensure that APIs, dashboards, and reports share the same magnitude logic and rounding rules.

```csharp
// Setup central governance
builder.Services.AddHumanNumbersDefaults(options =>
{
    options.AddPolicy("Finance", new HumanNumberFormatOptions
    {
        DecimalPlaces = 2,
        PromotionThreshold = 1.0m // strict mode: 999,499 -> "999.50K"
    });
});

// Apply consistently across services
HumanNumber.Format(value).UsingPolicy("Finance").ToHuman();
```

### 🌍 Multi-Culture & Custom Magnitude Examples

The policy system handles unique numbering systems (like 10,000-based scaling) with ease:

```csharp
builder.Services.AddHumanNumbersDefaults(options =>
{
    // Indian System: Lakhs (10^5) and Crores (10^7)
    options.AddPolicy("Indian", new HumanNumberFormatOptions
    {
        CachedCustomSuffixes = new[] {
            new MagnitudeSuffix(1000m, "K"),
            new MagnitudeSuffix(100_000m, "Lakh"),
            new MagnitudeSuffix(10_000_000m, "Crore")
        },
        Threshold = 1000m,
        DecimalPlaces = 2
    });

    // Chinese System: Wàn (10^4) and Yì (10^8)
    options.AddPolicy("Chinese", new HumanNumberFormatOptions
    {
        CachedCustomSuffixes = new[] {
            new MagnitudeSuffix(10_000m, "Wàn"),
            new MagnitudeSuffix(100_000_000m, "Yì")
        },
        Threshold = 10_000m
    });
});

// Usage
120000m.ToHuman(HumanNumbersConfig.Instance.GetPolicies()["Indian"]); // "1.20 Lakh"
120000m.ToHuman(HumanNumbersConfig.Instance.GetPolicies()["Chinese"]); // "12.00 Wàn"
```

> [!TIP]
> Use `CachedCustomSuffixes` for non-standard scaling (like 10^4 or 10^5). For standard 10^3 scaling, simply use `CustomSuffixes`.

---

## 🚀 Real-World Scenario: Zero-Allocation Telemetry

In high-frequency logging or telemetry, even small string allocations can trigger GC pressure. **HumanNumbers** allows you to format directly into reusable buffers:

```csharp
// Reusable buffer (stack or ArrayPool)
Span<char> buffer = stackalloc char[32];

if (largeValue.ToHuman(buffer, out var written))
{
    // Log directly from the span without creating a string
    logger.LogInformation("Metric: {Value}", buffer[..written]);
}
```

---

## 📊 Performance & Engineering Nuance

We benchmark against "Naive" implementations to provide an honest look at the costs of convenience.

| Operation | Latency | **Allocated Memory** | Engineering Note |
| :--- | :--- | :--- | :--- |
| **Manual Concatenation** | ~70 ns | 80 B | Standard `ToString() + "K"` approach. |
| **`ToHuman()`** | ~160 ns | **56 B** | **Balanced.** Uses less memory than naive concatenation (56 B vs 80 B). |
| **`ToHuman` (Span)** | ~140 ns | **0 B** | **Zero-Alloc.** When using Span overloads with preconfigured policies. |
| **`TryParse`** | ~45 ns | **0 B** | **Zero-Alloc.** Fully allocation-free parsing. |

> [!NOTE]
> Latency figures are environment-dependent. The "Naive" example reflects common real-world implementations, not optimized hand-written formatters.

---

## 🌐 ASP.NET Core: Safe by Default

Many libraries implicitly change JSON output globally, breaking API contracts. **HumanNumbers** is designed to be non-intrusive.

> [!IMPORTANT]
> By default, HumanNumbers **does not modify JSON output** unless explicitly enabled via attributes or result extensions.

### 1. Register Policies
```csharp
builder.Services.AddHumanNumbersDefaults(options => {
    options.AddPolicy("Compact", new HumanNumberFormatOptions { DecimalPlaces = 1 });
});
```

### 2. Selective Serialization
Control exactly what the client sees without breaking your DTOs.
```csharp
public class AnalyticsDto {
    public decimal RawValue { get; set; } // JSON: 1500000

    [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]
    public decimal DisplayValue { get; set; } // JSON: "1.50M"
}
```

### 3. Explicit Transformations
In Minimal APIs, use `HumanOk` to trigger the transformation only when intended:
```csharp
app.MapGet("/stats", () => Results.Extensions.HumanOk(new { Revenue = 1500000 }));
```

---

## 🧠 Design Philosophy
- **Predictable Performance**: Every feature has a known and stable allocation profile.
- **Governance First**: Use the `Policy` system to define "Brand Guidelines" for numbers once, then apply them everywhere.
- **Allocation Aware**: We leverage `Span<char>` and `ISpanFormattable` to ensure that adding "humanity" to your data doesn't sink your GC performance.
- **Contract Safety**: Your API types remain `decimal`. We only change the *representation* during the final serialization step.

---

---

## 🔄 Migration from `NumberFormatter`

`HumanNumbers` is the official successor to the legacy `NumberFormatter` package. It features a modernized API, significantly improved performance (via `Span<char>`), and a unified policy system.

### Key Changes
- **Namespace**: `NumberFormatter` → `HumanNumbers`
- **Method Renaming**:
  - `ToShortString()` → `ToHuman()`
  - `ToShortCurrencyString()` → `ToHumanCurrency()`
- **Binary Compatibility**: A legacy shim is provided in the `NumberFormatter` namespace (marked as `[Obsolete]`) to help with a zero-friction transition.

```csharp
// Legacy (NumberFormatter)
using NumberFormatter;
1500.ToShortString(); 

// Modern (HumanNumbers)
using HumanNumbers;
1500.ToHuman();
```

---

## 📦 Installation
```bash
dotnet add package HumanNumbers
dotnet add package HumanNumbers.AspNetCore
```

---

## 🙌 Contributing & License
Licensed under **MIT**. Contributions are welcome via Issues and Pull Requests.

---

## 📈 Appendix: Detailed Benchmarks

Detailed benchmarks below reflect full production runs and may differ slightly from summarized figures above. All results were generated using **BenchmarkDotNet v0.14.0** on **.NET 10.0** (X64 RyuJIT).

| Method | Scenario / Input | Mean | Gen 0 | **Allocated** | Engineering Context |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`StandardScaled`** | Naive (999,499) | 134.61 ns | 0.0026 | 80 B | Naive `ToString` + Concat approach. |
| **`ToHuman`** | Governed (999,499) | 351.97 ns | 0.0014 | **56 B** | **23% less memory** than naive. |
| **`ToHuman` (Span)** | Governed (999,499) | 302.92 ns | **0** | **0 B** | **Zero-Alloc** path verified. |
| | | | | | |
| **`TryParse`** | `$1.50M` | 85.82 ns | - | **0 B** | **Zero-Alloc** parsing. |
| **`ToHumanBytes`** | 1024 Bytes | 60.99 ns | 0.0013 | 40 B | Single materialized string. |
| **`ToHumanWords`** | 1234.56 | 513.41 ns | 0.0186 | 560 B | Non-recursive assembly. |
| **`ToRoman`** | 2024 | 50.66 ns | 0.0013 | 40 B | Optimized buffer path. |

**Key Takeaway**: While `HumanNumbers` handles complex thresholds and rounding logic that manual code often misses, it does so with a **smaller memory footprint** than naive string concatenation approaches.

---

## 🔎 Keywords
human-readable numbers, number formatting, suffix formatting, K/M/B formatting, span formatting, zero allocation, .NET performance, telemetry formatting, financial formatting, magnitude formatting.
