using System;
using System.Text;

namespace NumberFormatter;

/// <summary>
/// Provides extension methods for formatting numbers into Roman Numerals.
/// </summary>
public static class RomanNumeralFormatter
{
    private static readonly (int Value, string Symbol)[] RomanNumerals = new[]
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
    public static string ToRomanNumeral(this int number)
    {
        if (number < 1 || number > 3999)
            throw new ArgumentOutOfRangeException(nameof(number), "Value must be between 1 and 3999.");

        var sb = new StringBuilder();
        foreach (var (value, symbol) in RomanNumerals)
        {
            while (number >= value)
            {
                sb.Append(symbol);
                number -= value;
            }
        }
        return sb.ToString();
    }
}
