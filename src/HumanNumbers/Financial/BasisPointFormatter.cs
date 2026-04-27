using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace HumanNumbers.Financial
{
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal ToBps(this decimal value)
        {
            return value * BpsMultiplier;
        }

        /// <summary>
        /// Attempts to format a decimal to a Basis Points string (e.g., 0.0125m becomes "125 bps").
        /// </summary>
        public static bool TryToHumanBps(
            this decimal value,
            out string result,
            int decimals = 0, 
            MidpointRounding rounding = MidpointRounding.AwayFromZero,
            IFormatProvider? provider = null)
        {
#if !NETSTANDARD2_0
            Span<char> buffer = stackalloc char[64];
            if (TryFormatBps(value, buffer, out var charsWritten, decimals, rounding, provider))
            {
                result = new string(buffer.Slice(0, charsWritten));
                return true;
            }
            result = value.ToString(provider ?? CultureInfo.InvariantCulture);
            return false;
#else
            try
            {
                var bps = value * BpsMultiplier;
                var roundedBps = Math.Round(bps, decimals, rounding);
                result = $"{roundedBps.ToString($"F{decimals}", provider)} bps";
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString(provider ?? CultureInfo.InvariantCulture);
                return false;
            }
#endif
        }

        /// <summary>
        /// Converts a decimal to a Basis Points string (e.g., 0.0125m becomes "125 bps").
        /// </summary>
        public static string ToHumanBps(
            this decimal value, 
            int decimals = 0, 
            MidpointRounding rounding = MidpointRounding.AwayFromZero,
            IFormatProvider? provider = null)
        {
            if (TryToHumanBps(value, out var result, decimals, rounding, provider)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format Basis Points for value {value}");
            return value.ToString(provider ?? CultureInfo.InvariantCulture);
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Attempts to format a decimal to a Basis Points span.
        /// </summary>
        public static bool TryFormatBps(
            this decimal value,
            Span<char> destination,
            out int charsWritten,
            int decimals = 0,
            MidpointRounding rounding = MidpointRounding.AwayFromZero,
            IFormatProvider? provider = null)
        {
            charsWritten = 0;
            try
            {
                var bps = value * BpsMultiplier;
                var roundedBps = Math.Round(bps, decimals, rounding);

                Span<char> format = stackalloc char[8];
                "F".AsSpan().CopyTo(format);
                decimals.TryFormat(format.Slice(1), out var fLen);

                if (!roundedBps.TryFormat(destination, out var numWritten, format.Slice(0, 1 + fLen), provider))
                    return false;
                
                int pos = numWritten;
                var suffix = " bps".AsSpan();
                if (pos + suffix.Length > destination.Length) return false;
                suffix.CopyTo(destination.Slice(pos));
                charsWritten = pos + suffix.Length;
                return true;
            }
            catch
            {
                return false;
            }
        }
#endif

        /// <summary>
        /// Parses a Basis Points string back into a decimal.
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
#if !NETSTANDARD2_0
            if (decimal.TryParse(span, style, provider ?? CultureInfo.InvariantCulture, out var parsedBps))
#else
            if (decimal.TryParse(span.ToString(), style, provider ?? CultureInfo.InvariantCulture, out var parsedBps))
#endif
            {
                result = parsedBps / BpsMultiplier;
                return true;
            }

            return false;
        }
    }
}
