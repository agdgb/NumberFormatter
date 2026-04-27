using System.Globalization;
using HumanNumbers;
using HumanNumbers.Formatting;
using Xunit;

namespace HumanNumbers.Tests
{
    public class SuffixPromotionTests
    {
        [Theory]
        [InlineData(999999, "1.00M")]
        [InlineData(999.999, "1.00K")]
        [InlineData(999999999, "1.00B")]
        public void Should_Promote_Suffix_After_Rounding(decimal value, string expected)
        {
            // Using default options: DecimalPlaces = 2, PromotionThreshold = 0.95
            var result = value.ToHuman(culture: CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_Handle_Values_Just_Below_Threshold_With_Rounding()
        {
            // 999,999 with 2 decimal places rounds to 1,000,000.00
            // Without promotion logic, this would be 1000.00K
            var result = 999999m.ToHuman(2, CultureInfo.InvariantCulture);
            Assert.Equal("1.00M", result);
        }

        [Fact]
        public void Should_Handle_High_Precision_Rounding_Promotion()
        {
            // Value is 999.999, decimal places is 2
            // Scaled for K: 0.999999 -> rounds to 1.00K
            // Scaled for "": 999.999 -> rounds to 1000.00
            // If we use "", we get "1000.00"
            // If we use "K", we get "1.00K"
            var result = 999.999m.ToHuman(2, CultureInfo.InvariantCulture);
            Assert.Equal("1.00K", result);
        }

        [Theory]
        [InlineData(949000, "Readable", "949.00K")] 
        [InlineData(950000, "Readable", "0.95M")]   
        [InlineData(999499, "Readable", "1.00M")]   
        [InlineData(999499, "Strict", "999.50K")]   
        [InlineData(1000000, "Strict", "1.00M")]
        public void Should_Respect_Strict_And_Readable_Promotion_Boundaries(decimal value, string mode, string expected)
        {
            var culture = CultureInfo.InvariantCulture;
            var options = mode == "Strict" 
                ? new HumanNumberFormatOptions { PromotionThreshold = 1.0m, DecimalPlaces = 2, SuppressDefaultDecimals = false }
                : new HumanNumberFormatOptions { DecimalPlaces = 2, SuppressDefaultDecimals = false };

            var result = value.ToHuman(options, culture);

            Assert.Equal(expected, result);
        }
    }
}
