using NumberFormatter;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// JSON converter for serializing a dictionary of decimal values into structured currency strings.
/// </summary>
public class CurrencyDictionaryConverter : JsonConverter<Dictionary<string, decimal>>
{
    private readonly int _decimalPlaces;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyDictionaryConverter"/> class.
    /// </summary>
    /// <param name="decimalPlaces">Number of decimal places for formatting.</param>
    public CurrencyDictionaryConverter(int decimalPlaces = 2)
    {
        _decimalPlaces = decimalPlaces;
    }

    /// <inheritdoc />
    public override Dictionary<string, decimal> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialize as a normal dictionary (the converter is for writing only, or we can implement reading as well)
        var dict = new Dictionary<string, decimal>();
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dict;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string key = reader.GetString() ?? string.Empty;
            reader.Read();

            // The value might be a string (formatted) or a number
            if (reader.TokenType == JsonTokenType.String)
            {
                // Parse the formatted string back to decimal (using your existing logic)
                var str = reader.GetString() ?? string.Empty;
                var parsed = ParseFormattedNumber(str);  // Reuse your parser
                dict[key] = parsed;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                dict[key] = reader.GetDecimal();
            }
            else
            {
                throw new JsonException();
            }
        }
        return dict;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<string, decimal> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            // Determine currency based on key (custom mapping)
            string currencyCode = MapKeyToCurrencyCode(kvp.Key);
            string formatted = kvp.Value.ToShortCurrencyString(currencyCode, _decimalPlaces);
            writer.WriteStringValue(formatted);
        }
        writer.WriteEndObject();
    }

    private string MapKeyToCurrencyCode(string key)
    {
        // Define your mapping from dictionary keys to ISO currency codes
        return key switch
        {
            "USA" => "USD",
            "EUR" => "EUR",
            "JPY" => "JPY",
            "GBP" => "GBP",
            _ => "USD" // fallback
        };
    }

    private decimal ParseFormattedNumber(string value)
    {
        if (NumberFormatter.TryParse(value, out var parsedValue))
        {
            return parsedValue;
        }
        return 0;
    }
}