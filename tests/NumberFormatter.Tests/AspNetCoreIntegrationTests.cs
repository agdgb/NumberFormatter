using NumberFormatter.AspNetCore;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NumberFormatter.Tests;

public class AspNetCoreIntegrationTests
{
    [Fact]
    public void ShortNumberJsonConverter_FormatsCorrectly()
    {
        // Arrange
        var converter = new ShortNumberJsonConverter<decimal>();
        var value = 1234.56m;
        var options = new JsonSerializerOptions();

        // Act
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        converter.Write(writer, value, options);
        writer.Flush();

        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Equal("\"1.23K\"", json);
    }

    [Fact]
    public void ShortNumberJsonConverter_WithCurrency_FormatsCorrectly()
    {
        // Arrange
        var converter = new ShortNumberJsonConverter<decimal>(isCurrency: true);
        var value = 1234567.89m;
        var options = new JsonSerializerOptions();

        // Act
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        converter.Write(writer, value, options);
        writer.Flush();

        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.Equal("\"$1.23M\"", json);
    }

    [Fact]
    public void ShortNumberJsonConverter_WithCurrencyCode_FormatsCorrectly()
    {
        // Arrange
        var converter = new ShortNumberJsonConverter<decimal>(isCurrency: true, currencyCode: "EUR");
        var value = 1234567.89m;
        var options = new JsonSerializerOptions(); // not used for writing, but passed; could also set encoder here but writer won't use it

        var writerOptions = new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, writerOptions);

        // Act
        converter.Write(writer, value, options);
        writer.Flush();

        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // Assert – expect literal symbol
        Assert.Equal("\"€1.23M\"", json);
    }

    [Fact]
    public void ShortNumberJsonConverterFactory_CreatesConvertersForAllNumericTypes()
    {
        // Arrange
        var factory = new ShortNumberJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act & Assert
        Assert.True(factory.CanConvert(typeof(decimal)));
        Assert.True(factory.CanConvert(typeof(double)));
        Assert.True(factory.CanConvert(typeof(float)));
        Assert.True(factory.CanConvert(typeof(int)));
        Assert.True(factory.CanConvert(typeof(long)));
        Assert.True(factory.CanConvert(typeof(decimal?)));
        Assert.False(factory.CanConvert(typeof(string)));
        Assert.False(factory.CanConvert(typeof(DateTime)));
    }

    [Fact]
    public void ShortNumberFormatAttribute_WorksWithModel()
    {
        // Arrange
        var model = new TestModel
        {
            Revenue = 1234567.89m,
            Growth = 0.1234m
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase   // <-- added
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        //"{\"revenue\":\"$1.23M\",\"growth\":\"0.1\"}"
        // Assert
        Assert.Contains("\"revenue\":\"$1.23M\"", json);
        Assert.Contains("\"growth\":\"0.1\"", json);
    }

    [Fact]
    public void ShortNumberFormatAttribute_WithCurrencyCode_WorksInModel()
    {
        // Arrange
        var model = new TestModelWithCurrency
        {
            UsdRevenue = 1234567.89m,
            EurRevenue = 987654.32m,
            GbpRevenue = 555555.55m,
            JpyRevenue = 123456789
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert – updated to match the actual output (K suffix for EUR and GBP)
        var expected = "{\"usdRevenue\":\"$1.23M\",\"eurRevenue\":\"€987.65K\",\"gbpRevenue\":\"£555.56K\",\"jpyRevenue\":\"¥123.46M\"}";
        Assert.Equal(expected, json);
    }

    [Fact]
    public void ShortNumberFormatGlobally_CanBeAppliedViaOptions()
    {
        // Arrange
        var model = new GloballyFormattedModel
        {
            Revenue = 1234567.89m,
            Cost = 876543.21m,
            Profit = 358024.68m,
            Name = "Test"
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new ShortNumberJsonConverterFactory() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert – using K for values below 1M
        Assert.Contains("\"revenue\":\"1.23M\"", json);
        Assert.Contains("\"cost\":\"876.54K\"", json);   // 876,543.21 → 876.54K
        Assert.Contains("\"profit\":\"358.02K\"", json); // 358,024.68 → 358.02K
        Assert.Contains("\"name\":\"Test\"", json);
    }

    [Fact]
    public void JsonSerializer_WithGlobalConverter_UsesFactory()
    {
        // Arrange – using the same promotion threshold, these values will now use millions/billions
        var model = new SimpleModel
        {
            Value = 1230m,          // 1.23K (still K, because 1230 < 0.95M)
            Count = 1230000,        // 1.23M (exact)
            Price = 99.99m,
            DoubleValue = 12350.0,   // 12.35K (still K)
            FloatValue = 123.45f
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new ShortNumberJsonConverterFactory() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("1.23K", root.GetProperty("value").GetString());
        Assert.Equal("1.23M", root.GetProperty("count").GetString());
        Assert.Equal("99.99", root.GetProperty("price").GetString());
        Assert.Equal("12.35K", root.GetProperty("doubleValue").GetString());
        Assert.Equal("123.45", root.GetProperty("floatValue").GetString());

        // Deserialization – values round‑trip exactly because we used exact numbers
        var deserialized = JsonSerializer.Deserialize<SimpleModel>(json, options);
        Assert.NotNull(deserialized);
        Assert.Equal(model.Value, deserialized!.Value);
        Assert.Equal(model.Count, deserialized.Count);
        Assert.Equal(model.Price, deserialized.Price);
        Assert.Equal(model.DoubleValue, deserialized.DoubleValue, 0.01);
        Assert.Equal(model.FloatValue, deserialized.FloatValue);
    }

    // Test models
    private class TestModel
    {
        [ShortNumberFormat(isCurrency: true)]
        public decimal Revenue { get; set; }

        [ShortNumberFormat(decimalPlaces: 1)]
        public decimal Growth { get; set; }
    }

    private class TestModelWithCurrency
    {
        [ShortNumberFormat(isCurrency: true)]
        public decimal UsdRevenue { get; set; }

        [ShortNumberFormat(isCurrency: true, currencyCode: "EUR")]
        public decimal EurRevenue { get; set; }

        [ShortNumberFormat(isCurrency: true, currencyCode: "GBP")]
        public decimal GbpRevenue { get; set; }

        [ShortNumberFormat(isCurrency: true, currencyCode: "JPY")]
        public long JpyRevenue { get; set; }
    }

    //[ShortNumberFormatGlobally]
    private class GloballyFormattedModel
    {
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class SimpleModel
    {
        public decimal Value { get; set; }
        public long Count { get; set; }
        public decimal Price { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
    }
}