using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using HumanNumbers.Currencies;
using HumanNumbers.Formatting;
using HumanNumbers.Suffixes;

namespace HumanNumbers
{
    /// <summary>
    /// Provides culture-aware human-readable number formatting extensions.
    /// This is the primary entry point for the HumanNumbers platform.
    /// </summary>
    public static class HumanNumber
    {
        private static readonly MagnitudeSuffix[] DefaultSuffixes = StandardSuffixSets.Default;
        private static readonly ConcurrentDictionary<string, NumberFormatInfo> FormatCache = new();
        private static readonly ConcurrentDictionary<string, string> CurrencySymbolCache = new();
        private static readonly ConcurrentDictionary<string, RegionInfo> RegionCache = new();

        private static readonly Dictionary<string, string> CurrencyToRegion = new(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = "US", ["EUR"] = "FR", ["GBP"] = "GB", ["JPY"] = "JP",
            ["CAD"] = "CA", ["AUD"] = "AU", ["CHF"] = "CH", ["CNY"] = "CN",
            ["SEK"] = "SE", ["NZD"] = "NZ", ["MXN"] = "MX", ["SGD"] = "SG",
            ["HKD"] = "HK", ["NOK"] = "NO", ["KRW"] = "KR", ["TRY"] = "TR",
            ["INR"] = "IN", ["RUB"] = "RU", ["BRL"] = "BR", ["ZAR"] = "ZA",
            ["DKK"] = "DK", ["PLN"] = "PL", ["TWD"] = "TW", ["THB"] = "TH",
            ["MYR"] = "MY"
        };
        
        // Suffix multipliers used for parsing
        private static readonly Dictionary<string, decimal> SuffixMultipliers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["k"] = 1000m,
            ["K"] = 1000m,
            ["M"] = 1000000m,
            ["B"] = 1000000000m,
            ["T"] = 1000000000000m,
            ["Qa"] = 1000000000000000m,
            ["Qi"] = 1000000000000000000m
        };

        #region Public API

        /// <summary>
        /// Configures the global default settings for the HumanNumbers platform.
        /// </summary>
        public static void Configure(Action<HumanNumbersConfig> configure)
        {
            configure?.Invoke(HumanNumbersConfig.Instance);
        }

