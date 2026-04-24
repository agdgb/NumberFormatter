using System.Text.Json;
using HumanNumbers.AspNetCore.Financial;
using Xunit;

namespace NumberFormatter.Tests;

public class FinancialJsonAttributeTests
{
    private class MixedDto
    {
        public decimal Amount { get; set; }
        
        [BasisPoints] 
        public decimal Spread { get; set; }

        [FractionPrice(32)]
        public decimal TreasuryPrice { get; set; }
    }

    private class OverrideDto
    {
        [BasisPoints(Decimals = 2)]
        public decimal Spread { get; set; }
        
        [FractionPrice(64)]
        public decimal Price { get; set; }
    }

    [Fact]
    public void MixedDto_Serializes_OnlyAttributes()
    {
        var dto = new MixedDto
        {
            Amount = 1000.25m,
            Spread = 0.0125m,
            TreasuryPrice = 101.5m
        };

        // Note: No global converters added to JsonSerializerOptions here!
        // This validates "without service registration" capability.
        var options = new JsonSerializerOptions(); 

        var json = JsonSerializer.Serialize(dto, options);

        // Verify standard serialization
        Assert.Contains("\"Amount\":1000.25", json);
        // Verify attribute-driven basis points
        Assert.Contains("\"Spread\":\"125 bps\"", json);
        // Verify attribute-driven fractions
        Assert.Contains("\"TreasuryPrice\":\"101 16/32\"", json);
    }

    [Fact]
    public void MixedDto_Deserializes_Correctly()
    {
        var json = @"{
          ""Amount"": 1000.25,
          ""Spread"": ""125 bps"",
          ""TreasuryPrice"": ""101 16/32""
        }";

        var options = new JsonSerializerOptions();
        var dto = JsonSerializer.Deserialize<MixedDto>(json, options);

        Assert.NotNull(dto);
        Assert.Equal(1000.25m, dto!.Amount);
        Assert.Equal(0.0125m, dto.Spread);
        Assert.Equal(101.5m, dto.TreasuryPrice);
    }

    [Fact]
    public void Attributes_Override_GlobalSettings()
    {
        var dto = new OverrideDto
        {
            Spread = 0.012345m,
            Price = 101.015625m // Formats accurately resolving exactly to 1/64
        };

        var options = new JsonSerializerOptions();
        var json = JsonSerializer.Serialize(dto, options);

        Assert.Contains("\"Spread\":\"123.45 bps\"", json);
        Assert.Contains("\"Price\":\"101 1/64\"", json);
    }
}
