using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace HumanNumbers.Financial
{
    /// <summary>
    /// Extension methods for converting numbers to their spelled-out word representation.
    /// Very useful for check-writing, invoices, and financial documents.
    /// </summary>
    public static class WordsFormatter
    {
        /// <summary>
        /// Attempts to convert a number to its spelled-out words (e.g., 1234.56 -> "One Thousand...").
        /// Optionally includes check-style currency formats (e.g., "... Dollars and 56/100").
        /// Only formats up to 2 decimal places in the fractional part (common in finance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToHumanWords(
            this decimal value,
            out string result,
            string? majorCurrency = null,
            string? majorCurrencySingular = null,
            IWordsProvider? provider = null)
        {
            try
            {
                provider ??= EnglishWordsProvider.Instance;

                var isNegative = value < 0;
                var absValue = Math.Abs(value);
                
                // Round safely so 1.996m becomes 2.00m rather than 1 and 100/100.
                var roundedValue = Math.Round(absValue, 2, MidpointRounding.AwayFromZero);
                
                var integralPart = Math.Truncate(roundedValue);
                var fractionalPart = (int)((roundedValue - integralPart) * 100m);

                var words = provider.ToWords(integralPart);

                if (!string.IsNullOrEmpty(majorCurrency))
                {
                    majorCurrencySingular ??= majorCurrency;
                    var currencyLabel = integralPart == 1m ? majorCurrencySingular : majorCurrency;
                    words = $"{words} {currencyLabel} {provider.ConjunctionWord} {fractionalPart:00}/100";
                }
                else if (fractionalPart > 0)
                {
                    words = $"{words} {provider.ConjunctionWord} {fractionalPart:00}/100";
                }

                result = isNegative ? $"{provider.NegativeWord} {words}" : words;
                return true;
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString(CultureInfo.InvariantCulture);
                return false;
            }
        }

        /// <summary>
        /// Converts a number to its spelled-out words (e.g., 1234.56 -> "One Thousand...").
        /// Optionally includes check-style currency formats (e.g., "... Dollars and 56/100").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHumanWords(
            this decimal value,
            string? majorCurrency = null,
            string? majorCurrencySingular = null,
            IWordsProvider? provider = null)
        {
            if (TryToHumanWords(value, out var result, majorCurrency, majorCurrencySingular, provider)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format words for value {value}");
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

