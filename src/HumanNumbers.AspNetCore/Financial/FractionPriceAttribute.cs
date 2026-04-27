using System;
using System.Text.Json.Serialization;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// Applies fractional price JSON conversion specifically to the decorated property.
/// Works natively without global service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FractionPriceAttribute : JsonConverterAttribute
{
    /// <summary>
    /// The denominator to use for fractional representation (e.g. 32). Default is 32.
    /// </summary>
    public int Denominator { get; set; } = 32;

    /// <summary>
    /// The rounding method to use. Default is AwayFromZero.
    /// </summary>
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionPriceAttribute"/> class with a specific denominator.
    /// </summary>
    /// <param name="denominator">The denominator to use.</param>
    public FractionPriceAttribute(int denominator)
    {
        Denominator = denominator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionPriceAttribute"/> class with default options.
    /// </summary>
    public FractionPriceAttribute()
    {
    }

    /// <inheritdoc />
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
