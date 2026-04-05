using System;
using System.Text.Json.Serialization;

namespace NumberFormatter.AspNetCore.Financial;

/// <summary>
/// Applies basis points JSON conversion specifically to the decorated property.
/// Works natively without global service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BasisPointsAttribute : JsonConverterAttribute
{
    public int Decimals { get; set; } = 0;
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;
    public bool WriteAsString { get; set; } = true;

    /// <summary>
    /// Acts as the internal JsonConverterFactory for System.Text.Json.
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        var options = new BasisPointJsonOptions
        {
            Decimals = Decimals,
            Rounding = Rounding,
            WriteAsString = WriteAsString
        };
        var baseConverter = new BasisPointsJsonConverter(options);
        return ConverterFactoryHelper.CreateGenericWrapper(typeToConvert, baseConverter);
    }
}
