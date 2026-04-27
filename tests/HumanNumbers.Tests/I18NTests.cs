using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using HumanNumbers;
using HumanNumbers.Formatting;

namespace HumanNumbers.Tests
{
    public class I18NTests
    {
        private readonly ITestOutputHelper _output;
        private const int DeterministicSeed = 42;

        public I18NTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("ar-SA")]
        [InlineData("hi-IN")]
        [InlineData("fr-FR")]
        [InlineData("de-DE")]
        [InlineData("am-ET")]
        public void Verify_Extreme_Edge_Case_Cultures(string cultureName)
        {
            var culture = new CultureInfo(cultureName);
            ValidateAndLogCulture(culture);
        }

        [Fact]
        public void Verify_Arabic_Numerals_Handling()
        {
            var culture = CultureInfo.GetCultureInfo("ar-SA");
            var value = 1500000m;

            var formatted = value.ToHuman(2, culture);
            _output.WriteLine($"[Arabic] Value: {value} -> Formatted: {formatted}");

            var success = HumanNumber.TryParse(formatted, culture, out var parsed);
            Assert.True(success);
            Assert.Equal(value, parsed);
        }

        [Fact]
        public void Verify_Deterministic_I18N_Support_Across_Global_Cultures()
        {
            var allCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            var commonCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "en", "es", "fr", "de" };
            
            var random = new Random(DeterministicSeed);
            var testCultures = allCultures
                .Where(c => !commonCodes.Contains(c.TwoLetterISOLanguageName))
                .OrderBy(x => random.Next())
                .Take(20)
                .ToList();

            foreach (var culture in testCultures)
            {
                ValidateAndLogCulture(culture);
            }
        }

        private void ValidateAndLogCulture(CultureInfo culture)
        {
            var val = 1250000.55m;
            var numFormat = culture.NumberFormat;

            var human = val.ToHuman(2, culture);
            var currency = val.ToHumanCurrency(2, culture);
            
            var options = new HumanNumberFormatOptions { Threshold = 999999999m };
            var formattedLarge = 1000000m.ToHuman(options, culture);

            _output.WriteLine($"Culture: {culture.Name} ({culture.DisplayName})");
            _output.WriteLine($"  - Human:    {human}");
            _output.WriteLine($"  - Currency: {currency}");
            _output.WriteLine($"  - Grouping: {formattedLarge}");
            _output.WriteLine(new string('-', 40));

            // Basic Assertions
            Assert.Contains(numFormat.NumberDecimalSeparator, human);
            Assert.Contains(numFormat.CurrencyDecimalSeparator, currency);
            
            var symbol = numFormat.CurrencySymbol;
            if (!string.IsNullOrEmpty(symbol)) Assert.Contains(symbol, currency);
        }
    }
}
