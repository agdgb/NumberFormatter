using HumanNumbers;
using HumanNumbers.Formatting;
using Xunit;

namespace HumanNumbers.Tests
{
    public class AlwaysShowSuffixTests
    {
        [Fact]
        public void ToHuman_WithAlwaysShowSuffix_ShowsSuffixForSmallNumbers()
        {
            // Arrange
            var value = 50m;
            var options = new HumanNumberFormatOptions
            {
                AlwaysShowSuffix = true,
                DecimalPlaces = 2
            };

            // Act
            var result = value.ToHuman(options);

            // Assert
            // 50 / 1000 = 0.05
            Assert.Equal("0.05K", result);
        }

        [Fact]
        public void ToHuman_WithoutAlwaysShowSuffix_DoesNotShowSuffixForSmallNumbers()
        {
            // Arrange
            var value = 50m;
            var options = new HumanNumberFormatOptions
            {
                AlwaysShowSuffix = false,
                DecimalPlaces = 2
            };

            // Act
            var result = value.ToHuman(options);

            // Assert
            Assert.Equal("50", result);
        }

        [Fact]
        public void ToHuman_WithCustomThreshold_RespectsAlwaysShowSuffix()
        {
            // Arrange
            var value = 500m;
            var options = new HumanNumberFormatOptions
            {
                Threshold = 1000000m, // Only scale at 1M
                AlwaysShowSuffix = true,
                DecimalPlaces = 2
            };

            // Act
            var result = value.ToHuman(options);

            // Assert
            // 500 / 1000 = 0.50K
            Assert.Equal("0.50K", result);
        }

        [Fact]
        public void ToHuman_WithVerySmallNumber_ShowsZeroSuffix()
        {
            // Arrange
            var value = 0.1m;
            var options = new HumanNumberFormatOptions
            {
                AlwaysShowSuffix = true,
                DecimalPlaces = 2
            };

            // Act
            var result = value.ToHuman(options);

            // Assert
            Assert.Equal("0.00K", result);
        }
    }
}
