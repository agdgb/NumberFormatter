using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;

namespace NumberFormatter;

/// <summary>
/// Provides culture-aware short number formatting
/// </summary>
public static class NumberFormatter
{
    private static readonly NumberSuffix[] DefaultSuffixes = NumberSuffixes.Default;
    private static readonly ConcurrentDictionary<string, NumberFormatInfo> FormatCache = new();

    #region Public API

    /// <summary>
    /// Formats a number to a short string (e.g., 1.23M, 5.68B)
    /// </summary>
    public static string ToShortString(
        this decimal value,
        int decimalPlaces = 2,
        CultureInfo? culture = null)
    {
        var options = new ShortNumberFormatOptions { DecimalPlaces = decimalPlaces };
        return FormatNumber(value, options, culture);
    }

    /// <summary>
    /// Formats a number to a short string with custom options
    /// </summary>
    public static string ToShortString(
        this decimal value,
        ShortNumberFormatOptions options,
        CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        return FormatNumber(value, options, culture);
    }

    /// <summary>
    /// Formats a number as currency in short form
    /// </summary>
    public static string ToShortCurrencyString(
        this decimal value,
        int decimalPlaces = 2,
        CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var options = new ShortNumberFormatOptions
        {
            DecimalPlaces = decimalPlaces,
            CurrencySymbol = culture.NumberFormat.CurrencySymbol
        };
        return FormatNumber(value, options, culture);
    }

    /// <summary>
    /// Formats a number as currency with explicit currency code
    /// </summary>
    public static string ToShortCurrencyString(
        this decimal value,
        string currencyCode,
        int decimalPlaces = 2,
        CultureInfo? culture = null)
    {
        var symbol = GetCurrencySymbol(currencyCode);
        var options = new ShortNumberFormatOptions
        {
            DecimalPlaces = decimalPlaces,
            CurrencySymbol = symbol
        };
        return FormatNumber(value, options, culture);
    }

    /// <summary>
    /// Generic version for any numeric type
    /// </summary>
    public static string ToShortString<T>(
        this T value,
        int decimalPlaces = 2,
        CultureInfo? culture = null) where T : INumber<T>
    {
        var decimalValue = Convert.ToDecimal(value);
        return decimalValue.ToShortString(decimalPlaces, culture);
    }

    #endregion

    #region Core Formatting Logic

