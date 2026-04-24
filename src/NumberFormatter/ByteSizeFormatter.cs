using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HumanNumbers.Bytes
{
    /// <summary>
    /// Provides extension methods for formatting byte sizes.
    /// </summary>
    public static class ByteSizeFormatter
    {
        private static readonly string[] BinarySuffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };
        private static readonly string[] DecimalSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        /// <summary>
        /// Attempts to format a byte count to a human-readable string.
        /// </summary>
        public static bool TryToHumanBytes(
            this long bytes,
            out string result,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, out result);
        }

        /// <summary>
        /// Formats a byte count to a human-readable string (e.g., 1.54 MB or 1.54 MiB).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanBytes(
            this long bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null)
        {
            if (TryToHumanBytes(bytes, out var result, decimalPlaces, useBinaryPrefixes, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format byte size {bytes}");
            return bytes.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format a byte count to a human-readable string.
        /// </summary>
        public static bool TryToHumanBytes(
            this ulong bytes,
            out string result,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, out result);
        }

        /// <summary>
        /// Formats a byte count to a human-readable string (e.g., 1.54 MB or 1.54 MiB).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanBytes(
            this ulong bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null)
        {
            if (TryToHumanBytes(bytes, out var result, decimalPlaces, useBinaryPrefixes, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format byte size {bytes}");
            return bytes.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format any numeric type to a human-readable byte size.
        /// </summary>
#if NET7_0_OR_GREATER
        public static bool TryToHumanBytes<T>(
            this T bytes,
            out string result,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static bool TryToHumanBytes<T>(
            this T bytes,
            out string result,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var doubleValue = Convert.ToDouble(bytes);
                return TryFormatBytes(doubleValue, decimalPlaces, useBinaryPrefixes, culture, out result);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = bytes.ToString() ?? string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Generic version for any numeric type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET7_0_OR_GREATER
        public static string ToHumanBytes<T>(
            this T bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanBytes<T>(
            this T bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (TryToHumanBytes(bytes, out var result, decimalPlaces, useBinaryPrefixes, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format byte size {bytes}");
            return bytes.ToString() ?? string.Empty;
        }

        internal static bool TryFormatBytes(double bytes, int decimalPlaces, bool useBinaryPrefixes, CultureInfo? culture, out string result)
        {
            try
            {
                culture ??= CultureInfo.CurrentCulture;
                double absBytes = Math.Abs(bytes);

                double divisor = useBinaryPrefixes ? 1024.0 : 1000.0;
                string[] suffixes = useBinaryPrefixes ? BinarySuffixes : DecimalSuffixes;

                if (absBytes == 0)
                {
                    result = $"0 {suffixes[0]}";
                    return true;
                }

                int place = Convert.ToInt32(Math.Floor(Math.Log(absBytes, divisor)));
                
                // Ensure place is within array bounds (Math.Clamp is not available in netstandard2.0)
                place = Math.Max(0, Math.Min(place, suffixes.Length - 1));

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
                
                result = $"{sign}{num.ToString(formatString, culture)} {suffixes[place]}";
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = bytes.ToString(CultureInfo.InvariantCulture);
                return false;
            }
        }
    }
}

