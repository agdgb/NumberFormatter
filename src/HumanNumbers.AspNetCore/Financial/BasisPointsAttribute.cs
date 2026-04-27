using System;
using System.Text.Json.Serialization;
using HumanNumbers.Financial;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// Applies basis points JSON conversion specifically to the decorated property.
/// Works natively without global service registration.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BasisPointsAttribute : JsonConverterAttribute
{
    /// <summary>
    /// The number of decimal places to include when formatting as a string. Default is 0.
    /// </summary>
    public int Decimals { get; set; } = 0;

    /// <summary>
    /// The rounding method to use. Default is AwayFromZero.
    /// </summary>
    public MidpointRounding Rounding { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>
    /// If true, the value is written as a string (e.g. "125 bps"). If false, as a number representing basis points (e.g. 125).
    /// </summary>
    public bool WriteAsString { get; set; } = true;

    /// <inheritdoc />
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
