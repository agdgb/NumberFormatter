using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using HumanNumbers;

namespace HumanNumbers.AspNetCore.Serialization;

/// <summary>
/// JSON converter factory for human-readable number formatting.
/// </summary>
public class HumanNumberJsonConverterFactory : JsonConverterFactory
{
    private readonly int _defaultDecimalPlaces;
    private readonly bool _defaultIsCurrency;
    private readonly string? _defaultCurrencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberJsonConverterFactory"/> class.
    /// </summary>
    /// <param name="defaultDecimalPlaces">The default number of decimal places to include.</param>
    /// <param name="defaultIsCurrency">Whether to format as currency by default.</param>
    /// <param name="defaultCurrencyCode">The default currency code.</param>
    public HumanNumberJsonConverterFactory(
        int defaultDecimalPlaces = 2,
        bool defaultIsCurrency = false,
        string? defaultCurrencyCode = null)
    {
        _defaultDecimalPlaces = defaultDecimalPlaces;
        _defaultIsCurrency = defaultIsCurrency;
        _defaultCurrencyCode = defaultCurrencyCode;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return IsNumericType(typeToConvert);
    }

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(decimal) ||
               underlyingType == typeof(double) ||
               underlyingType == typeof(float) ||
               underlyingType == typeof(int) ||
               underlyingType == typeof(long) ||
               underlyingType == typeof(short) ||
               underlyingType == typeof(byte) ||
               underlyingType == typeof(uint) ||
               underlyingType == typeof(ulong) ||
               underlyingType == typeof(ushort) ||
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
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _defaultDecimalPlaces,
                _defaultIsCurrency,
                _defaultCurrencyCode)!;
        }
        else
        {
            var converterType = typeof(HumanNumberJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _defaultDecimalPlaces,
                _defaultIsCurrency,
                _defaultCurrencyCode)!;
        }
    }
}

/// <summary>
/// Generic JSON converter for human-readable number formatting.
/// </summary>
public class HumanNumberJsonConverter<T> : JsonConverter<T> where T : struct, INumber<T>
{
    private readonly int _decimalPlaces;
    private readonly bool _isCurrency;
    private readonly string? _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberJsonConverter{T}"/> class.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places.</param>
    /// <param name="isCurrency">Whether to format as currency.</param>
    /// <param name="currencyCode">The currency code.</param>
    public HumanNumberJsonConverter(
        int decimalPlaces = 2,
        bool isCurrency = false,
        string? currencyCode = null)
    {
        _decimalPlaces = decimalPlaces;
        _isCurrency = isCurrency;
        _currencyCode = currencyCode;
    }

    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return T.Zero;

            if (HumanNumber.TryParse(stringValue, out var parsedValue))
            {
                return T.CreateChecked(parsedValue);
            }
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            return T.CreateChecked(reader.GetDecimal());
        }

        return T.Zero;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        string formatted;
        if (_isCurrency)
        {
            if (string.IsNullOrEmpty(_currencyCode))
            {
                // P2 Cleanup: Handle incomplete currency configuration
                if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict)
                {
                    throw new InvalidOperationException("CurrencyCode must be provided when IsCurrency is true in Strict mode.");
                }

                formatted = value.ToHumanCurrency(_decimalPlaces);
            }
            else
            {
                formatted = value.ToHumanCurrency(_currencyCode, _decimalPlaces);
            }
        }
        else
        {
            formatted = value.ToHuman(_decimalPlaces);
        }

        writer.WriteStringValue(formatted);
    }
}

/// <summary>
/// JSON converter for nullable numeric types using human-readable formatting.
/// </summary>
public class HumanNumberNullableJsonConverter<T> : JsonConverter<T?> where T : struct, INumber<T>
{
    private readonly HumanNumberJsonConverter<T> _innerConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberNullableJsonConverter{T}"/> class.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places.</param>
    /// <param name="isCurrency">Whether to format as currency.</param>
    /// <param name="currencyCode">The currency code.</param>
    public HumanNumberNullableJsonConverter(int decimalPlaces = 2, bool isCurrency = false, string? currencyCode = null)
    {
        _innerConverter = new HumanNumberJsonConverter<T>(decimalPlaces, isCurrency, currencyCode);
    }

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return _innerConverter.Read(ref reader, typeof(T), options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        _innerConverter.Write(writer, value.Value, options);
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