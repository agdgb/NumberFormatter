using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HumanNumbers.Financial;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// Options for configuring the <see cref="FractionPriceJsonConverter"/>.
/// </summary>
public sealed class FractionJsonOptions
{
    /// <summary>
    /// The denominator to use for fractional representation (e.g. 32 for Treasury bonds). Default is 32.
    /// </summary>
    public int Denominator { get; set; } = 32;

    /// <summary>
    /// The rounding method to use. Default is AwayFromZero.
    /// </summary>
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;
}

/// <summary>
/// A JSON converter that translates between decimal values (e.g. 101.5) and their fractional price representation (e.g. "101 16/32").
/// </summary>
public sealed class FractionPriceJsonConverter : JsonConverter<decimal>
{
    private readonly FractionJsonOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionPriceJsonConverter"/> class with default options.
    /// </summary>
    public FractionPriceJsonConverter()
    {
        _options = new FractionJsonOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionPriceJsonConverter"/> class with specific options.
    /// </summary>
    /// <param name="options">Configuration options for the converter.</param>
    public FractionPriceJsonConverter(FractionJsonOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        var stringValue = value.ToHumanFraction(_options.Denominator, _options.Rounding);
        writer.WriteStringValue(stringValue);
    }
}
