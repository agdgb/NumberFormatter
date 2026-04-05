using System;
using System.Text.Json.Serialization;

namespace NumberFormatter.AspNetCore.Financial;

/// <summary>
/// Applies fractional price JSON conversion specifically to the decorated property.
/// Works natively without global service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FractionPriceAttribute : JsonConverterAttribute
{
    public int Denominator { get; set; } = 32;
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>
    /// Instantiates the attribute with an optional fraction denominator (e.g. 32).
    /// </summary>
    public FractionPriceAttribute(int denominator)
    {
        Denominator = denominator;
    }

    public FractionPriceAttribute()
    {
    }

    /// <summary>
    /// Acts as the internal JsonConverterFactory for System.Text.Json.
    /// </summary>
    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        var options = new FractionJsonOptions
        {
            Denominator = Denominator,
            Rounding = Rounding
        };

        var baseConverter = new FractionPriceJsonConverter(options);
        return ConverterFactoryHelper.CreateGenericWrapper(typeToConvert, baseConverter);
    }
}
