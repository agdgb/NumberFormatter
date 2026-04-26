using System;
using System.Text.Json.Serialization;
using HumanNumbers.AspNetCore.Serialization;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Defines how the <see cref="HumanNumberAttribute"/> affects the numeric output.
/// </summary>
public enum HumanNumberOutputMode
{
    /// <summary>
    /// Act as a metadata marker only. Standard numeric serialization is preserved in JSON.
    /// This is the default mode (Safe-First).
    /// </summary>
    FormatOnly,

    /// <summary>
    /// Automatically applies a custom JSON converter to transform the numeric value 
    /// into a human-readable string during serialization.
    /// </summary>
    SerializeAsHuman
}

/// <summary>
/// Attribute to apply human-readable number formatting to properties.
/// Works as a metadata marker for API filters and can optionally override JSON serialization.
/// This attribute does NOT inherit from <see cref="JsonConverterAttribute"/> to ensure 
/// zero interference with standard serialization unless explicitly opted-in via TypeInfo modifiers.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class HumanNumberAttribute : Attribute
{
    /// <summary>Number of decimal places to use for this property.</summary>
    public int DecimalPlaces { get; set; } = 2;
    
    /// <summary>The name of the policy to use for formatting this property.</summary>
    public string? PolicyName { get; set; }

    /// <summary>Whether to format the value as a currency.</summary>
    public bool IsCurrency { get; set; }

    /// <summary>The currency code to use if <see cref="IsCurrency"/> is true.</summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Controls whether this attribute triggers a custom JSON converter.
    /// Default is <see cref="HumanNumberOutputMode.FormatOnly"/>.
    /// </summary>
    public HumanNumberOutputMode OutputMode { get; set; } = HumanNumberOutputMode.FormatOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberAttribute"/> class.
    /// </summary>
    public HumanNumberAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberAttribute"/> class with a specific number of decimal places.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places.</param>
    public HumanNumberAttribute(int decimalPlaces)
    {
        DecimalPlaces = decimalPlaces;
    }

    /// <summary>
    /// Internal helper to create the appropriate JSON converter for this attribute configuration.
    /// </summary>
    internal JsonConverter? CreateConverter(Type typeToConvert)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? typeToConvert;

        var options = new HumanNumbers.Formatting.HumanNumberFormatOptions();
        
        if (!string.IsNullOrEmpty(PolicyName))
        {
            options.UsingPolicy(PolicyName);
        }
        else
        {
            options.DecimalPlaces = DecimalPlaces;
        }

        if (IsCurrency)
        {
            options.CurrencySymbol = !string.IsNullOrEmpty(CurrencyCode) 
                ? HumanNumbers.Currencies.CurrencyRegistry.GetSymbol(CurrencyCode) 
                : "$";
        }

        if (isNullable)
        {
            var converterType = typeof(HumanNumberNullableJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType, options, OutputMode)!;
        }
        else if (IsNumericType(actualType))
        {
            var converterType = typeof(HumanNumberJsonConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType, options, OutputMode)!;
        }
        else if (TryGetCollectionElementType(typeToConvert, out var elementType))
        {
            var converterType = typeof(HumanNumberCollectionConverter<,>).MakeGenericType(elementType, typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType, options)!;
        }

        return null;
    }

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

    private static bool TryGetCollectionElementType(Type type, out Type elementType)
    {
        elementType = null!;
        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return IsNumericType(elementType);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>))
        {
            elementType = type.GetGenericArguments()[0];
            return IsNumericType(elementType);
        }

        var enumerableType = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>));

        if (enumerableType != null)
        {
            elementType = enumerableType.GetGenericArguments()[0];
            return IsNumericType(elementType);
        }

        return false;
    }
}

/// <summary>
/// Obsolete alias for <see cref="HumanNumberAttribute"/>.
/// </summary>
[Obsolete("Use HumanNumberAttribute instead. This alias will be removed in v3.0.")]
public sealed class ShortNumberFormatAttribute : HumanNumberAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberFormatAttribute"/> class.
    /// </summary>
    public ShortNumberFormatAttribute() : base() 
    {
        OutputMode = HumanNumberOutputMode.SerializeAsHuman;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortNumberFormatAttribute"/> class with a specific number of decimal places.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal places.</param>
    public ShortNumberFormatAttribute(int decimalPlaces) : base(decimalPlaces) 
    {
        OutputMode = HumanNumberOutputMode.SerializeAsHuman;
    }
}
