using System;
using HumanNumbers.Financial;
using Xunit;

namespace NumberFormatter.Tests;

public class FinancialTests
{
    // --- WordsFormatter Tests ---
    [Theory]
    [InlineData(0, "Zero Dollars and 00/100")]
    [InlineData(1, "One Dollar and 00/100")] // Singular rule
    [InlineData(1.01, "One Dollar and 01/100")]
    [InlineData(1234.56, "One Thousand Two Hundred Thirty-Four Dollars and 56/100")]
    [InlineData(-1234.56, "Negative One Thousand Two Hundred Thirty-Four Dollars and 56/100")]
    [InlineData(1000, "One Thousand Dollars and 00/100")]
    [InlineData(1000000, "One Million Dollars and 00/100")]
    [InlineData(1001, "One Thousand One Dollars and 00/100")] // Not thousands
    [InlineData(999.99, "Nine Hundred Ninety-Nine Dollars and 99/100")]
    public void ToCheckWords_FormatsCorrectly(decimal value, string expected)
    {
        var result = value.ToHumanWords("Dollars", "Dollar");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10.50, "Ten Euros and 50/100")]
    public void ToCheckWords_WithCustomCurrency_FormatsCorrectly(decimal value, string expected)
    {
        var result = value.ToHumanWords("Euros", "Euro");
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(0, "Zero")]
    [InlineData(15, "Fifteen")]
    [InlineData(-15, "Negative Fifteen")]
    [InlineData(100, "One Hundred")]
    [InlineData(1234.56, "One Thousand Two Hundred Thirty-Four and 56/100")]
    [InlineData(1.996, "Two")] // Tests Math.Round to 2 decimal places before processing
    public void ToWords_FormatsCorrectly(decimal value, string expected)
    {
        var result = value.ToHumanWords();
        Assert.Equal(expected, result);
    }

    // --- BasisPointFormatter Tests ---

    [Theory]
    [InlineData(0.0125, 125)]
    [InlineData(0.01, 100)]
    [InlineData(0.0001, 1)]
    [InlineData(-0.0125, -125)]
    public void ToBps_ReturnsCorrectDecimal(decimal value, decimal expected)
    {
        Assert.Equal(expected, value.ToBps());
    }

    [Theory]
    [InlineData(0.0125, 0, "125 bps")]
    [InlineData(0.012345, 2, "123.45 bps")]
    [InlineData(-0.0125, 0, "-125 bps")]
    [InlineData(0.01256, 1, "125.6 bps")]
    [InlineData(0.012555, 1, "125.6 bps")] // AwayFromZero midpoint rounding
    public void ToBpsString_FormatsCorrectly(decimal value, int decimals, string expected)
    {
        Assert.Equal(expected, value.ToHumanBps(decimals));
    }

    [Theory]
    [InlineData("125 bps", 0.0125)]
    [InlineData("125", 0.0125)]
    [InlineData("123.45 bps", 0.012345)]
    [InlineData("-125    bps", -0.0125)]
    public void TryParseBps_ParsesCorrectly(string input, decimal expected)
    {
        Assert.True(BasisPointFormatter.TryParseBps(input, out var result));
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void TryParseBps_InvalidString_ReturnsFalse()
    {
        Assert.False(BasisPointFormatter.TryParseBps("invalid", out var _));
        Assert.False(BasisPointFormatter.TryParseBps("123.abc bps", out var _));
    }

    // --- FinancialRounding Tests ---

    [Theory]
    // Nearest
    [InlineData(10.22, 0.05, TickRoundingMode.Nearest, 10.20)]
    [InlineData(10.23, 0.05, TickRoundingMode.Nearest, 10.25)]
    [InlineData(-10.22, 0.05, TickRoundingMode.Nearest, -10.20)] // MidpointRounding.ToEven acts symetrically
    [InlineData(-10.23, 0.05, TickRoundingMode.Nearest, -10.25)]
    // Up (Ceiling)
    [InlineData(10.21, 0.05, TickRoundingMode.Up, 10.25)]
    [InlineData(-10.21, 0.05, TickRoundingMode.Up, -10.20)] // Ceiling goes towards positive infinity
    // Down (Floor)
    [InlineData(10.24, 0.05, TickRoundingMode.Down, 10.20)]
    [InlineData(-10.24, 0.05, TickRoundingMode.Down, -10.25)] // Floor goes towards negative infinity
    public void RoundToTick_AppliesCorrectRounding(decimal value, decimal tickSize, TickRoundingMode mode, decimal expected)
    {
        Assert.Equal(expected, value.RoundToTick(tickSize, mode));
    }
    
    [Theory]
    [InlineData(10.03125, 32, "10 1/32")]
    [InlineData(-10.03125, 32, "-10 1/32")]
    [InlineData(10.5, 32, "10 16/32")]
    [InlineData(10.99999, 32, "11")] // Rounds up to 32/32 -> integral + 1
    [InlineData(10.0, 32, "10")]
    public void ToFractionString_FormatsCorrectly(decimal value, int denominator, string expected)
    {
        Assert.Equal(expected, value.ToHumanFraction(denominator));
    }

    [Theory]
    [InlineData("10 1/32", 10.03125)]
    [InlineData("-10 1/32", -10.03125)]
    [InlineData("1/2", 0.5)]
    [InlineData("-1/2", -0.5)]
    [InlineData("10", 10)]
    [InlineData("-10", -10)]
    public void TryParseFraction_ParsesCorrectly(string input, decimal expected)
    {
        Assert.True(FinancialRounding.TryParseFraction(input, out var result));
        Assert.Equal(expected, result);
    }
}
