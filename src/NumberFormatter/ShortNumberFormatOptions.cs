using System;

namespace NumberFormatter;

/// <summary>
/// Mutable configuration class for customizing <see cref="NumberFormatter"/> behavior.
/// Controls decimal precision, currency display, suffix logic, and number patterns.
/// </summary>
/// <remarks>
/// Defaults produce standard short formatting like "1.23M" or "$1.23K".
/// Supports culture-aware formatting via <see cref="NumberFormatter"/> methods.
/// </remarks>
public class ShortNumberFormatOptions
{
    /// <summary>
    /// Number of decimal places in output (default: <c>2</c>).
    /// </summary>
    /// <example>1234.567 → "1.23K" (2 places) vs "1.235K" (3 places)</example>
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// Prefix positive numbers with plus sign (default: <see langword="false"/>).
    /// </summary>
    /// <example><c>true</c>: "+1.23M"; <c>false</c>: "1.23M"</example>
    public bool ShowPlusSign { get; set; }

    /// <summary>
    /// Overrides culture's currency symbol (default: <see langword="null"/> → uses culture default).
    /// </summary>
    /// <example>"€" → "€1.23M" regardless of culture</example>
    public string? CurrencySymbol { get; set; }

    /// <summary>
    /// Placement of <see cref="CurrencySymbol"/> (default: <see cref="CurrencyPosition.Before"/>).
    /// </summary>
    public CurrencyPosition CurrencyPosition { get; set; } = CurrencyPosition.Before;

    /// <summary>
    /// Negative number format pattern (default: <c>"-n"</c>).
    /// </summary>
    /// <remarks>Supported: <c>"-n"</c> → "-1.23M"; <c>"(n)"</c> → "(1.23M)"; <c>"n-"</c> → "1.23M-"</remarks>
    public string NegativePattern { get; set; } = "-n";

    /// <summary>
    /// Replaces standard suffixes (e.g. <c>["thou", "lac", "cr"]</c> for Indian numbering).
    /// </summary>
    /// <remarks>
    /// Array processed largest-to-smallest. Thresholds auto-assigned as 10³, 10⁶, 10⁹...
    /// <example><c>["K", "M", "B"]</c> → same as <see cref="NumberSuffixes.Default"/></example>
    /// </remarks>
    public string[]? CustomSuffixes { get; set; }

    /// <summary>
    /// Force suffix display for all values (default: <see langword="false"/>).
    /// </summary>
    /// <example><c>true</c>: "0.50K"; <c>false</c>: "500" (if under threshold)</example>
    public bool AlwaysShowSuffix { get; set; }

    /// <summary>
    /// Absolute value minimum for short formatting/suffix (default: <c>1000m</c>).
    /// </summary>
    /// <example>999 → "999"; 1000 → "1K"</example>
    public decimal Threshold { get; set; } = 1000m;

    /// <summary>
    /// Early-promotion factor ∈ [0,1] to next-higher suffix (default: <c>0.95m</c>).
    /// </summary>
    /// <remarks>
    /// When value ≥ (next_threshold × factor), promotes to next suffix.
    /// <example>950_000 ≥ 1_000_000 × 0.95 → "0.95M" instead of "950K"</example>
    /// </remarks>
    public decimal PromotionThreshold { get; set; } = 0.95m;
}
