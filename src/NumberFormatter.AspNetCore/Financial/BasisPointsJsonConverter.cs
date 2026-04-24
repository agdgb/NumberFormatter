using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HumanNumbers.Financial;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// Options for configuring the <see cref="BasisPointsJsonConverter"/>.
/// </summary>
public sealed class BasisPointJsonOptions
{
    /// <summary>
    /// The number of decimal places to include when formatting as a string. Default is 0.
    /// </summary>
    public int Decimals { get; set; } = 0;

    /// <summary>
    /// The rounding method to use. Default is AwayFromZero.
    /// </summary>
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>
    /// If true, the value is written as a string (e.g. "125 bps"). If false, as a number representing basis points (e.g. 125).
    /// </summary>
    public bool WriteAsString { get; set; } = true;
}

/// <summary>
/// A JSON converter that translates between decimal values (e.g. 0.0125) and their basis points representation (e.g. "125 bps").
/// </summary>
public sealed class BasisPointsJsonConverter : JsonConverter<decimal>
{
    private readonly BasisPointJsonOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasisPointsJsonConverter"/> class with default options.
    /// </summary>
    public BasisPointsJsonConverter()
    {
        _options = new BasisPointJsonOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasisPointsJsonConverter"/> class with specific options.
    /// </summary>
    /// <param name="options">Configuration options for the converter.</param>
    public BasisPointsJsonConverter(BasisPointJsonOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (BasisPointFormatter.TryParseBps(stringValue, out var parsedValue))
            {
                return parsedValue;
            }
            throw new JsonException($"Unable to parse \"{stringValue}\" as basis points.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetDecimal(out var numberValue))
            {
                return numberValue / 10000m;
            }
        }

        throw new JsonException($"Unexpected token parsing basis points. Expected String or Number, got {reader.TokenType}.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        if (_options.WriteAsString)
        {
            var stringValue = value.ToHumanBps(_options.Decimals, _options.Rounding);
            writer.WriteStringValue(stringValue);
        }
        else
        {
            var bpsValue = value.ToBps();
            var roundedBpsValue = Math.Round(bpsValue, _options.Decimals, _options.Rounding);
            writer.WriteNumberValue(roundedBpsValue);
        }
    }
}
