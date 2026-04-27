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
            bool useBinaryPrefixes = true,
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
            bool useBinaryPrefixes = true,
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
            bool useBinaryPrefixes = true,
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
            bool useBinaryPrefixes = true,
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
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static bool TryToHumanBytes<T>(
            this T bytes,
            out string result,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (bytes is long l) return TryToHumanBytes(l, out result, decimalPlaces, useBinaryPrefixes, culture);
            if (bytes is ulong ul) return TryToHumanBytes(ul, out result, decimalPlaces, useBinaryPrefixes, culture);
            if (bytes is int i) return TryToHumanBytes((long)i, out result, decimalPlaces, useBinaryPrefixes, culture);

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
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanBytes<T>(
            this T bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (TryToHumanBytes(bytes, out var result, decimalPlaces, useBinaryPrefixes, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format byte size {bytes}");
            return bytes.ToString() ?? string.Empty;
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Formats a byte count into a character span.
        /// </summary>
        public static bool ToHumanBytes(
            this long bytes,
            Span<char> destination,
            out int charsWritten,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, destination, out charsWritten);
        }

        /// <summary>
        /// Attempts to format a byte count into a character span.
        /// </summary>
        public static bool TryFormat(
            this long bytes,
            Span<char> destination,
            out int charsWritten,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, destination, out charsWritten);
        }

        /// <summary>
        /// Formats a byte count into a character span.
        /// </summary>
        public static bool ToHumanBytes(
            this ulong bytes,
            Span<char> destination,
            out int charsWritten,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, destination, out charsWritten);
        }

        /// <summary>
        /// Attempts to format a byte count into a character span.
        /// </summary>
        public static bool TryFormat(
            this ulong bytes,
            Span<char> destination,
            out int charsWritten,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = true,
            CultureInfo? culture = null)
        {
            return TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, destination, out charsWritten);
        }
#endif

        internal static bool TryFormatBytes(double bytes, int decimalPlaces, bool useBinaryPrefixes, CultureInfo? culture, out string result)
        {
#if !NETSTANDARD2_0
            Span<char> buffer = stackalloc char[64];
            if (TryFormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture, buffer, out var charsWritten))
            {
                result = new string(buffer.Slice(0, charsWritten));
                return true;
            }
            result = bytes.ToString(CultureInfo.InvariantCulture);
            return false;
#else
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

                int place = 0;
                double num = absBytes;

                if (absBytes >= divisor)
                {
                    while (num >= divisor && place < suffixes.Length - 1)
                    {
                        num /= divisor;
                        place++;
                    }
                }

                num = Math.Round(num, decimalPlaces, MidpointRounding.AwayFromZero);

                // Edge case where rounding pushes it up to the next category (e.g. 999.9 MB -> 1.0 GB)
                if (num >= divisor && place < suffixes.Length - 1)
                {
                    num /= divisor;
                    place++;
                }

                string sign = bytes < 0 ? culture.NumberFormat.NegativeSign : "";

                // Suppress decimals if it's the base unit (B) and is a whole number
                int precision = (place == 0 && num == Math.Truncate(num)) ? 0 : decimalPlaces;
                string formatString = $"F{precision}";

                result = $"{sign}{num.ToString(formatString, culture)} {suffixes[place]}";
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = bytes.ToString(CultureInfo.InvariantCulture);
                return false;
            }
#endif
        }

#if !NETSTANDARD2_0
        internal static bool TryFormatBytes(double bytes, int decimalPlaces, bool useBinaryPrefixes, CultureInfo? culture, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            try
            {
                culture ??= CultureInfo.CurrentCulture;
                double absBytes = Math.Abs(bytes);
                double divisor = useBinaryPrefixes ? 1024.0 : 1000.0;
                var suffixes = useBinaryPrefixes ? BinarySuffixes : DecimalSuffixes;

                if (absBytes == 0)
                {
                    if (destination.Length < 3) return false;
                    destination[0] = '0';
                    destination[1] = ' ';
                    destination[2] = 'B';
                    charsWritten = 3;
                    return true;
                }

                int place = 0;
                double num = absBytes;

                if (absBytes >= divisor)
                {
                    while (num >= divisor && place < suffixes.Length - 1)
                    {
                        num /= divisor;
                        place++;
                    }
                }

                num = Math.Round(num, decimalPlaces, MidpointRounding.AwayFromZero);

                if (num >= divisor && place < suffixes.Length - 1)
                {
                    num /= divisor;
                    place++;
                }

                int pos = 0;
                if (bytes < 0)
                {
                    var negSign = culture.NumberFormat.NegativeSign.AsSpan();
                    if (pos + negSign.Length > destination.Length) return false;
                    negSign.CopyTo(destination.Slice(pos));
                    pos += negSign.Length;
                }

                int precision = (place == 0 && num == Math.Truncate(num)) ? 0 : decimalPlaces;
                Span<char> format = stackalloc char[8];
                "F".AsSpan().CopyTo(format);
                precision.TryFormat(format.Slice(1), out var fLen);

                if (!num.TryFormat(destination.Slice(pos), out var numWritten, format.Slice(0, 1 + fLen), culture))
                    return false;
                pos += numWritten;

                if (pos + 1 > destination.Length) return false;
                destination[pos++] = ' ';

                var suffix = suffixes[place].AsSpan();
                if (pos + suffix.Length > destination.Length) return false;
                suffix.CopyTo(destination.Slice(pos));
                pos += suffix.Length;

                charsWritten = pos;
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                return bytes.TryFormat(destination, out charsWritten, default, culture);
            }
        }
#endif
    }
}
