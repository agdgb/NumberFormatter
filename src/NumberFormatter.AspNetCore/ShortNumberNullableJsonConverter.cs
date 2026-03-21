using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// JSON converter for nullable numeric types using short number formatting.
/// </summary>
public class ShortNumberNullableJsonConverter<T> : JsonConverter<T?> where T : struct, INumber<T>
{
    private readonly ShortNumberJsonConverter<T> _innerConverter;

    public ShortNumberNullableJsonConverter(int decimalPlaces = 2, bool isCurrency = false, string? currencyCode = null)
    {
        _innerConverter = new ShortNumberJsonConverter<T>(decimalPlaces, isCurrency, currencyCode);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Delegate to the inner converter (which handles the non‑null value)
        return _innerConverter.Read(ref reader, typeof(T), options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        // Delegate to the inner converter
        _innerConverter.Write(writer, value.Value, options);
    }
}