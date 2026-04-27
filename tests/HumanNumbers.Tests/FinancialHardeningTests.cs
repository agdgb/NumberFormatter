using System.Collections.Generic;
using System.Text.Json;
using HumanNumbers.AspNetCore.Financial;
using Xunit;

namespace HumanNumbers.Tests;

public class FinancialHardeningTests
{
    private class HardenedDto
    {
        [BasisPoints]
        public decimal? NullableSpread { get; set; }

        [BasisPoints]
        public List<decimal>? SpreadList { get; set; }

        [BasisPoints]
        public decimal[]? SpreadArray { get; set; }

        [BasisPoints]
        public Dictionary<string, decimal>? SpreadDictionary { get; set; }
        
        [BasisPoints]
        public IEnumerable<decimal?>? NullableIEnumerable { get; set; }
    }

    [Fact]
    public void Serializes_Nullables_And_Collections_Correctly()
    {
        var dto = new HardenedDto
        {
            NullableSpread = 0.0125m, // 125 bps
            SpreadList = new List<decimal> { 0.0125m, 0.0050m }, // 125 bps, 50 bps
            SpreadArray = new decimal[] { 0.0200m }, // 200 bps
            SpreadDictionary = new Dictionary<string, decimal> { ["US"] = 0.0125m },
            NullableIEnumerable = new List<decimal?> { 0.0125m, null }
        };

        var options = new JsonSerializerOptions();
        var json = JsonSerializer.Serialize(dto, options);

        Assert.Contains("\"NullableSpread\":\"125 bps\"", json);
        Assert.Contains("\"SpreadList\":[\"125 bps\",\"50 bps\"]", json);
        Assert.Contains("\"SpreadArray\":[\"200 bps\"]", json);
        Assert.Contains("\"SpreadDictionary\":{\"US\":\"125 bps\"}", json);
        Assert.Contains("\"NullableIEnumerable\":[\"125 bps\",null]", json);
    }

    [Fact]
    public void Deserializes_Nullables_And_Collections_Correctly()
    {
        var json = @"{
          ""NullableSpread"": ""125 bps"",
          ""SpreadList"": [""125 bps"", ""50 bps""],
          ""SpreadArray"": [""200 bps""],
          ""SpreadDictionary"": { ""US"": ""125 bps"" },
          ""NullableIEnumerable"": [""125 bps"", null]
        }";

        var options = new JsonSerializerOptions();
        var dto = JsonSerializer.Deserialize<HardenedDto>(json, options);

        Assert.NotNull(dto);
        Assert.Equal(0.0125m, dto!.NullableSpread);
        
        Assert.NotNull(dto.SpreadList);
        Assert.Equal(2, dto.SpreadList.Count);
        Assert.Equal(0.0125m, dto.SpreadList[0]);
        Assert.Equal(0.0050m, dto.SpreadList[1]);

        Assert.NotNull(dto.SpreadArray);
        Assert.Single(dto.SpreadArray);
        Assert.Equal(0.0200m, dto.SpreadArray[0]);

        Assert.NotNull(dto.SpreadDictionary);
        Assert.True(dto.SpreadDictionary.ContainsKey("US"));
        Assert.Equal(0.0125m, dto.SpreadDictionary["US"]);
        
        Assert.NotNull(dto.NullableIEnumerable);
        var nullableList = new List<decimal?>(dto.NullableIEnumerable);
        Assert.Equal(0.0125m, nullableList[0]);
        Assert.Null(nullableList[1]);
    }
}
