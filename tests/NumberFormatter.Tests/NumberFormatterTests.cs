using System.Globalization;
using NumberFormatter;

namespace NumberFormatter.Tests;

public class NumberFormatterTests
{
    [Fact]
    public void ToShortString_WithThousands_ReturnsK()
    {
        // Arrange
        var value = 1234.56m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23K", result);
    }

    [Fact]
    public void ToShortString_WithMillions_ReturnsM()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23M", result);
    }

    [Fact]
    public void ToShortString_WithBillions_ReturnsB()
    {
        // Arrange
        var value = 1234567890.12m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23B", result);
    }

    [Fact]
    public void ToShortString_WithTrillions_ReturnsT()
    {
        // Arrange
        var value = 1234567890123.45m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23T", result);
    }

    [Fact]
    public void ToShortString_WithNegativeValue_ReturnsNegative()
    {
        // Arrange
        var value = -1234.56m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("-1.23K", result);
    }

    [Fact]
    public void ToShortString_WithZero_ReturnsZero()
    {
        // Arrange
        var value = 0m;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("0", result);
    }

    [Fact]
    public void ToShortString_WithDecimalPlaces_RespectsParameter()
    {
        // Arrange
        var value = 1234.5678m;

        // Act
        var result = value.ToShortString(3);

        // Assert
        Assert.Equal("1.235K", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithUSD_ReturnsFormatted()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToShortCurrencyString(currencyCode: "USD");

        // Assert
        Assert.Equal("$1.23M", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithETB_ReturnsFormatted()
    {
        // Arrange
        var value = 1234567.89m;

        // Act
        var result = value.ToShortCurrencyString(currencyCode: "ETB");

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
        var result = value.ToShortString(2, germanCulture);

        // Assert
        Assert.Equal("1,23K", result);
    }

    [Fact]
    public void ToShortString_WithCustomOptions_AppliesFormatting()
    {
        // Arrange
        var value = 1234.56m;
        var options = new ShortNumberFormatOptions
        {
            DecimalPlaces = 1,
            ShowPlusSign = true
        };

        // Act
        var result = value.ToShortString(options);

        // Assert
        Assert.Equal("+1.2K", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithNegativePattern_AppliesCorrectly()
    {
        // Arrange
        var value = -1234.56m;
        var options = new ShortNumberFormatOptions
        {
            CurrencySymbol = "$",
            NegativePattern = "(n)"
        };

        // Act
        var result = value.ToShortString(options);

        // Assert
        Assert.Equal("($1.23K)", result);
    }

    [Fact]
    public void ToShortCurrencyString_WithCurrencyAfter_FormatsCorrectly()
    {
        // Arrange
        var value = 1234.56m;
        var options = new ShortNumberFormatOptions
        {
            CurrencySymbol = "€",
            CurrencyPosition = CurrencyPosition.After
        };

        // Act
        var result = value.ToShortString(options);

        // Assert
        Assert.Equal("1.23K€", result);
    }

    [Fact]
    public void ToShortString_WithGenericInt_WorksCorrectly()
    {
        // Arrange
        int value = 1234567;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23M", result);
    }

    [Fact]
    public void ToShortString_WithGenericLong_WorksCorrectly()
    {
        // Arrange
        long value = 1234567890;

        // Act
        var result = value.ToShortString();

        // Assert
        Assert.Equal("1.23B", result);
    }

    [Fact]
    public void ToShortString_WithVerySmallNumber_FormatsCorrectly()
    {
        // Arrange
        var value = 0.00123m;

        // Act
        var result = value.ToShortString(3);

        // Assert
        Assert.Equal("0.001", result);
    }

    [Fact]
    public void ToShortString_WithCustomSuffixes_UsesThem()
    {
        // Arrange
        var value = 1234567m;
        var options = new ShortNumberFormatOptions
        {
            CustomSuffixes = new[] { "", "Thousand", "Million", "Billion" }
        };

        // Act
        var result = value.ToShortString(options);

        // Assert
        Assert.Equal("1.23Million", result);
    }
}