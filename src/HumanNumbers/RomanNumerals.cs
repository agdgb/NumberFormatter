using System;
using System.Text;
using System.Runtime.CompilerServices;

namespace HumanNumbers.Roman
{
    /// <summary>
    /// Provides extension methods for formatting numbers into Roman Numerals.
    /// </summary>
    public static class RomanNumerals
    {
        private static readonly (int Value, string Symbol)[] Symbols = new[]
        {
            (1000, "M"),
            (900, "CM"),
            (500, "D"),
            (400, "CD"),
            (100, "C"),
            (90, "XC"),
            (50, "L"),
            (40, "XL"),
            (10, "X"),
            (9, "IX"),
            (5, "V"),
            (4, "IV"),
            (1, "I")
        };

        /// <summary>
        /// Converts an integer between 1 and 3999 to a Roman numeral.
        /// </summary>
        public static string ToRoman(this int number)
        {
#if !NETSTANDARD2_0
            Span<char> buffer = stackalloc char[16]; // Max roman length for 3999 is 15 (MMMCMXCIX)
            if (TryFormat(number, buffer, out var charsWritten))
            {
                return new string(buffer.Slice(0, charsWritten));
            }
            throw new ArgumentOutOfRangeException(nameof(number), "Value must be between 1 and 3999.");
#else
            if (number < 1 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "Value must be between 1 and 3999.");

            var sb = new StringBuilder();
            foreach (var (value, symbol) in Symbols)
            {
                while (number >= value)
                {
                    sb.Append(symbol);
                    number -= value;
                }
            }
            return sb.ToString();
#endif
        }

        /// <summary>
        /// Attempts to format an integer as a Roman numeral into a span.
        /// </summary>
        public static bool TryFormat(this int number, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (number < 1 || number > 3999) return false;

            var pos = 0;
            foreach (var (value, symbol) in Symbols)
            {
                while (number >= value)
                {
                    if (pos + symbol.Length > destination.Length) return false;
                    symbol.AsSpan().CopyTo(destination.Slice(pos));
                    pos += symbol.Length;
                    number -= value;
                }
            }

            charsWritten = pos;
            return true;
        }

        /// <summary>
        /// Obsolete alias for <see cref="ToRoman(int)"/>.
        /// </summary>
        [Obsolete("Use ToRoman instead. This alias will be removed in a future version.")]
        public static string ToRomanNumeral(this int number) => ToRoman(number);
    }
}

namespace NumberFormatter.Roman
{
    using System;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Roman.RomanNumerals"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.Roman.RomanNumerals instead. This alias will be removed in a future version.")]
    public static class RomanNumerals
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Roman.RomanNumerals.ToRoman"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Roman.RomanNumerals.ToRoman instead.")]
        public static string ToRomanNumeral(this int number) 
            => HumanNumbers.Roman.RomanNumerals.ToRoman(number);
    }
}
