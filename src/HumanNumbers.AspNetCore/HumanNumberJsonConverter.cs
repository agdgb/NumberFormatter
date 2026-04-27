using HumanNumbers;
using HumanNumbers.Formatting;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;

namespace HumanNumbers.AspNetCore.Serialization;

/// <summary>
/// JSON converter factory for human-readable number formatting.
/// </summary>
public class HumanNumberJsonConverterFactory : JsonConverterFactory
{
    private readonly HumanNumberFormatOptions _options;
    private readonly HumanNumberOutputMode _mode;

    /// <summary>Initializes the factory with the given <see cref="HumanNumberFormatOptions"/>.</summary>
    public HumanNumberJsonConverterFactory(HumanNumberFormatOptions options, HumanNumberOutputMode mode = HumanNumberOutputMode.SerializeAsHuman)
    {
        _options = options;
        _mode = mode;
    }

    /// <summary>Initializes the factory with a default decimal places setting.</summary>
    public HumanNumberJsonConverterFactory(int defaultDecimalPlaces = 2)
    {
        _options = new HumanNumberFormatOptions { DecimalPlaces = defaultDecimalPlaces };
        _mode = HumanNumberOutputMode.SerializeAsHuman;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => IsNumericType(typeToConvert);

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float) || underlyingType == typeof(int) ||
               underlyingType == typeof(long) || underlyingType == typeof(short) ||
               underlyingType == typeof(byte) || underlyingType == typeof(uint) ||
               underlyingType == typeof(ulong) || underlyingType == typeof(ushort) ||
               underlyingType == typeof(sbyte);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? typeToConvert;

        if (isNullable)
        {
            var converterType = typeof(HumanNumberNullableJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType, _options, _mode)!;
        }
        else
        {
            var converterType = typeof(HumanNumberJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType, _options, _mode)!;
        }
    }
}

/// <summary>JSON converter that serializes a numeric value as a human-readable string and deserializes back.</summary>
public class HumanNumberJsonConverter<T> : JsonConverter<T> where T : struct, INumber<T>
{
    private readonly HumanNumberFormatOptions _options;
    private readonly HumanNumberOutputMode _mode;

    /// <summary>Initializes the converter with the given options.</summary>
    public HumanNumberJsonConverter(HumanNumberFormatOptions options, HumanNumberOutputMode mode = HumanNumberOutputMode.SerializeAsHuman)
    {
        _options = options;
        _mode = mode;
    }

    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue)) return T.Zero;
            if (HumanNumber.TryParse(stringValue, out var parsedValue)) return T.CreateChecked(parsedValue);
        }

        if (reader.TokenType == JsonTokenType.Number) return T.CreateChecked(reader.GetDecimal());
        return T.Zero;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var decimalValue = decimal.CreateChecked(value);

        if (_mode == HumanNumberOutputMode.FormatOnly)
        {
            writer.WriteNumberValue(decimalValue);
            return;
        }

        string formatted;

        if (_options.CurrencySymbol != null)
        {
            formatted = decimalValue.ToHumanCurrency(_options);
        }
        else
        {
            formatted = decimalValue.ToHuman(_options);
        }

        writer.WriteStringValue(formatted);
    }
}

/// <summary>JSON converter that handles nullable numeric values, delegating to <see cref="HumanNumberJsonConverter{T}"/>.</summary>
public class HumanNumberNullableJsonConverter<T> : JsonConverter<T?> where T : struct, INumber<T>
{
    private readonly HumanNumberJsonConverter<T> _innerConverter;

    /// <summary>Initializes the converter with the given options.</summary>
    public HumanNumberNullableJsonConverter(HumanNumberFormatOptions options, HumanNumberOutputMode mode = HumanNumberOutputMode.SerializeAsHuman)
    {
        _innerConverter = new HumanNumberJsonConverter<T>(options, mode);
    }

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null ? null : _innerConverter.Read(ref reader, typeof(T), options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else _innerConverter.Write(writer, value.Value, options);
    }
}


/// <summary>
/// A JSON converter that bypasses HumanNumbers formatting and writes the numeric value normally.
/// </summary>
public class NumericPassthroughConverter<T> : JsonConverter<T> where T : struct, INumber<T>
{
    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
        T.CreateChecked(reader.GetDecimal());

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => 
        writer.WriteNumberValue(decimal.CreateChecked(value));
}

/// <summary>
/// A JSON converter for nullable types that bypasses HumanNumbers formatting.
/// </summary>
public class NumericPassthroughNullableConverter<T> : JsonConverter<T?> where T : struct, INumber<T>
{
    private readonly NumericPassthroughConverter<T> _inner = new();

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
        reader.TokenType == JsonTokenType.Null ? null : _inner.Read(ref reader, typeof(T), options);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else _inner.Write(writer, value.Value, options);
    }
}

/// <summary>
/// A JSON converter that serializes a collection of numeric values as human-readable strings.
/// </summary>
public class HumanNumberCollectionConverter<T, TCollection> : JsonConverter<TCollection>
    where T : struct, INumber<T>
    where TCollection : IEnumerable<T>
{
    private readonly HumanNumberFormatOptions _options;

    /// <summary>Initializes the converter with the given options.</summary>
    public HumanNumberCollectionConverter(HumanNumberFormatOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserializing human-formatted collections is not supported.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var item in value)
        {
            var decimalValue = decimal.CreateChecked(item);
            string formatted = _options.CurrencySymbol != null 
                ? decimalValue.ToHumanCurrency(_options) 
                : decimalValue.ToHuman(_options);
            writer.WriteStringValue(formatted);
        }
        writer.WriteEndArray();
    }
}