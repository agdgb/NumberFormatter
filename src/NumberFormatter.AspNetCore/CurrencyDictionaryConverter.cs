using NumberFormatter;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NumberFormatter.AspNetCore;

public class CurrencyDictionaryConverter : JsonConverter<Dictionary<string, decimal>>
{
    private readonly int _decimalPlaces;
    private static readonly string[] CurrencySymbols = { "$", "€", "£", "¥", "₹", "Br" };
    // Suffix multipliers (case-insensitive)
    public static readonly Dictionary<string, decimal> SuffixMultipliers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["K"] = 1000m,
        ["M"] = 1000000m,
        ["B"] = 1000000000m,
        ["T"] = 1000000000000m,
        ["Qa"] = 1000000000000000m,
        ["Qi"] = 1000000000000000000m
    };

    public CurrencyDictionaryConverter(int decimalPlaces = 2)
    {
        _decimalPlaces = decimalPlaces;
    }

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

            string key = reader.GetString();
            reader.Read();

            // The value might be a string (formatted) or a number
            if (reader.TokenType == JsonTokenType.String)
            {
                // Parse the formatted string back to decimal (using your existing logic)
                var str = reader.GetString();
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
        // Trim and remove currency symbols
        var cleanValue = value.Trim();
        foreach (var symbol in CurrencySymbols)
        {
            if (cleanValue.StartsWith(symbol))
            {
                cleanValue = cleanValue.Substring(symbol.Length).TrimStart();
                break;
            }
        }

        // Use regex to extract number and optional suffix
        // Pattern: optional sign, digits, optional decimal point and digits, optional whitespace, optional suffix
        var match = Regex.Match(cleanValue, @"^([+-]?\d+\.?\d*)\s*([KkMmBbTt][aA]?[iI]?)?$");
        if (match.Success)
        {
            var numberPart = match.Groups[1].Value;
            var suffix = match.Groups[2].Value;

            if (decimal.TryParse(numberPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            {
                if (!string.IsNullOrEmpty(suffix) && SuffixMultipliers.TryGetValue(suffix, out var multiplier))
                {
                    return number * multiplier;
                }
                return number; // No suffix
            }
        }

        // Fallback: try to parse the whole string as a plain decimal
        if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var fallback))
        {
            return fallback;
        }

        return 0;
    }
}