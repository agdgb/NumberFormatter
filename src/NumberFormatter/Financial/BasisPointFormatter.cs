using System;
using System.Globalization;

namespace NumberFormatter.Financial;

/// <summary>
/// Extension methods for converting numbers to and from Basis Points (BPS).
/// 1 Basis Point = 0.01% = 0.0001
/// </summary>
public static class BasisPointFormatter
{
    private const decimal BpsMultiplier = 10000m;

    /// <summary>
    /// Converts a decimal to its Basis Points value (e.g., 0.0125m becomes 125m).
    /// </summary>
    public static decimal ToBps(this decimal value)
    {
        return value * BpsMultiplier;
    }

    /// <summary>
    /// Converts a decimal to a Basis Points string (e.g., 0.0125m becomes "125 bps").
    /// </summary>
    /// <param name="value">The decimal value.</param>
    /// <param name="decimals">Number of decimal places to round the BPS value to.</param>
    /// <param name="rounding">The rounding strategy.</param>
    /// <param name="provider">Optional format provider.</param>
    public static string ToBpsString(
        this decimal value, 
        int decimals = 0, 
        MidpointRounding rounding = MidpointRounding.AwayFromZero,
        IFormatProvider? provider = null)
    {
        var bps = value * BpsMultiplier;
        var roundedBps = Math.Round(bps, decimals, rounding);
        
        return $"{roundedBps.ToString($"F{decimals}", provider)} bps";
    }

    /// <summary>
    /// Parses a Basis Points string (e.g., "125 bps", "125", "123.45 bps") back into a decimal (e.g., 0.0125).
    /// </summary>
    public static bool TryParseBps(string? input, out decimal result, IFormatProvider? provider = null)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var span = input.AsSpan().Trim();
        
        if (span.EndsWith("bps".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            span = span.Slice(0, span.Length - 3).Trim();
        }

        var style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
        if (decimal.TryParse(span, style, provider ?? CultureInfo.InvariantCulture, out var parsedBps))
        {
            result = parsedBps / BpsMultiplier;
            return true;
        }

        return false;
    }
}
