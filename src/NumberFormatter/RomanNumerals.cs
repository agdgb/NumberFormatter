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
        /// <param name="number">The integer to convert.</param>
        /// <returns>A string representing the Roman numeral.</returns>
        public static string ToRoman(this int number)
        {
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