    /// <summary>
    /// Core formatting logic for short number conversion.
    /// </summary>
    /// <param name="value">The numeric value to format.</param>
    /// <param name="options">Formatting options.</param>
    /// <param name="culture">Culture to use for formatting; defaults to current culture if null.</param>
    /// <returns>The formatted string.</returns>
    private static string FormatNumber(
        decimal value,
        ShortNumberFormatOptions options,
        CultureInfo? culture)
    {
        culture ??= CultureInfo.CurrentCulture;
        var numberFormat = GetNumberFormatInfo(culture);

        // Handle zero
        if (value == 0)
        {
            return options.CurrencySymbol != null
                ? FormatCurrencyNumber(0, "", options, numberFormat)
                : "0";
        }

        var isNegative = value < 0;
        var absValue = Math.Abs(value);

        // Get appropriate suffix
        var suffixes = options.CustomSuffixes != null
            ? CreateCustomSuffixes(options.CustomSuffixes)
            : DefaultSuffixes;

        var (divisor, suffix) = GetSuffixAndDivisor(absValue, suffixes, options.Threshold, options.PromotionThreshold);

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

    /// <summary>
    /// Formats a number as a currency string with the given suffix and options.
    /// </summary>
    /// <param name="value">The numeric value (already scaled and rounded).</param>
    /// <param name="suffix">The suffix (K, M, B, etc.) to append.</param>
    /// <param name="options">Formatting options.</param>
    /// <param name="numberFormat">The culture-specific number format info.</param>
    /// <returns>The formatted currency string.</returns>
    private static string FormatCurrencyNumber(
        decimal value,
        string suffix,
        ShortNumberFormatOptions options,
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

    /// <summary>
    /// Formats a negative number according to the specified negative pattern.
    /// </summary>
    /// <param name="formattedNumber">The formatted positive number (including suffix).</param>
    /// <param name="options">Formatting options containing the negative pattern.</param>
    /// <param name="numberFormat">Unused but kept for consistency.</param>
    /// <returns>The negative-formatted string.</returns>
    private static string FormatNegativeNumber(
        string formattedNumber,
        ShortNumberFormatOptions options,
        NumberFormatInfo numberFormat)
    {
        return options.NegativePattern switch
        {
            "(n)" => $"({formattedNumber})",
            "n-" => $"{formattedNumber}-",
            _ => $"-{formattedNumber}" // "-n" is default
        };
    }

    /// <summary>
    /// Determines the appropriate divisor and suffix for the given absolute value.
    /// </summary>
    /// <param name="value">The absolute value of the number to format.</param>
    /// <param name="suffixes">Array of possible suffixes in descending order (largest first).</param>
    /// <param name="threshold">Minimum value to apply short formatting (default 1000).</param>
    /// <param name="promotionThreshold">Percentage (0-1) of the next suffix threshold to promote.</param>
    /// <returns>A tuple containing the divisor and suffix to use.</returns>
    private static (decimal Divisor, string Suffix) GetSuffixAndDivisor(
        decimal value,
        NumberSuffix[] suffixes,
        decimal threshold,
        decimal promotionThreshold)
    {
        if (value < threshold)
            return (1m, "");

        // Suffixes are ordered descending (largest first)
        for (int i = 0; i < suffixes.Length; i++)
        {
            var current = suffixes[i];
            if (value >= current.Threshold)
                return (current.Threshold, current.Suffix);

            // Check promotion to this suffix if there is a larger one
            if (i > 0 && value >= suffixes[i - 1].Threshold * promotionThreshold)
                return (suffixes[i - 1].Threshold, suffixes[i - 1].Suffix);
        }

        return (1m, "");
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates an array of NumberSuffix from a list of custom suffix strings.
    /// The suffixes are assumed to be in order of increasing magnitude (e.g., "", "K", "M").
    /// The resulting array is ordered descending by threshold.
    /// </summary>
    /// <param name="suffixes">Array of suffix strings, starting from the smallest unit.</param>
    /// <returns>Array of NumberSuffix in descending order (largest threshold first).</returns>
    private static NumberSuffix[] CreateCustomSuffixes(string[] suffixes)
    {
        var result = new List<NumberSuffix>();
        var multiplier = 1m;

        // Create suffixes in descending order (largest first)
        for (int i = suffixes.Length - 1; i >= 0; i--)
        {
            if (i > 0)
            {
                multiplier *= 1000;
            }

            result.Add(new NumberSuffix(multiplier, suffixes[i]));
        }

        return result.OrderByDescending(x => x.Threshold).ToArray();
    }

    /// <summary>
    /// Retrieves or caches the NumberFormatInfo for a given culture.
    /// </summary>
    /// <param name="culture">The culture whose number format is requested.</param>
    /// <returns>The NumberFormatInfo for the culture.</returns>
    private static NumberFormatInfo GetNumberFormatInfo(CultureInfo culture)
    {
        var key = culture.Name;
        return FormatCache.GetOrAdd(key, _ => culture.NumberFormat);
    }

    /// <summary>
    /// Maps a currency code (e.g., "USD") to its symbol.
    /// If the code is not recognized, returns the code itself as a fallback.
    /// </summary>
    /// <param name="currencyCode">The ISO currency code (e.g., "USD", "EUR").</param>
    /// <returns>The corresponding currency symbol.</returns>
    private static string GetCurrencySymbol(string currencyCode)
    {
        // This is a simplified version - in production, you'd want a more robust mapping
        return currencyCode.ToUpperInvariant() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "JPY" => "¥",
            "CNY" => "¥",
            "ETB" => "Br",
            "INR" => "₹",
            _ => currencyCode // Fallback to code if symbol not found
        };
    }

    #endregion
}