using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization.Metadata;
using System.Linq;
using System.Reflection;
using HumanNumbers.AspNetCore.Serialization;
using HumanNumbers.AspNetCore.TagHelpers;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Extension methods for setting up HumanNumbers services in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core HumanNumbers services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddHumanNumbersCore(this IServiceCollection services, Action<HumanNumbersOptions>? configure = null)
    {
        services.AddOptions<HumanNumbersOptions>()
            .Configure(configure ?? (_ => { }))
            .PostConfigure<IServiceProvider>((options, sp) =>
            {
                // Sync core library defaults with ASP.NET Core settings
                HumanNumber.Configure(config =>
                {
                    config.GlobalOptions = options.CoreOptions;
                });

                // Auto-hook logging if enabled
                if (options.EnableLogging)
                {
                    var loggerFactory = sp.GetService<ILoggerFactory>();
                    if (loggerFactory != null)
                    {
                        var logger = loggerFactory.CreateLogger("HumanNumbers");
                        var coreOptions = options.CoreOptions;
                        var originalError = coreOptions.OnFormattingError;
                        
                        // We wrap the existing error handler to add logging
                        coreOptions.OnFormattingError = ex =>
                        {
                            logger.LogError(ex, "HumanNumbers formatting error: {Message}", ex.Message);
                            originalError?.Invoke(ex);
                        };
                        
                        options.CoreOptions = coreOptions;
                        
                        // Push updated options back to core
                        HumanNumber.Configure(config => config.GlobalOptions = options.CoreOptions);
                    }
                }
            });

        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<HumanNumbersOptions>>().Value);
        services.TryAddSingleton<IHumanNumberService, HumanNumberService>();
        
        return services;
    }

    /// <summary>
    /// Configures System.Text.Json to use HumanNumbers converters.
    /// Only properties explicitly marked with <c>[HumanNumber(OutputMode = SerializeAsHuman)]</c> will be formatted.
    /// This is the Safe-First default — no global interference with numeric serialization.
    /// </summary>
    public static IServiceCollection AddHumanNumbersJson(this IServiceCollection services)
    {
        services.AddOptions<JsonOptions>()
            .Configure<HumanNumbersOptions>((options, hnOptions) =>
            {
                // Core goal: Zero Interference. 
                // The TypeInfo modifier is the ONLY mechanism here. It only activates for
                // properties explicitly opted-in via [HumanNumber(OutputMode = SerializeAsHuman)].
                var resolver = options.JsonSerializerOptions.TypeInfoResolver as DefaultJsonTypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
                resolver.Modifiers.Add(CreateHumanNumberModifier);
                options.JsonSerializerOptions.TypeInfoResolver = resolver;
            });
        return services;
    }

    /// <summary>
    /// Configures System.Text.Json to format ALL numeric types as human-readable strings globally.
    /// Use this only when you want blanket formatting across your entire API surface.
    /// For per-property control, use <see cref="AddHumanNumbersJson"/> instead.
    /// </summary>
    public static IServiceCollection AddHumanNumbersJsonGlobal(this IServiceCollection services)
    {
        services.AddHumanNumbersJson(); // Include attribute-driven opt-in
        services.AddOptions<JsonOptions>()
            .Configure<HumanNumbersOptions>((options, hnOptions) =>
            {
                options.JsonSerializerOptions.Converters.Add(new HumanNumberJsonConverterFactory(hnOptions.DefaultDecimalPlaces));
            });
        return services;
    }

    internal static void CreateHumanNumberModifier(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;

        foreach (var property in typeInfo.Properties)
        {
            // P0 Requirement: Conflict Handling. 
            // If a custom converter is already specified (via [JsonConverter]), we respect it and skip.
            if (property.CustomConverter != null) continue;

            // Scoped detection: only look for [HumanNumber] on numeric types we support.
            if (!IsNumericType(property.PropertyType)) continue;

            var attribute = property.AttributeProvider?.GetCustomAttributes(typeof(HumanNumberAttribute), inherit: true)
                .FirstOrDefault() as HumanNumberAttribute;

            if (attribute == null || attribute.OutputMode != HumanNumberOutputMode.SerializeAsHuman)
            {
                // This is TRUE ZERO INTERFERENCE. If it's not opted-in, we do nothing.
                continue;
            }

            // Architecture: Metadata -> Pipeline Augmentation
            property.CustomConverter = attribute.CreateConverter(property.PropertyType);
        }
    }

    internal static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float) || underlyingType == typeof(int) ||
               underlyingType == typeof(long) || underlyingType == typeof(short) ||
               underlyingType == typeof(byte) || underlyingType == typeof(uint) ||
               underlyingType == typeof(ulong) || underlyingType == typeof(ushort) ||
               underlyingType == typeof(sbyte);
    }

    /// <summary>
    /// Configures MVC to use HumanNumbers features.
    /// </summary>
    public static IMvcBuilder AddHumanNumbersMvc(this IMvcBuilder builder)
    {
        // Add automatic formatting filters if enabled
        builder.Services.AddHumanNumbersCore();
        
        return builder;
    }

    /// <summary>
    /// Adds HumanNumbers with a set of opinionated defaults.
    /// </summary>
    public static IServiceCollection AddHumanNumbersDefaults(this IServiceCollection services, Action<HumanNumbersOptions>? configure = null)
    {
        services.AddHumanNumbersCore(configure);
        
        // Setup JSON
        services.AddHumanNumbersJson();

        return services;
    }
}

/// <summary>
/// Service interface for manual number formatting in ASP.NET Core applications.
/// </summary>
public interface IHumanNumberService
{
    /// <summary>Formats a value according to the current policy.</summary>
    string Format(decimal value, int? decimalPlaces = null, string? culture = null);
    /// <summary>Formats a value as currency according to the current policy.</summary>
    string FormatCurrency(decimal value, string? currencyCode = null, int? decimalPlaces = null);
}

/// <summary>
/// Default implementation of the HumanNumbers service.
/// </summary>
public class HumanNumberService : IHumanNumberService
{
    private readonly HumanNumbersOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberService"/> class.
    /// </summary>
    public HumanNumberService(HumanNumbersOptions options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public string Format(decimal value, int? decimalPlaces = null, string? culture = null)
    {
        var cultureInfo = culture != null ? new System.Globalization.CultureInfo(culture) : null;
        return value.ToHuman(decimalPlaces ?? _options.DefaultDecimalPlaces, cultureInfo);
    }

    /// <inheritdoc/>
    public string FormatCurrency(decimal value, string? currencyCode = null, int? decimalPlaces = null)
    {
        if (currencyCode != null)
        {
            return value.ToHumanCurrency(currencyCode, decimalPlaces ?? _options.DefaultDecimalPlaces);
        }

        return value.ToHumanCurrency(decimalPlaces ?? _options.DefaultDecimalPlaces);
    }
}