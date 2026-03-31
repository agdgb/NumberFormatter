using System;
using System.Globalization;
using System.Numerics;

namespace NumberFormatter;

/// <summary>
/// Provides extension methods for formatting byte sizes.
/// </summary>
public static class ByteFormatter
{
    private static readonly string[] BinarySuffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
    private static readonly string[] DecimalSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    /// <summary>
    /// Formats a byte count to a human-readable string (e.g., 1.54 MB or 1.54 MiB).
    /// </summary>
    /// <param name="bytes">The total bytes.</param>
    /// <param name="decimalPlaces">Number of decimal places.</param>
    /// <param name="useBinaryPrefixes">If true, uses base 1024 (KiB, MiB). If false, uses base 1000 (KB, MB).</param>
    /// <param name="culture">Culture to use for formatting.</param>
    public static string ToShortByteString(
        this long bytes,
        int decimalPlaces = 2,
        bool useBinaryPrefixes = false,
        CultureInfo? culture = null)
    {
        return FormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
    }

    /// <summary>
    /// Generic version for any numeric type.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="bytes">The total bytes.</param>
    /// <param name="decimalPlaces">Number of decimal places.</param>
    /// <param name="useBinaryPrefixes">If true, uses base 1024 (KiB, MiB). If false, uses base 1000 (KB, MB).</param>
    /// <param name="culture">Culture to use for formatting.</param>
    /// <returns>A formatted byte string.</returns>
    public static string ToShortByteString<T>(
        this T bytes,
        int decimalPlaces = 2,
        bool useBinaryPrefixes = false,
        CultureInfo? culture = null) where T : INumber<T>
    {
        var longValue = Convert.ToInt64(bytes);
        return FormatBytes(longValue, decimalPlaces, useBinaryPrefixes, culture);
    }

    private static string FormatBytes(long bytes, int decimalPlaces, bool useBinaryPrefixes, CultureInfo? culture)
    {
        culture ??= CultureInfo.CurrentCulture;
        long absBytes = Math.Abs(bytes);

        double divisor = useBinaryPrefixes ? 1024.0 : 1000.0;
        string[] suffixes = useBinaryPrefixes ? BinarySuffixes : DecimalSuffixes;

        if (absBytes == 0)
            return $"0 {suffixes[0]}";

        int place = Convert.ToInt32(Math.Floor(Math.Log(absBytes, divisor)));
        
        // Ensure place is within array bounds
        place = Math.Clamp(place, 0, suffixes.Length - 1);

        double num = absBytes / Math.Pow(divisor, place);
        num = Math.Round(num, decimalPlaces, MidpointRounding.AwayFromZero);

        // Edge case where rounding pushes it up to the next category (e.g. 999.9 MB -> 1.0 GB)
        if (num >= divisor && place < suffixes.Length - 1)
        {
            num /= divisor;
            place++;
        }

        string sign = bytes < 0 ? culture.NumberFormat.NegativeSign : "";
        string formatString = $"F{decimalPlaces}";
        
        return $"{sign}{num.ToString(formatString, culture)} {suffixes[place]}";
    }
}
