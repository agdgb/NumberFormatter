using System;
using Microsoft.Extensions.DependencyInjection;
using HumanNumbers.AspNetCore.Financial;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// Options for configuring financial-specific JSON serialization.
/// </summary>
public sealed class HumanFinancialJsonOptions
{
    /// <summary>
    /// Gets or sets the options for basis points formatting.
    /// </summary>
    public BasisPointJsonOptions BasisPoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the options for fractional price formatting.
    /// </summary>
    public FractionJsonOptions Fractions { get; set; } = new();
}

/// <summary>
/// Extension methods for registering financial formatters in a HumanNumbers application.
/// </summary>
public static class HumanFinancialExtensions
{
    /// <summary>
    /// Adds financial-specific JSON converters (BasisPoints, FractionPrice) to the MVC JSON options.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="configure">Optional action to configure the financial JSON options.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddHumanFinancialFormatters(this IMvcBuilder builder, Action<HumanFinancialJsonOptions>? configure = null)
    {
        var options = new HumanFinancialJsonOptions();
        configure?.Invoke(options);

        builder.AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new BasisPointsJsonConverter(options.BasisPoints));
            opts.JsonSerializerOptions.Converters.Add(new FractionPriceJsonConverter(options.Fractions));
        });

        return builder;
    }

    /// <summary>
    /// Adds financial-specific JSON converters (BasisPoints, FractionPrice) to the minimal API JSON options.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Optional action to configure the financial JSON options.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddHumanFinancialFormatters(this IServiceCollection services, Action<HumanFinancialJsonOptions>? configure = null)
    {
        var options = new HumanFinancialJsonOptions();
        configure?.Invoke(options);

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
        {
            opts.SerializerOptions.Converters.Add(new BasisPointsJsonConverter(options.BasisPoints));
            opts.SerializerOptions.Converters.Add(new FractionPriceJsonConverter(options.Fractions));
        });

        return services;
    }
}
