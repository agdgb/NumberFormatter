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
        /// Converts a decimal to a Basis Points string (e.g., 0.0125m becomes "125 bps").
        /// </summary>
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
            if (decimal.TryParse(span.ToString(), style, provider ?? CultureInfo.InvariantCulture, out var parsedBps))
            {
                result = parsedBps / BpsMultiplier;
                return true;
            }

            return false;
        }
    }
}

namespace NumberFormatter.Financial
{
    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Financial.BasisPointFormatter"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.Financial.BasisPointFormatter instead. This alias will be removed in a future version.")]
    public static class BasisPointFormatter
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Financial.BasisPointFormatter.ToBps"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Financial.BasisPointFormatter.ToBps instead.")]
        public static decimal ToBps(this decimal value) => HumanNumbers.Financial.BasisPointFormatter.ToBps(value);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Financial.BasisPointFormatter.ToBpsString"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Financial.BasisPointFormatter.ToBpsString instead.")]
        public static string ToBpsString(this decimal value, int decimals = 0, MidpointRounding rounding = MidpointRounding.AwayFromZero, IFormatProvider? provider = null) 
            => HumanNumbers.Financial.BasisPointFormatter.ToBpsString(value, decimals, rounding, provider);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Financial.BasisPointFormatter.TryParseBps"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Financial.BasisPointFormatter.TryParseBps instead.")]
        public static bool TryParseBps(string? input, out decimal result, IFormatProvider? provider = null)
            => HumanNumbers.Financial.BasisPointFormatter.TryParseBps(input, out result, provider);
    }
}
