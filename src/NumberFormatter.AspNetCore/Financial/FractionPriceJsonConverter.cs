using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NumberFormatter.Financial;

namespace NumberFormatter.AspNetCore.Financial;

public sealed class FractionJsonOptions
{
    public int Denominator { get; set; } = 32;
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;
}

public sealed class FractionPriceJsonConverter : JsonConverter<decimal>
{
    private readonly FractionJsonOptions _options;

    public FractionPriceJsonConverter()
    {
        _options = new FractionJsonOptions();
    }

    public FractionPriceJsonConverter(FractionJsonOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue)) return 0m;
            
            if (FinancialRounding.TryParseFraction(stringValue, out var result))
            {
                return result;
            }
            throw new JsonException($"Unable to parse \"{stringValue}\" as fractional price.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetDecimal(out var numberValue))
            {
                // Number represents the raw decimal price directly.
                return numberValue;
            }
        }

        throw new JsonException($"Unexpected token parsing fractional price. Expected String or Number, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        var stringValue = value.ToFractionString(_options.Denominator, _options.Rounding);
        writer.WriteStringValue(stringValue);
    }
}
