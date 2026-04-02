using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// JSON converter factory for short number formatting
/// </summary>
public class ShortNumberJsonConverterFactory : JsonConverterFactory
{
    private readonly int _defaultDecimalPlaces;
    private readonly bool _defaultIsCurrency;
    private readonly string? _defaultCurrencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberJsonConverterFactory"/> class.
    /// </summary>
    /// <param name="defaultDecimalPlaces">The default number of decimal places to include.</param>
    /// <param name="defaultIsCurrency">Whether to format as currency by default.</param>
    /// <param name="defaultCurrencyCode">The default currency code.</param>
    public ShortNumberJsonConverterFactory(
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
        // Handle nullable types
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

    //public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    //{
    //    var underlyingType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

    //    // Create the appropriate converter based on the type
    //    var converterType = typeof(ShortNumberJsonConverter<>).MakeGenericType(underlyingType);
    //    return (JsonConverter)Activator.CreateInstance(
    //        converterType, 
    //        _defaultDecimalPlaces, 
    //        _defaultIsCurrency, 
    //        _defaultCurrencyCode)!;
    //}

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? typeToConvert;

        // Ensure the actual type is numeric (should be, because CanConvert returned true)
        if (!IsNumericType(actualType))
            return null;

        if (isNullable)
        {
            // Create the nullable converter for the underlying type
            var converterType = typeof(ShortNumberNullableJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _defaultDecimalPlaces,
                _defaultIsCurrency,
                _defaultCurrencyCode)!;
        }
        else
        {
            // Create the non‑nullable converter
            var converterType = typeof(ShortNumberJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _defaultDecimalPlaces,
                _defaultIsCurrency,
                _defaultCurrencyCode)!;
        }
    }
}


/// <summary>
/// Generic JSON converter for short number formatting
/// </summary>
public class ShortNumberJsonConverter<T> : JsonConverter<T> where T : struct, INumber<T>
{
    private readonly int _decimalPlaces;
    private readonly bool _isCurrency;
    private readonly string? _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberJsonConverter{T}"/> class.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places to include.</param>
    /// <param name="isCurrency">Whether the value represents currency.</param>
    /// <param name="currencyCode">The optional currency code.</param>
    public ShortNumberJsonConverter(
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

            var parsedValue = ParseFormattedNumber(stringValue);
            return T.CreateChecked(parsedValue);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            // Direct number token – read according to the target type
            return T.CreateChecked(reader.GetDecimal());
        }

        return T.Zero;
    }

    private decimal ParseFormattedNumber(string value)
    {
        if (NumberFormatter.TryParse(value, out var parsedValue))
        {
            return parsedValue;
        }
        return 0;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var decimalValue = Convert.ToDecimal(value);

        string formatted;
        if (_isCurrency)
        {
            formatted = _currencyCode != null
                ? decimalValue.ToShortCurrencyString(_currencyCode, _decimalPlaces)
                : decimalValue.ToShortCurrencyString(_decimalPlaces);
        }
        else
        {
            formatted = decimalValue.ToShortString(_decimalPlaces);
        }

        writer.WriteStringValue(formatted);
    }
}

/// <summary>
/// Attribute to apply short number formatting to properties
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ShortNumberFormatAttribute : JsonConverterAttribute
{
    private readonly int _decimalPlaces;
    private readonly bool _isCurrency;
    private readonly string? _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberFormatAttribute"/> class.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places to include.</param>
    /// <param name="isCurrency">Whether the value represents currency.</param>
    /// <param name="currencyCode">The optional currency code.</param>
    public ShortNumberFormatAttribute(int decimalPlaces = 2, bool isCurrency = false, string? currencyCode = null)
    {
        _decimalPlaces = decimalPlaces;
        _isCurrency = isCurrency;
        _currencyCode = currencyCode;
    }

    //public override JsonConverter? CreateConverter(Type typeToConvert)
    //{
    //    var underlyingType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
    //    var converterType = typeof(ShortNumberJsonConverter<>).MakeGenericType(underlyingType);
    //    return (JsonConverter)Activator.CreateInstance(
    //        converterType, 
    //        _decimalPlaces, 
    //        _isCurrency, 
    //        _currencyCode)!;
    //}
    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? typeToConvert;

        if (isNullable)
        {
            var converterType = typeof(ShortNumberNullableJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _decimalPlaces,
                _isCurrency,
                _currencyCode)!;
        }
        else
        {
            var converterType = typeof(ShortNumberJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(
                converterType,
                _decimalPlaces,
                _isCurrency,
                _currencyCode)!;
        }
    }
}

/// <summary>
/// Attribute to apply short number formatting to all numeric properties in a class
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ShortNumberFormatGloballyAttribute : JsonConverterAttribute
{
    private readonly int _decimalPlaces;
    private readonly bool _isCurrency;
    private readonly string? _currencyCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberFormatGloballyAttribute"/> class.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places to include.</param>
    /// <param name="isCurrency">Whether the value represents currency.</param>
    /// <param name="currencyCode">The optional currency code.</param>
    public ShortNumberFormatGloballyAttribute(int decimalPlaces = 2, bool isCurrency = false, string? currencyCode = null)
    {
        _decimalPlaces = decimalPlaces;
        _isCurrency = isCurrency;
        _currencyCode = currencyCode;
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        return new ShortNumberJsonConverterFactory(_decimalPlaces, _isCurrency, _currencyCode);
    }
}