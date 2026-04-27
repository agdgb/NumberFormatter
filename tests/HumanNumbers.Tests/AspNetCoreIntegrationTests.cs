using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Serialization;
using Xunit;

namespace HumanNumbers.Tests;

public class AspNetCoreIntegrationTests
{
        [Fact]
    public void HumanNumberAttribute_DefaultMode_DoesNotFormatInJson()
    {
        // Arrange
        var model = new DefaultModeModel { Value = 1234567.89m };
        // We simulate the DI setup manually
        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = GetResolver()
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert - should be a raw number
        Assert.Contains("\"value\":1234567.89", json);
    }

    [Fact]
    public void HumanNumberAttribute_WithSerializeAsHuman_FormatsInJson()
    {
        // Arrange
        var model = new SerializeModeModel { Value = 1234567.89m };
        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = GetResolver()
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert - should be humanized string
        Assert.Contains("\"value\":\"1.23M\"", json);
    }

    [Fact]
    public void HumanNumberAttribute_Nullable_FormatsCorrectly()
    {
        // Arrange
        var model = new NullableModel { Value = 1234.56m };
        var options = new JsonSerializerOptions 
        { 
            TypeInfoResolver = GetResolver()
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.Contains("\"Value\":\"1.23K\"", json);
    }

    [Fact]
    public void HumanNumberAttribute_Conflict_RespectsExistingConverter()
    {
        // Arrange
        var model = new ConflictModel { Value = 1234.56m };
        var options = new JsonSerializerOptions 
        { 
            TypeInfoResolver = GetResolver()
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert - should be 1234.56 (raw) because MySimpleConverter writes raw number, 
        // but crucially NOT "1.23K"
        Assert.Contains("\"Value\":1234.56", json);
    }

    private static IJsonTypeInfoResolver GetResolver()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(ServiceCollectionExtensions.CreateHumanNumberModifier);
        return resolver;
    }

    private class DefaultModeModel
    {
        [HumanNumber]
        public decimal Value { get; set; }
    }

    private class SerializeModeModel
    {
        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]
        public decimal Value { get; set; }
    }

    private class NullableModel
    {
        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]
        public decimal? Value { get; set; }
    }

    private class ConflictModel
    {
        [JsonConverter(typeof(NumericPassthroughConverter<decimal>))]
        [HumanNumber(OutputMode = HumanNumberOutputMode.SerializeAsHuman)]
        public decimal Value { get; set; }
    }
}