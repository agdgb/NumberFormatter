using System;
using System.Text;

namespace HumanNumbers;

/// <summary>
/// Provides shared internal utilities for numeric type validation and chunk formatting.
/// </summary>
public static class NumberUtils
{
    private static readonly string[] Units = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
    private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

    /// <summary>
    /// Checks if a given type is a supported numeric type (or nullable equivalent).
    /// </summary>
    public static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float) || underlyingType == typeof(int) ||
               underlyingType == typeof(long) || underlyingType == typeof(short) ||
               underlyingType == typeof(byte) || underlyingType == typeof(uint) ||
               underlyingType == typeof(ulong) || underlyingType == typeof(ushort) ||
               underlyingType == typeof(sbyte);
    }

    /// <summary>
    /// Converts a three-digit integer chunk into its English word representation.
    /// </summary>
    public static string ConvertChunk(int value)
    {
        var sb = new StringBuilder();
        AppendChunk(sb, value);
        return sb.ToString();
    }

    /// <summary>
    /// Appends the English word representation of a three-digit integer chunk to a StringBuilder.
    /// </summary>
    public static void AppendChunk(StringBuilder sb, int value)
    {
        if (value >= 100)
        {
            sb.Append(Units[value / 100]).Append(" Hundred");
            value %= 100;
            if (value > 0) sb.Append(' ');
        }

        if (value >= 20)
        {
            sb.Append(Tens[value / 10]);
            value %= 10;
            if (value > 0) sb.Append('-').Append(Units[value]);
        }
        else if (value > 0)
        {
            sb.Append(Units[value]);
        }
    }
}
