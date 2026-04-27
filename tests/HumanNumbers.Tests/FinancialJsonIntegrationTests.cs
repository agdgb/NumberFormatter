using System.Text.Json;
using HumanNumbers.AspNetCore.Financial;
using Xunit;

namespace HumanNumbers.Tests;

public class FinancialJsonIntegrationTests
{
    private class BasisPointModel
    {
        public decimal Spread { get; set; }
    }

    private class FractionPriceModel
    {
        public decimal Price { get; set; }
    }

    [Fact]
    public void BasisPointsJsonConverter_Serializes_ToBpsString()
    {
        var model = new BasisPointModel { Spread = 0.0125m };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new BasisPointsJsonConverter());

        var json = JsonSerializer.Serialize(model, options);

        Assert.Contains("\"Spread\":\"125 bps\"", json);
    }

    [Fact]
    public void BasisPointsJsonConverter_WithWriteAsStringFalse_SerializesAsNumber()
    {
        var model = new BasisPointModel { Spread = 0.0125m };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new BasisPointsJsonConverter(new BasisPointJsonOptions { WriteAsString = false }));

        var json = JsonSerializer.Serialize(model, options);

        Assert.Contains("\"Spread\":125", json);
    }

    [Theory]
    [InlineData("{\"Spread\": \"125 bps\"}", 0.0125)]
    [InlineData("{\"Spread\": \"125.5 bps\"}", 0.01255)]
    [InlineData("{\"Spread\": \"125\"}", 0.0125)]
    [InlineData("{\"Spread\": 125}", 0.0125)]
    [InlineData("{\"Spread\": 0.0125}", 0.00000125)] // By strict definition, raw numbers get divided by 10000 uniformly
    public void BasisPointsJsonConverter_Deserializes_Correctly(string json, decimal expected)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new BasisPointsJsonConverter());

        var model = JsonSerializer.Deserialize<BasisPointModel>(json, options);

        Assert.NotNull(model);
        Assert.Equal(expected, model!.Spread);
    }

    [Fact]
    public void FractionPriceJsonConverter_Serializes_ToFractionString()
    {
        var model = new FractionPriceModel { Price = 101.5m };
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FractionPriceJsonConverter(new FractionJsonOptions { Denominator = 32 }));

        var json = JsonSerializer.Serialize(model, options);

        Assert.Contains("\"Price\":\"101 16/32\"", json);
    }

    [Theory]
    [InlineData("{\"Price\": \"101 16/32\"}", 101.5)]
    [InlineData("{\"Price\": \"16/32\"}", 0.5)]
    [InlineData("{\"Price\": 101.5}", 101.5)]
    public void FractionPriceJsonConverter_Deserializes_Correctly(string json, decimal expected)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new FractionPriceJsonConverter(new FractionJsonOptions { Denominator = 32 }));

        var model = JsonSerializer.Deserialize<FractionPriceModel>(json, options);

        Assert.NotNull(model);
        Assert.Equal(expected, model!.Price);
    }
}
