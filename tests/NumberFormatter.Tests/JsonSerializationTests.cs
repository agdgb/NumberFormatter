using NumberFormatter.AspNetCore;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace NumberFormatter.Tests;

public class JsonSerializationTests
{
    [Fact]
    public void Serialize_ComplexObject_AllFormatsApplied()
    {
        // Arrange
        var report = new FinancialReport
        {
            Id = 12345,
            Title = "Q4 2023 Report",
            Revenue = 12345678.90m,
            Expenses = 8765432.10m,
            ProfitMargin = 0.2345m,
            InternationalRevenue = new Dictionary<string, decimal>
            {
                ["USA"] = 5000000m,
                ["EUR"] = 4000000m,
                ["JPY"] = 3345678m
            },
            QuarterlyData = new[]
            {
                new QuarterlyData { Quarter = "Q1", Value = 3000000m },
                new QuarterlyData { Quarter = "Q2", Value = 3500000m },
                new QuarterlyData { Quarter = "Q3", Value = 4000000m },
                new QuarterlyData { Quarter = "Q4", Value = 4500000m }
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // preserve literal €, ¥, etc.
        };
        options.Converters.Add(new CurrencyDictionaryConverter());

        // Act
        var json = JsonSerializer.Serialize(report, options);

        // Assert – using camelCase property names and literal symbols
        Assert.Contains("\"revenue\": \"$12.35M\"", json);
        Assert.Contains("\"expenses\": \"$8.77M\"", json);      // 8.7654321 → 8.77M (away-from-zero)
        Assert.Contains("\"profitMargin\": \"0.23\"", json);

        // dictionary keys are case‑sensitive; "USA" becomes "usa" due to naming policy?
        // No, dictionary keys are not transformed.
        // Wait, dictionary keys are strings, not properties – they remain exactly as given.
        // So "USA" stays "USA". The test originally had "USA" – we'll keep that.
        Assert.Contains("\"USA\": \"$5.00M\"", json);
        // dictionary keys are not affected by naming policy
        Assert.Contains("\"EUR\": \"€4.00M\"", json);
        Assert.Contains("\"JPY\": \"¥3.35M\"", json);
        Assert.Contains("\"value\": \"3.00M\"", json);
    }

    [Fact]
    public void Deserialize_WithFormattedJson_ReadsOriginalValues()
    {
        // Arrange – JSON with formatted numbers
        var json = @"{
            ""revenue"": ""$12.35M"",
            ""expenses"": ""$8.77M"",
            ""profitMargin"": ""0.23""
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        options.Converters.Add(new CurrencyDictionaryConverter());
        options.Converters.Add(new ShortNumberJsonConverterFactory());

        // Act
        var report = JsonSerializer.Deserialize<SimpleFinancialReport>(json, options);

        // Assert – the converter now parses the formatted strings
        Assert.NotNull(report);
        Assert.Equal(12_350_000m, report!.Revenue);      // 12.35M → 12,350,000
        Assert.Equal(8_770_000m, report.Expenses);       // 8.77M → 8,770,000
        Assert.Equal(0.23m, report.ProfitMargin);        // 0.23 → 0.23
    }

    [Fact]
    public void ShortNumberJsonConverterFactory_HandlesAllNumericTypes()
    {
        // Arrange
        var model = new AllNumericTypesModel
        {
            DecimalValue = 1234567.89m,
            DoubleValue = 1234567.89,
            FloatValue = 1234567.89f,
            IntValue = 1234567,
            LongValue = 1234567890L,
            ShortValue = 1234,
            ByteValue = 255,
            NullableDecimal = 1234567.89m,      // <-- uncommented
            NullableInt = 1234567,               // <-- uncommented
            UIntValue = 1234567u,
            ULongValue = 1234567890uL
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new ShortNumberJsonConverterFactory() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);
        var deserialized = JsonSerializer.Deserialize<AllNumericTypesModel>(json, options);

        // Assert – serialized strings
        Assert.Contains("\"decimalValue\":\"1.23M\"", json);
        Assert.Contains("\"doubleValue\":\"1.23M\"", json);
        Assert.Contains("\"floatValue\":\"1.23M\"", json);
        Assert.Contains("\"intValue\":\"1.23M\"", json);
        Assert.Contains("\"longValue\":\"1.23B\"", json);
        Assert.Contains("\"shortValue\":\"1.23K\"", json);
        Assert.Contains("\"nullableDecimal\":\"1.23M\"", json);
        Assert.Contains("\"nullableInt\":\"1.23M\"", json);
        Assert.Contains("\"byteValue\":\"255.00\"", json);

        // Verify deserialization
        Assert.NotNull(deserialized);
        Assert.Equal(1_230_000m, deserialized!.DecimalValue);
        Assert.Equal(1_230_000, deserialized.DoubleValue, 0.01);
        Assert.Equal(1_230_000f, deserialized.FloatValue, 0.01);
        Assert.Equal(1_230_000, deserialized.IntValue);
        Assert.Equal(1_230_000_000L, deserialized.LongValue);
        Assert.Equal(1230, deserialized.ShortValue);
        Assert.Equal(1_230_000m, deserialized.NullableDecimal);
        Assert.Equal(1_230_000, deserialized.NullableInt);
        Assert.Equal(255, deserialized.ByteValue);
    }

    // Test models (unchanged)
    private class AllNumericTypesModel
    {
        public decimal DecimalValue { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public short ShortValue { get; set; }
        public byte ByteValue { get; set; }
        public decimal? NullableDecimal { get; set; }
        public int? NullableInt { get; set; }
        public uint UIntValue { get; set; }
        public ulong ULongValue { get; set; }
    }

    private class FinancialReport
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        [ShortNumberFormat(isCurrency: true)]
        public decimal Revenue { get; set; }

        [ShortNumberFormat(isCurrency: true)]
        public decimal Expenses { get; set; }

        [ShortNumberFormat(decimalPlaces: 2)]
        public decimal ProfitMargin { get; set; }

        public Dictionary<string, decimal> InternationalRevenue { get; set; } = new();

        public QuarterlyData[] QuarterlyData { get; set; } = Array.Empty<QuarterlyData>();
    }

    private class QuarterlyData
    {
        public string Quarter { get; set; } = string.Empty;

        [ShortNumberFormat]
        public decimal Value { get; set; }
    }

    private class SimpleFinancialReport
    {
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}