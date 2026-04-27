using System;
using System.Collections.Generic;
using System.Text;

namespace HumanNumbers.Financial
{
    /// <summary>
    /// Provides English language word conversion for numbers up to huge limits (~Octillions) using efficient allocation.
    /// Does not natively pluralize the word "Thousand" (e.g. 2000 -> "Two Thousand", not "Two Thousands").
    /// </summary>
    public sealed class EnglishWordsProvider : IWordsProvider
    {
        /// <summary>
        /// Singleton instance of the provider.
        /// </summary>
        public static readonly EnglishWordsProvider Instance = new();

        /// <inheritdoc />
        public string NegativeWord => "Negative";
        
        /// <inheritdoc />
        public string ConjunctionWord => "and";
        
        private static readonly string[] Units = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        
        // decimal.MaxValue is ~79 Octillion. This array covers everything.
        private static readonly string[] ThousandsGroups = { "", "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion", "Sextillion", "Septillion", "Octillion", "Nonillion", "Decillion" };

        /// <inheritdoc />
        public string ToWords(decimal value)
        {
            decimal num = Math.Truncate(Math.Abs(value));
            if (num == 0) return Units[0];

#if !NETSTANDARD2_0
            // Optimized path for modern .NET
            return ToWordsOptimized(num);
#else
            var parts = new Stack<string>();
            int groupIndex = 0;

            while (num > 0)
            {
                decimal chunk = num % 1000m;
                if (chunk > 0)
                {
                    var chunkWords = ConvertChunk((int)chunk);
                    if (groupIndex < ThousandsGroups.Length && !string.IsNullOrEmpty(ThousandsGroups[groupIndex]))
                    {
                        chunkWords += " " + ThousandsGroups[groupIndex];
                    }
                    parts.Push(chunkWords);
                }
                num = Math.Truncate(num / 1000m);
                groupIndex++;
            }

            var sb = new StringBuilder();
            while (parts.Count > 0)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(parts.Pop());
            }

            return sb.ToString();
#endif
        }

#if !NETSTANDARD2_0
        private string ToWordsOptimized(decimal num)
        {
            // Calculate how many groups we have
            Span<int> groups = stackalloc int[12]; // Up to Decillion (12 groups of 1000)
            int groupCount = 0;
            while (num > 0)
            {
                groups[groupCount++] = (int)(num % 1000m);
                num = Math.Truncate(num / 1000m);
            }

            var sb = new StringBuilder();
            for (int i = groupCount - 1; i >= 0; i--)
            {
                int chunk = groups[i];
                if (chunk == 0) continue;

                if (sb.Length > 0) sb.Append(' ');

                // Convert chunk
                if (chunk >= 100)
                {
                    sb.Append(Units[chunk / 100]).Append(" Hundred");
                    chunk %= 100;
                    if (chunk > 0) sb.Append(' ');
                }

                if (chunk >= 20)
                {
                    sb.Append(Tens[chunk / 10]);
                    chunk %= 10;
                    if (chunk > 0) sb.Append('-').Append(Units[chunk]);
                }
                else if (chunk > 0)
                {
                    sb.Append(Units[chunk]);
                }

                if (i < ThousandsGroups.Length && !string.IsNullOrEmpty(ThousandsGroups[i]))
                {
                    sb.Append(' ').Append(ThousandsGroups[i]);
                }
            }

            return sb.ToString();
        }
#endif

        private static string ConvertChunk(int value)
        {
            var sb = new StringBuilder();
            
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

            return sb.ToString();
        }
    }
}

namespace NumberFormatter.Financial
{
    using System;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Financial.EnglishWordsProvider"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.Financial.EnglishWordsProvider instead. This alias will be removed in a future version.")]
    public sealed class EnglishWordsProvider : IWordsProvider
    {
        /// <summary>Singleton instance.</summary>
        public static readonly EnglishWordsProvider Instance = new();
        /// <inheritdoc/>
        public string NegativeWord => HumanNumbers.Financial.EnglishWordsProvider.Instance.NegativeWord;
        /// <inheritdoc/>
        public string ConjunctionWord => HumanNumbers.Financial.EnglishWordsProvider.Instance.ConjunctionWord;
        /// <inheritdoc/>
        public string ToWords(decimal value) => HumanNumbers.Financial.EnglishWordsProvider.Instance.ToWords(value);
    }
}
