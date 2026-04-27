using System.Text.Json;
using System.Text.Json.Serialization;
using HumanNumbers.AspNetCore.Serialization;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Attribute to exempt a property from automatic human-readable formatting during JSON serialization.
/// This attribute overrides the global HumanNumber converter factory.
/// </summary>
public class NoHumanFormatAttribute : JsonConverterAttribute
{
    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? typeToConvert;

        if (isNullable)
        {
            var converterType = typeof(NumericPassthroughNullableConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
        else
        {
            var converterType = typeof(NumericPassthroughConverter<>).MakeGenericType(actualType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }
}