        /// <summary>
        /// Formats a number to a human-readable string (e.g., 1.23M, 5.68B)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHuman(
            this decimal value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var options = HumanNumbersConfig.Instance.GlobalOptions with 
            { 
                DecimalPlaces = decimalPlaces ?? HumanNumbersConfig.Instance.GlobalOptions.DecimalPlaces 
            };
            return FormatNumber(value, options, culture);
        }

        /// <summary>
        /// Formats a number to a human-readable string with custom options
        /// </summary>
        public static string ToHuman(
            this decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            return FormatNumber(value, options, culture);
        }

        /// <summary>
        /// Formats a number as currency in short form
        /// </summary>
        public static string ToHumanCurrency(
            this decimal value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            
            // If we are in InvariantCulture, we should probably default to en-US for currency formatting
            // to avoid the generic currency symbol '¤'.
            if (culture.Name == "") 
            {
                culture = new CultureInfo("en-US");
            }

            var options = HumanNumbersConfig.Instance.GlobalOptions with
            {
                DecimalPlaces = decimalPlaces ?? HumanNumbersConfig.Instance.GlobalOptions.DecimalPlaces,
                CurrencySymbol = culture.NumberFormat.CurrencySymbol
            };
            return FormatNumber(value, options, culture);
        }

        /// <summary>
        /// Formats a number as currency with explicit currency code
        /// </summary>
        public static string ToHumanCurrency(
            this decimal value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var symbol = GetCurrencySymbol(currencyCode);
            var options = HumanNumbersConfig.Instance.GlobalOptions with
            {
                DecimalPlaces = decimalPlaces ?? HumanNumbersConfig.Instance.GlobalOptions.DecimalPlaces,
                CurrencySymbol = symbol
            };
            return FormatNumber(value, options, culture);
        }

        /// <summary>
        /// Generic version for any numeric type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET7_0_OR_GREATER
        public static string ToHuman<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHuman<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return ToHuman(decimalValue, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                return value.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Formats a number as currency in short form.
        /// Generic version for any numeric type.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return ToHumanCurrency(decimalValue, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                return value.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Formats a number as currency with explicit currency code.
        /// Generic version for any numeric type.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return ToHumanCurrency(decimalValue, currencyCode, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                return value.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Generic version for any nullable numeric type. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHumanCurrency(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Generic version for any nullable numeric type and currency code. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHumanCurrency(value.Value, currencyCode, decimalPlaces, culture) : string.Empty;
        }
        /// <summary>
        /// Formats a nullable decimal to a human-readable string. Returns string.Empty if null.
        /// </summary>
        public static string ToHuman(
            this decimal? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHuman(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number to a human-readable string with custom options. Returns string.Empty if null.
        /// </summary>
        public static string ToHuman(
            this decimal? value,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHuman(value.Value, options, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number as currency in short form. Returns string.Empty if null.
        /// </summary>
        public static string ToHumanCurrency(
            this decimal? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHumanCurrency(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number as currency with explicit currency code. Returns string.Empty if null.
        /// </summary>
        public static string ToHumanCurrency(
            this decimal? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHumanCurrency(value.Value, currencyCode, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Generic version for any nullable numeric type. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHuman<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHuman<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHuman(value.Value, decimalPlaces, culture) : string.Empty;
        }

        #region Obsolete Aliases (Backward Compatibility)

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal, HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal value, HumanNumberFormatOptions options, CultureInfo? culture = null)
            => ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman{T}(T, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal? value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal?, HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal? value, HumanNumberFormatOptions options, CultureInfo? culture = null)
            => ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal? value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal?, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal? value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman{T}(T?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T? value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToShortString<T>(this T? value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => ToHuman(value, decimalPlaces, culture);

        #endregion

        /// <summary>
        /// Parses a formatted short numeric string (e.g., "$1.5M", "50K") back to a decimal.
        /// Uses CultureInfo.InvariantCulture by default.
        /// </summary>
        public static bool TryParse(string? value, out decimal result)
        {
            return TryParse(value, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Parses a formatted short numeric string (e.g., "$1.5M", "50K") back to a decimal using the specified culture.
        /// </summary>
        public static bool TryParse(string? value, CultureInfo? culture, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            culture ??= CultureInfo.InvariantCulture;
            var nfi = culture.NumberFormat;
            var decimalSeparator = nfi.NumberDecimalSeparator[0];
            var groupSeparator = nfi.NumberGroupSeparator[0];

            var span = value.AsSpan().Trim();

            // Strip non-numeric prefixes (like currency symbols)
            while (span.Length > 0 && !char.IsDigit(span[0]) && span[0] != '+' && span[0] != '-' && span[0] != decimalSeparator)
            {
                span = span.Slice(1).TrimStart();
            }

            // Find where the number ends
            int numEnd = 0;
            while (numEnd < span.Length && (char.IsDigit(span[numEnd]) || span[numEnd] == decimalSeparator || span[numEnd] == groupSeparator || span[numEnd] == '+' || span[numEnd] == '-'))
            {
                numEnd++;
            }

            var numberPart = span.Slice(0, numEnd);
            var suffixPart = span.Slice(numEnd).Trim();

            // Strip trailing non-numeric/non-letter characters (like trailing currency symbols)
            int suffixEnd = suffixPart.Length;
            while (suffixEnd > 0 && !char.IsLetter(suffixPart[suffixEnd - 1]))
            {
                suffixEnd--;
            }
            suffixPart = suffixPart.Slice(0, suffixEnd);

            if (decimal.TryParse(numberPart.ToString(), NumberStyles.Any, culture, out var number))
            {
                if (suffixPart.Length > 0)
                {
                    var suffixStr = suffixPart.ToString();
                    if (SuffixMultipliers.TryGetValue(suffixStr, out var multiplier))
                    {
                        result = number * multiplier;
                        return true;
                    }
                }
                else
                {
                    result = number;
                    return true;
                }
            }

            // Fallback: Try parsing the whole string natively
            return decimal.TryParse(value, NumberStyles.Any, culture, out result);
        }

        /// <summary>
        /// Parses a formatted short numeric string, throwing an exception if invalid.
        /// Uses CultureInfo.InvariantCulture by default.
        /// </summary>
        public static decimal Parse(string? value)
        {
            return Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a formatted short numeric string using the specified culture, throwing an exception if invalid.
        /// </summary>
        public static decimal Parse(string? value, CultureInfo? culture)
        {
            if (TryParse(value, culture, out var result))
                return result;
            throw new FormatException($"Input string '{value}' was not in a correct format.");
        }

        #endregion

        #region Core Formatting Logic

        /// <summary>
        /// Core formatting logic for short number conversion.
        /// </summary>
        private static string FormatNumber(
            decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture)
        {
            culture ??= CultureInfo.CurrentCulture;
            var numberFormat = GetNumberFormatInfo(culture);

            // Handle zero
            if (value == 0 && !options.AlwaysShowSuffix)
            {
                return options.CurrencySymbol != null
                    ? FormatCurrencyNumber(0, "", options, numberFormat)
                    : "0";
            }

            var isNegative = value < 0;
            var absValue = Math.Abs(value);

            // Get appropriate suffix
            var suffixes = options.CachedCustomSuffixes ?? DefaultSuffixes;

            var (divisor, suffix) = GetSuffixAndDivisor(absValue, suffixes, options.Threshold, options.PromotionThreshold, options.AlwaysShowSuffix);

            // Format the number
            var scaledValue = absValue / divisor;
            var roundedValue = Math.Round(scaledValue, options.DecimalPlaces, MidpointRounding.AwayFromZero);

            // Build the formatted number part
            var formattedNumber = roundedValue.ToString($"F{options.DecimalPlaces}", numberFormat);

            // Apply currency if needed
            string result;
            if (options.CurrencySymbol != null)
            {
                result = FormatCurrencyNumber(roundedValue, suffix, options, numberFormat);
            }
            else
            {
                result = options.ShowPlusSign && !isNegative && value > 0
                    ? $"+{formattedNumber}{suffix}"
                    : $"{formattedNumber}{suffix}";
            }

            // Handle negative numbers
            return isNegative ? FormatNegativeNumber(result, options, numberFormat) : result;
        }

        private static string FormatCurrencyNumber(
            decimal value,
            string suffix,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat)
        {
            var symbol = options.CurrencySymbol ?? numberFormat.CurrencySymbol;
            var formattedValue = value.ToString($"F{options.DecimalPlaces}", numberFormat);
            var numberWithSuffix = $"{formattedValue}{suffix}";

            return options.CurrencyPosition switch
            {
                CurrencyPosition.Before => $"{symbol}{numberWithSuffix}",
                CurrencyPosition.After => $"{numberWithSuffix}{symbol}",
                CurrencyPosition.BeforeWithSpace => $"{symbol} {numberWithSuffix}",
                CurrencyPosition.AfterWithSpace => $"{numberWithSuffix} {symbol}",
                _ => $"{symbol}{numberWithSuffix}"
            };
        }

        private static string FormatNegativeNumber(
            string formattedNumber,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat)
        {
            return options.NegativePattern switch
            {
                "(n)" => $"({formattedNumber})",
                "n-" => $"{formattedNumber}-",
                _ => $"-{formattedNumber}" // "-n" is default
            };
        }

        private static (decimal Divisor, string Suffix) GetSuffixAndDivisor(
            decimal value,
            MagnitudeSuffix[] suffixes,
            decimal threshold,
            decimal promotionThreshold,
            bool alwaysShowSuffix)
        {
            if (value < threshold && !alwaysShowSuffix)
                return (1m, "");

            for (int i = 0; i < suffixes.Length; i++)
            {
                var current = suffixes[i];
                if (value >= current.Threshold)
                    return (current.Threshold, current.Suffix);

                if (i > 0 && value >= suffixes[i - 1].Threshold * promotionThreshold)
                    return (suffixes[i - 1].Threshold, suffixes[i - 1].Suffix);
            }

            if (alwaysShowSuffix && suffixes.Length > 1)
            {
                var smallestSuffix = suffixes[suffixes.Length - 2];
                return (smallestSuffix.Threshold, smallestSuffix.Suffix);
            }

            return (1m, "");
        }

        #endregion

        #region Helpers

        internal static MagnitudeSuffix[] CreateCustomSuffixes(string[] suffixes)
        {
            var result = new List<MagnitudeSuffix>();
            var multiplier = 1m;

            for (int i = suffixes.Length - 1; i >= 0; i--)
            {
                if (i > 0)
                {
                    multiplier *= 1000;
                }
                result.Add(new MagnitudeSuffix(multiplier, suffixes[i]));
            }

            return result.OrderByDescending(x => x.Threshold).ToArray();
        }

        private static NumberFormatInfo GetNumberFormatInfo(CultureInfo culture)
        {
            var key = culture.Name;
            return FormatCache.GetOrAdd(key, _ => culture.NumberFormat);
        }

        private static string GetCurrencySymbol(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode)) return string.Empty;
            currencyCode = currencyCode.ToUpperInvariant();

            return CurrencySymbolCache.GetOrAdd(currencyCode, code =>
            {
                if (CurrencyToRegion.TryGetValue(code, out var regionName))
                {
                    try
                    {
                        var ri = RegionCache.GetOrAdd(regionName, name => new RegionInfo(name));
                        var symbol = ri.CurrencySymbol;
                        if (symbol == "￥") symbol = "¥";
                        return symbol;
                    }
                    catch { }
                }

                // If specialized mapping fails, fallback safely to the code itself to prevent crashes.
                return code;
            });
        }

        #endregion
    }
}

namespace NumberFormatter
{
    using System;
    using System.Globalization;
    using HumanNumbers.Formatting;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.HumanNumber"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.HumanNumber instead. This alias will be removed in a future version.")]
    public static class NumberFormatter
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
        public static string ToShortString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman(decimal, HumanNumbers.Formatting.HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
        public static string ToShortString(this decimal value, ShortNumberFormatOptions options, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman{T}(T, int?, System.Globalization.CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => HumanNumbers.HumanNumber.ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHumanCurrency(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHumanCurrency instead.")]
        public static string ToShortCurrencyString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHumanCurrency(decimal, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHumanCurrency instead.")]
        public static string ToShortCurrencyString(this decimal value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.TryParse(string?, out decimal)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.TryParse instead.")]
        public static bool TryParse(string? value, out decimal result)
            => HumanNumbers.HumanNumber.TryParse(value, out result);
    }
}