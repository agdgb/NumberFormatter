using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HumanNumbers.Currencies;
using HumanNumbers.Suffixes;

namespace HumanNumbers.Formatting
{
    /// <summary>
    /// Mutable configuration class for customizing <see cref="HumanNumber"/> behavior.
    /// Controls decimal precision, currency display, suffix logic, and number patterns.
    /// </summary>
    /// <remarks>
    /// Defaults produce standard short formatting like "1.23M" or "$1.23K".
    /// Supports culture-aware formatting via <see cref="HumanNumber"/> methods.
    /// </remarks>
    public record struct HumanNumberFormatOptions()
    {
        /// <summary>
        /// An optional action triggered when a generic parsing or casting exception occurs.
        /// This enables centralized error logging (e.g., Sentry) without crashing the API output.
        /// </summary>
        public Action<Exception>? OnFormattingError { get; set; }

        /// <summary>
        /// Controls how formatting errors are handled (default: <see cref="HumanNumbersErrorMode.SafeFallback"/>).
        /// </summary>
        public HumanNumbersErrorMode ErrorMode { get; set; } = HumanNumbersErrorMode.SafeFallback;

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

        private string[]? _customSuffixes;

        /// <summary>
        /// Replaces standard suffixes (e.g. <c>["thou", "lac", "cr"]</c> for Indian numbering).
        /// </summary>
        /// <remarks>
        /// Array processed largest-to-smallest. Thresholds auto-assigned as 10³, 10⁶, 10⁹...
        /// <example><c>["K", "M", "B"]</c> → same as <see cref="StandardSuffixSets.Default"/></example>
        /// </remarks>
        public string[]? CustomSuffixes
        {
            get => _customSuffixes;
            set
            {
                _customSuffixes = value;
                CachedCustomSuffixes = value != null ? HumanNumber.CreateCustomSuffixes(value) : null;
            }
        }

        /// <summary>
        /// Gets or sets the manually defined magnitude suffixes. 
        /// Use this to override standard 3-digit scaling (e.g., for Chinese 10,000-based scaling).
        /// Setting <see cref="CustomSuffixes"/> will automatically populate this collection.
        /// </summary>
        public MagnitudeSuffix[]? CachedCustomSuffixes { get; set; }

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
}

namespace NumberFormatter
{
    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Formatting.HumanNumberFormatOptions"/>.
    /// </summary>
    [System.Obsolete("Use HumanNumbers.Formatting.HumanNumberFormatOptions instead. This alias will be removed in a future version.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public record struct ShortNumberFormatOptions()
    {
        private HumanNumbers.Formatting.HumanNumberFormatOptions _options = new();

        /// <summary>Error handling mode.</summary>
        public HumanNumbers.HumanNumbersErrorMode ErrorMode { get => _options.ErrorMode; set => _options.ErrorMode = value; }
        /// <summary>Number of decimal places.</summary>
        public int DecimalPlaces { get => _options.DecimalPlaces; set => _options.DecimalPlaces = value; }
        /// <summary>Show plus sign for positive numbers.</summary>
        public bool ShowPlusSign { get => _options.ShowPlusSign; set => _options.ShowPlusSign = value; }
        /// <summary>Currency symbol override.</summary>
        public string? CurrencySymbol { get => _options.CurrencySymbol; set => _options.CurrencySymbol = value; }
        /// <summary>Currency symbol position.</summary>
        public HumanNumbers.Currencies.CurrencyPosition CurrencyPosition { get => _options.CurrencyPosition; set => _options.CurrencyPosition = value; }
        /// <summary>Negative number pattern.</summary>
        public string NegativePattern { get => _options.NegativePattern; set => _options.NegativePattern = value; }
        /// <summary>Custom magnitude suffixes.</summary>
        public string[]? CustomSuffixes { get => _options.CustomSuffixes; set => _options.CustomSuffixes = value; }
        /// <summary>Always show suffix.</summary>
        public bool AlwaysShowSuffix { get => _options.AlwaysShowSuffix; set => _options.AlwaysShowSuffix = value; }
        /// <summary>Formatting threshold.</summary>
        public decimal Threshold { get => _options.Threshold; set => _options.Threshold = value; }
        /// <summary>Promotion threshold factor.</summary>
        public decimal PromotionThreshold { get => _options.PromotionThreshold; set => _options.PromotionThreshold = value; }

        /// <summary>Implicit conversion from old options to new.</summary>
        public static implicit operator HumanNumbers.Formatting.HumanNumberFormatOptions(ShortNumberFormatOptions old) => old._options;
        /// <summary>Implicit conversion from new options to old.</summary>
        public static implicit operator ShortNumberFormatOptions(HumanNumbers.Formatting.HumanNumberFormatOptions @new) => new() { _options = @new };
    }
}
