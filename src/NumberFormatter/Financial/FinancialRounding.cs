using System;

namespace NumberFormatter.Financial;

/// <summary>
/// Provides advanced formatting and rounding for financial markets, including fractional ticks.
/// </summary>
public static class FinancialRounding
{
    /// <summary>
    /// Rounds a number to the nearest provided tick size (e.g. 0.05).
    /// Example: 10.22m with tick size 0.05m and mode Nearest becomes 10.20m.
    /// Handles negatives safely (e.g. Rounding -10.22m down with tick 0.05m -> -10.25m).
    /// </summary>
    public static decimal RoundToTick(this decimal value, decimal tickSize, TickRoundingMode mode = TickRoundingMode.Nearest)
    {
        if (tickSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(tickSize), "Tick size must be greater than zero.");

        var division = value / tickSize;
        
        decimal roundedDivision = mode switch
        {
            TickRoundingMode.Up => Math.Ceiling(division),
            TickRoundingMode.Down => Math.Floor(division),
            // Default to nearest, MidpointRounding.ToEven
            _ => Math.Round(division, 0, MidpointRounding.ToEven) 
        };

        return roundedDivision * tickSize;
    }

    /// <summary>
    /// Converts a decimal to a fractional string used in Treasury/Bond markets (e.g. 10.03125 -> "10 1/32").
    /// </summary>
    public static string ToFractionString(this decimal value, int denominator, MidpointRounding rounding = MidpointRounding.ToEven)
    {
        if (denominator <= 0)
            throw new ArgumentOutOfRangeException(nameof(denominator), "Denominator must be greater than zero.");

        var isNegative = value < 0;
        var absValue = Math.Abs(value);

        var integral = Math.Truncate(absValue);
        var decimalPart = absValue - integral;

        // Convert the decimal part to the closest fraction of the denominator
        var fractionNumerator = Math.Round(decimalPart * denominator, 0, rounding);

        // If the rounding pushes the fraction numerator to equal the denominator (e.g. 32/32)
        if (fractionNumerator >= denominator)
        {
            integral += 1;
            fractionNumerator = 0;
        }

        var sign = isNegative ? "-" : "";

        if (fractionNumerator == 0)
        {
            return $"{sign}{integral}";
        }

        return $"{sign}{integral} {fractionNumerator}/{denominator}";
    }

    /// <summary>
    /// Parses a fractional string (e.g. "10 1/32", "-10 1/32") into a decimal (10.03125m).
    /// </summary>
    public static bool TryParseFraction(string? input, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var span = input.AsSpan().Trim();
        var isNegative = false;

        if (span.StartsWith("-"))
        {
            isNegative = true;
            span = span.Slice(1).TrimStart();
        }

        // Find space separating integral from fraction
        var spaceIndex = span.IndexOf(' ');
        var slashIndex = span.IndexOf('/');

        if (slashIndex == -1)
        {
            // No fraction part, just a regular number
            if (decimal.TryParse(span, out var num))
            {
                result = isNegative ? -num : num;
                return true;
            }
            return false;
        }

        // Parse fraction parts
        ReadOnlySpan<char> integralSpan = "0";
        ReadOnlySpan<char> numeratorSpan;
        ReadOnlySpan<char> denominatorSpan;

        if (spaceIndex != -1 && spaceIndex < slashIndex)
        {
            integralSpan = span.Slice(0, spaceIndex);
            numeratorSpan = span.Slice(spaceIndex + 1, slashIndex - spaceIndex - 1);
            denominatorSpan = span.Slice(slashIndex + 1);
        }
        else
        {
            // Just a fraction, e.g. "1/32"
            numeratorSpan = span.Slice(0, slashIndex);
            denominatorSpan = span.Slice(slashIndex + 1);
        }

        if (decimal.TryParse(integralSpan, out var integral) &&
            decimal.TryParse(numeratorSpan, out var numPart) &&
            decimal.TryParse(denominatorSpan, out var denPart) &&
            denPart != 0)
        {
            var value = integral + (numPart / denPart);
            result = isNegative ? -value : value;
            return true;
        }

        return false;
    }
}
