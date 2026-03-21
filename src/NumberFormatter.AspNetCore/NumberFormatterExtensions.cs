using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Numerics;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// Extension methods for ASP.NET Core integration
/// </summary>
public static class NumberFormatterExtensions
{
    /// <summary>
    /// Adds number formatter services to the DI container
    /// </summary>
    public static IServiceCollection AddNumberFormatter(this IServiceCollection services)
    {
        services.TryAddSingleton<INumberFormatterService, NumberFormatterService>();
        return services;
    }

    /// <summary>
    /// Adds MVC services with short number formatting
    /// </summary>
    public static IMvcBuilder AddNumberFormatterMvc(this IServiceCollection services)
    {
        return services.AddControllers()
            .AddShortNumberJsonFormatter();
    }

    /// <summary>
    /// Adds MVC services with short number formatting and custom options
    /// </summary>
    public static IMvcBuilder AddNumberFormatterMvc(
        this IServiceCollection services,
        Action<MvcOptions>? configureMvc = null,
        int defaultDecimalPlaces = 2)
    {
        var builder = services.AddControllers(configureMvc ?? (_ => { }));
        return builder.AddShortNumberJsonFormatter(defaultDecimalPlaces);
    }

    /// <summary>
    /// Configures JSON options for short number formatting
    /// </summary>
    public static IMvcBuilder AddShortNumberJsonFormatter(
        this IMvcBuilder builder,
        int defaultDecimalPlaces = 2)
    {
        builder.AddJsonOptions(options =>
        {
            // Add the converter factory
            options.JsonSerializerOptions.Converters.Add(
                new ShortNumberJsonConverterFactory(defaultDecimalPlaces));
        });

        return builder;
    }

    /// <summary>
    /// Adds Razor Pages with short number tag helper support
    /// </summary>
    public static IMvcBuilder AddNumberFormatterRazorPages(this IServiceCollection services)
    {
        return services.AddRazorPages()
            .AddShortNumberJsonFormatter();
    }
}

/// <summary>
/// Service interface for number formatting
/// </summary>
public interface INumberFormatterService
{
    string FormatShort(decimal value, int decimalPlaces = 2, string? culture = null);
    string FormatCurrency(decimal value, int decimalPlaces = 2, string? currencyCode = null);
    string FormatShort<T>(T value, int decimalPlaces = 2, string? culture = null) where T : INumber<T>;
}

/// <summary>
/// Implementation of number formatter service
/// </summary>
public class NumberFormatterService : INumberFormatterService
{
    public string FormatShort(decimal value, int decimalPlaces = 2, string? culture = null)
    {
        var cultureInfo = culture != null ? new System.Globalization.CultureInfo(culture) : null;
        return value.ToShortString(decimalPlaces, cultureInfo);
    }

    public string FormatCurrency(decimal value, int decimalPlaces = 2, string? currencyCode = null)
    {
        if (currencyCode != null)
        {
            return value.ToShortCurrencyString(currencyCode, decimalPlaces);
        }

        return value.ToShortCurrencyString(decimalPlaces);
    }

    public string FormatShort<T>(T value, int decimalPlaces = 2, string? culture = null) where T : INumber<T>
    {
        var decimalValue = Convert.ToDecimal(value);
        var cultureInfo = culture != null ? new System.Globalization.CultureInfo(culture) : null;
        return decimalValue.ToShortString(decimalPlaces, cultureInfo);
    }
}