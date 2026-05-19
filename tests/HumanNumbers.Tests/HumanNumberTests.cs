using System.Globalization;
using HumanNumbers;
using HumanNumbers.Currencies;
using HumanNumbers.Formatting;
using HumanNumbers.Suffixes;

namespace HumanNumbers.Tests;

public class HumanNumberTests
{
    [Fact]
    public void ToShortString_WithThousands_ReturnsK()
    {
        // Arrange
        var value = 1234.56m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23K", result);
    }

    [Fact]
    public void ToShortString_WithMillions_ReturnsM()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23M", result);
    }

    [Fact]
    public void ToShortString_WithBillions_ReturnsB()
    {
        // Arrange
        var value = 1234567890.12m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23B", result);
    }

    [Fact]
    public void ToShortString_WithTrillions_ReturnsT()
    {
        // Arrange
        var value = 1234567890123.45m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23T", result);
    }

    [Fact]
    public void ToShortString_WithNegativeValue_ReturnsNegative()
    {
        // Arrange
        var value = -1234.56m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("-1.23K", result);
    }

    [Fact]
    public void ToShortString_WithZero_ReturnsZero()
    {
        // Arrange
        var value = 0m;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void ToShortString_WithDecimalPlaces_RespectsParameter()
    {
        // Arrange
        var value = 1234.5678m;

        // Act
        var result = value.ToHuman(3);

        // Assert
        Assert.Equal("1.235K", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithUSD_ReturnsFormatted()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToHumanCurrency(currencyCode: "USD");

        // Assert
        Assert.Equal("$1.23M", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithETB_ReturnsFormatted()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToHumanCurrency(currencyCode: "ETB");

        // Assert
        Assert.Equal("Br1.23M", result);
    }

    [Fact]
    public void ToShortString_WithGermanCulture_UsesCommaDecimal()
    {
        // Arrange
        var value = 1234.56m;
        var germanCulture = new CultureInfo("de-DE");

        // Act
        var result = value.ToHuman(2, germanCulture);

        // Assert
        Assert.Equal("1,23K", result);
    }

    [Fact]
    public void ToShortString_WithCustomOptions_AppliesFormatting()
    {
        // Arrange
        var value = 1234.56m;
        var options = new HumanNumberFormatOptions
        {
            DecimalPlaces = 1,
            ShowPlusSign = true
        };

        // Act
        var result = value.ToHuman(options);

        // Assert
        Assert.Equal("+1.2K", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithNegativePattern_AppliesCorrectly()
    {
        // Arrange
        var value = -1234.56m;
        var options = new HumanNumberFormatOptions
        {
            CurrencySymbol = "$",
            NegativePattern = "(n)"
        };

        // Act
        var result = value.ToHuman(options);

        // Assert
        Assert.Equal("($1.23K)", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithCurrencyAfter_FormatsCorrectly()
    {
        // Arrange
        var value = 1234.56m;
        var options = new HumanNumberFormatOptions
        {
            CurrencySymbol = "â‚¬",
            CurrencyPosition = CurrencyPosition.After
        };

        // Act
        var result = value.ToHuman(options);

        // Assert
        Assert.Equal("1.23Kâ‚¬", result);
    }

    [Fact]
    public void ToShortString_WithGenericInt_WorksCorrectly()
    {
        // Arrange
        int value = 1234567;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23M", result);
    }

    [Fact]
    public void ToShortString_WithGenericLong_WorksCorrectly()
    {
        // Arrange
        long value = 1234567890;

        // Act
        var result = value.ToHuman();

        // Assert
        Assert.Equal("1.23B", result);
    }

    [Fact]
    public void ToShortString_WithVerySmallNumber_FormatsCorrectly()
    {
        // Arrange
        var value = 0.00123m;

        // Act
        var result = value.ToHuman(3);

        // Assert
        Assert.Equal("0.001", result);
    }

    [Fact]
    public void ToShortString_WithCustomSuffixes_UsesThem()
    {
        // Arrange
        var value = 1234567m;
        var options = new HumanNumberFormatOptions
        {
            CustomSuffixes = new[] { "", "Thousand", "Million", "Billion" }
        };

        // Act
        var result = value.ToHuman(options);

        // Assert
        Assert.Equal("1.23Million", result);
    }

    [Fact]
    public void Should_Handle_Negative_And_Edge_Case_Numbers()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");

        // Standard negative scaling
        Assert.Contains("-1.50M", (-1500000m).ToHuman(2, culture));
        
        // Extreme values
        Assert.False(string.IsNullOrWhiteSpace(decimal.MinValue.ToHuman(2, culture)));

        // Zero and Tiny values
        Assert.Equal("0", 0m.ToHuman(2, culture));
        // 0.0001 with 2 decimal places rounds to 0. With default suppression, it's "0"
        Assert.Equal("0", 0.0001m.ToHuman(2, culture)); 
    }

    [Fact]
    public void Should_Maintain_RoundTrip_Consistency_With_Scaling()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        // Use values that don't lose precision when scaled to 2 decimal places
        var values = new[] { 1500m, 1500000m, 1500000000m };

        foreach (var val in values)
        {
            var formatted = val.ToHuman(2, culture);
            var success = HumanNumber.TryParse(formatted, culture, out var parsed);

            Assert.True(success, $"Failed to parse scaled value '{formatted}'");
            Assert.Equal(val, parsed);
        }
    }

    [Fact]
    public void ResetHooks_ClearGlobalConfigurationState()
    {
        // Arrange
        var customPolicy = new HumanNumberFormatOptions { DecimalPlaces = 5 };
        HumanNumbersConfig.Instance.AddPolicy("CustomResetTest", customPolicy);
        Assert.True(HumanNumbersConfig.Instance.TryGetPolicy("CustomResetTest", out _));

        CurrencyRegistry.RegisterCurrency("XXX", "X_SYMBOL");
        Assert.Equal("X_SYMBOL", CurrencyRegistry.GetSymbol("XXX"));

        // Act
        HumanNumbersConfig.Reset();
        CurrencyRegistry.Reset();

        // Assert
        Assert.False(HumanNumbersConfig.Instance.TryGetPolicy("CustomResetTest", out _));
        Assert.Equal("XXX", CurrencyRegistry.GetSymbol("XXX")); // should fall back to using currency code
    }

    [Fact]
    public void LargeCustomSuffixSets_TriggersBinarySearch_FormatsCorrectly()
    {
        // Create a large list of custom suffixes (> 8 elements) manually to avoid decimal overflow
        var suffixes = new MagnitudeSuffix[]
        {
            new(1000000000000000m, "Qa"),
            new(1000000000000m, "T"),
            new(1000000000m, "B"),
            new(100000000m, "HM"),
            new(10000000m, "TenM"),
            new(1000000m, "M"),
            new(100000m, "HK"),
            new(10000m, "TenK"),
            new(1000m, "K"),
            new(1m, "")
        };
        var options = new HumanNumberFormatOptions
        {
            CachedCustomSuffixes = suffixes
        };

        // Act
        var result1 = 1250m.ToHuman(options); // 1.25K
        var result2 = 1250000000000m.ToHuman(options); // 1.25T

        // Assert
        Assert.Equal("1.25K", result1);
        Assert.Equal("1.25T", result2);
    }
}
