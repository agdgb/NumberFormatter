using System;
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
        /// Converts a number to its spelled-out words (e.g., 1234.56 -> "One Thousand Two Hundred Thirty-Four and 56/100").
        /// Only formats up to 2 decimal places in the fractional part (common in finance).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToWords(
            this decimal value, 
            IWordsProvider? provider = null)
        {
            provider ??= EnglishWordsProvider.Instance;

            var isNegative = value < 0;
            var absValue = Math.Abs(value);
            
            // Round safely so 1.996m becomes 2.00m rather than 1 and 100/100.
            var roundedValue = Math.Round(absValue, 2, MidpointRounding.AwayFromZero);
            
            var integralPart = Math.Truncate(roundedValue);
            var fractionalPart = (int)((roundedValue - integralPart) * 100m);

            var words = provider.ToWords(integralPart);

            if (fractionalPart > 0)
            {
                words = $"{words} {provider.ConjunctionWord} {fractionalPart:00}/100";
            }

            return isNegative ? $"{provider.NegativeWord} {words}" : words;
        }

        /// <summary>
        /// Converts a number to its spelled-out words, explicitly formatted for checks with major currency labels.
        /// Example: 1234.56 -> "One Thousand Two Hundred Thirty-Four Dollars and 56/100"
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToCheckWords(
            this decimal value, 
            string majorCurrency = "Dollars", 
            string majorCurrencySingular = "Dollar",
            IWordsProvider? provider = null)
        {
            provider ??= EnglishWordsProvider.Instance;

            var isNegative = value < 0;
            var absValue = Math.Abs(value);
            
            var roundedValue = Math.Round(absValue, 2, MidpointRounding.AwayFromZero);
            var integralPart = Math.Truncate(roundedValue);
            var fractionalPart = (int)((roundedValue - integralPart) * 100m);

            var words = provider.ToWords(integralPart);
            
            // Grammatical rule: if integral part is exactly 1, use singular currency.
            var currencyLabel = integralPart == 1m ? majorCurrencySingular : majorCurrency;

            words = $"{words} {currencyLabel} {provider.ConjunctionWord} {fractionalPart:00}/100";

            return isNegative ? $"{provider.NegativeWord} {words}" : words;
        }
    }
}

namespace NumberFormatter.Financial
{
    using System;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Financial.WordsFormatter"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.Financial.WordsFormatter instead. This alias will be removed in a future version.")]
    public static class WordsFormatter
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Financial.WordsFormatter.ToWords"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Financial.WordsFormatter.ToWords instead.")]
        public static string ToWords(this decimal value, IWordsProvider? provider = null) 
            => HumanNumbers.Financial.WordsFormatter.ToWords(value, provider);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.Financial.WordsFormatter.ToCheckWords"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.Financial.WordsFormatter.ToCheckWords instead.")]
        public static string ToCheckWords(this decimal value, string majorCurrency = "Dollars", string majorCurrencySingular = "Dollar", IWordsProvider? provider = null) 
            => HumanNumbers.Financial.WordsFormatter.ToCheckWords(value, majorCurrency, majorCurrencySingular, provider);
    }
}
