using System;
using System.Runtime.CompilerServices;

namespace HumanNumbers.Financial
{
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                // Note: In .NET Standard 2.0 Math.Round(decimal) defaults to ToEven.
            };

            return roundedDivision * tickSize;
        }

        /// <summary>
        /// Attempts to convert a decimal to a fractional string used in Treasury/Bond markets (e.g. 10.03125 -> "10 1/32").
        /// </summary>
        public static bool TryToHumanFraction(
            this decimal value, 
            out string result,
            int denominator, 
            MidpointRounding rounding = MidpointRounding.ToEven)
        {
#if !NETSTANDARD2_0
            Span<char> buffer = stackalloc char[128];
            if (TryFormatFraction(value, buffer, out var charsWritten, denominator, rounding))
            {
                result = new string(buffer.Slice(0, charsWritten));
                return true;
            }
            result = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return false;
#else
            try
            {
                if (denominator <= 0)
                {
                    result = string.Empty;
                    return false;
                }

                var isNegative = value < 0;
                var absValue = Math.Abs(value);

                var integral = Math.Truncate(absValue);
                var decimalPart = absValue - integral;

                var fractionNumerator = Math.Round(decimalPart * denominator, 0, rounding);

                if (fractionNumerator >= denominator)
                {
                    integral += 1;
                    fractionNumerator = 0;
                }

                var sign = isNegative ? "-" : "";

                if (fractionNumerator == 0)
                {
                    result = $"{sign}{integral}";
                }
                else
                {
                    result = $"{sign}{integral} {fractionNumerator}/{denominator}";
                }
                
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return false;
            }
#endif
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Attempts to format a decimal to a fractional span.
        /// </summary>
        public static bool TryFormatFraction(
            this decimal value,
            Span<char> destination,
            out int charsWritten,
            int denominator,
            MidpointRounding rounding = MidpointRounding.ToEven)
        {
            charsWritten = 0;
            if (denominator <= 0) return false;

            var isNegative = value < 0;
            var absValue = Math.Abs(value);
            var integral = Math.Truncate(absValue);
            var decimalPart = absValue - integral;

            var fractionNumerator = Math.Round(decimalPart * denominator, 0, rounding);
            if (fractionNumerator >= denominator)
            {
                integral += 1;
                fractionNumerator = 0;
            }

            int pos = 0;
            if (isNegative)
            {
                if (pos >= destination.Length) return false;
                destination[pos++] = '-';
            }

            if (!integral.TryFormat(destination.Slice(pos), out var intWritten, default, System.Globalization.CultureInfo.InvariantCulture))
                return false;
            pos += intWritten;

            if (fractionNumerator != 0)
            {
                if (pos + 1 > destination.Length) return false;
                destination[pos++] = ' ';

                if (!fractionNumerator.TryFormat(destination.Slice(pos), out var numWritten, default, System.Globalization.CultureInfo.InvariantCulture))
                    return false;
                pos += numWritten;

                if (pos + 1 > destination.Length) return false;
                destination[pos++] = '/';

                if (!((decimal)denominator).TryFormat(destination.Slice(pos), out var denWritten, default, System.Globalization.CultureInfo.InvariantCulture))
                    return false;
                pos += denWritten;
            }

            charsWritten = pos;
            return true;
        }
#endif

        /// <summary>
        /// Converts a decimal to a fractional string used in Treasury/Bond markets (e.g. 10.03125 -> "10 1/32").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanFraction(this decimal value, int denominator, MidpointRounding rounding = MidpointRounding.ToEven)
        {
            if (TryToHumanFraction(value, out var result, denominator, rounding)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format fraction for value {value}");
            return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
#if !NETSTANDARD2_0
                if (decimal.TryParse(span, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
#else
                if (decimal.TryParse(span.ToString(), out var num))
#endif
                {
                    result = isNegative ? -num : num;
                    return true;
                }
                return false;
            }

            // Parse fraction parts
            ReadOnlySpan<char> integralSpan = "0".AsSpan();
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
                numeratorSpan = span.Slice(0, slashIndex);
                denominatorSpan = span.Slice(slashIndex + 1);
            }

#if !NETSTANDARD2_0
            if (decimal.TryParse(integralSpan, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var integral) &&
                decimal.TryParse(numeratorSpan, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var numPart) &&
                decimal.TryParse(denominatorSpan, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var denPart) &&
                denPart != 0)
#else
            if (decimal.TryParse(integralSpan.ToString(), out var integral) &&
                decimal.TryParse(numeratorSpan.ToString(), out var numPart) &&
                decimal.TryParse(denominatorSpan.ToString(), out var denPart) &&
                denPart != 0)
#endif
            {
                var valuePart = integral + (numPart / denPart);
                result = isNegative ? -valuePart : valuePart;
                return true;
            }

            return false;
        }
    }
}
