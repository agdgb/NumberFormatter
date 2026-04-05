using System;
using Microsoft.Extensions.DependencyInjection;

namespace NumberFormatter.AspNetCore.Financial;

public sealed class FinancialJsonOptions
{
    public BasisPointJsonOptions BasisPoints { get; set; } = new();
    public FractionJsonOptions Fractions { get; set; } = new();
}

public static class FinancialExtensions
{
    public static IMvcBuilder AddFinancialFormatters(this IMvcBuilder builder, Action<FinancialJsonOptions>? configure = null)
    {
        var options = new FinancialJsonOptions();
        configure?.Invoke(options);

        builder.AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new BasisPointsJsonConverter(options.BasisPoints));
            opts.JsonSerializerOptions.Converters.Add(new FractionPriceJsonConverter(options.Fractions));
        });

        return builder;
    }
    
    public static IServiceCollection AddFinancialFormatters(this IServiceCollection services, Action<FinancialJsonOptions>? configure = null)
    {
        var options = new FinancialJsonOptions();
        configure?.Invoke(options);

        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
        {
            opts.SerializerOptions.Converters.Add(new BasisPointsJsonConverter(options.BasisPoints));
            opts.SerializerOptions.Converters.Add(new FractionPriceJsonConverter(options.Fractions));
        });

        return services;
    }
}
