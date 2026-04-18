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
        /// Formats a byte count to a human-readable string (e.g., 1.54 MB or 1.54 MiB).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanBytes(
            this long bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null)
        {
            return FormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
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
            return FormatBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
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
            try
            {
                var doubleValue = Convert.ToDouble(bytes);
                return FormatBytes(doubleValue, decimalPlaces, useBinaryPrefixes, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                return bytes.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Obsolete alias for <see cref="ToHumanBytes(long, int, bool, CultureInfo?)"/>.
        /// </summary>
        [Obsolete("Use ToHumanBytes instead. This alias will be removed in a future version.")]
        public static string ToShortByteString(
            this long bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) => ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);

        /// <summary>
        /// Obsolete alias for <see cref="ToHumanBytes(ulong, int, bool, CultureInfo?)"/>.
        /// </summary>
        [Obsolete("Use ToHumanBytes instead. This alias will be removed in a future version.")]
        public static string ToShortByteString(
            this ulong bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) => ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);

        /// <summary>
        /// Obsolete alias for <see cref="ToHumanBytes{T}(T, int, bool, CultureInfo?)"/>.
        /// </summary>
        [Obsolete("Use ToHumanBytes instead. This alias will be removed in a future version.")]
#if NET7_0_OR_GREATER
        public static string ToShortByteString<T>(
            this T bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : INumber<T> => ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
#else
        public static string ToShortByteString<T>(
            this T bytes,
            int decimalPlaces = 2,
            bool useBinaryPrefixes = false,
            CultureInfo? culture = null) where T : struct => ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
#endif

        private static string FormatBytes(double bytes, int decimalPlaces, bool useBinaryPrefixes, CultureInfo? culture)
        {
            culture ??= CultureInfo.CurrentCulture;
            double absBytes = Math.Abs(bytes);

            double divisor = useBinaryPrefixes ? 1024.0 : 1000.0;
            string[] suffixes = useBinaryPrefixes ? BinarySuffixes : DecimalSuffixes;

            if (absBytes == 0)
                return $"0 {suffixes[0]}";

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
            
            return $"{sign}{num.ToString(formatString, culture)} {suffixes[place]}";
        }
    }
}

namespace NumberFormatter.Bytes
{
    using System;
    using System.Globalization;
    using System.Numerics;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Bytes.ByteSizeFormatter"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.Bytes.ByteSizeFormatter instead. This alias will be removed in a future version.")]
    public static class ByteSizeFormatter
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(long, int, bool, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes instead.")]
        public static string ToShortByteString(this long bytes, int decimalPlaces = 2, bool useBinaryPrefixes = false, CultureInfo? culture = null)
            => HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(ulong, int, bool, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes instead.")]
        public static string ToShortByteString(this ulong bytes, int decimalPlaces = 2, bool useBinaryPrefixes = false, CultureInfo? culture = null)
            => HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(long, int, bool, System.Globalization.CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes instead.")]
#if NET7_0_OR_GREATER
        public static string ToShortByteString<T>(this T bytes, int decimalPlaces = 2, bool useBinaryPrefixes = false, CultureInfo? culture = null) where T : INumber<T>
            => HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
#else
        public static string ToShortByteString<T>(this T bytes, int decimalPlaces = 2, bool useBinaryPrefixes = false, CultureInfo? culture = null) where T : struct
            => HumanNumbers.Bytes.ByteSizeFormatter.ToHumanBytes(bytes, decimalPlaces, useBinaryPrefixes, culture);
#endif
    }
}
