using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HumanNumbers.AspNetCore.Serialization;
using HumanNumbers.Formatting;
using System.Text.Json;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Extension methods for producing human-readable JSON results in Minimal APIs.
/// </summary>
public static class ResultsExtensions
{
    /// <summary>
    /// Returns a JSON result with human-readable numeric formatting.
    /// </summary>
    /// <param name="results">The <see cref="IResultExtensions"/>.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="decimalPlaces">The number of decimal places for numeric formatting. Defaults to 2.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to 200 OK.</param>
    /// <returns>A <see cref="IResult"/> that serializes the value as human-readable JSON.</returns>
    public static IResult Human(this IResultExtensions results, object? value, int decimalPlaces = 2, int statusCode = StatusCodes.Status200OK)
    {
        return new HumanJsonResult(value, new HumanNumberFormatOptions { DecimalPlaces = decimalPlaces }, statusCode);
    }

    /// <summary>
    /// Returns a JSON result with human-readable numeric formatting using a fluent configuration delegate.
    /// </summary>
    /// <param name="results">The <see cref="IResultExtensions"/>.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="configure">An action to configure <see cref="HumanNumberFormatOptions"/>.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to 200 OK.</param>
    public static IResult Human(this IResultExtensions results, object? value, Action<HumanNumberFormatOptions> configure, int statusCode = StatusCodes.Status200OK)
    {
        var options = new HumanNumberFormatOptions();
        configure(options);
        return new HumanJsonResult(value, options, statusCode);
    }

    /// <summary>Returns a 200 OK JSON result with human-readable numeric formatting.</summary>
    /// <param name="results">The <see cref="IResultExtensions"/>.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="decimalPlaces">The number of decimal places. Defaults to 2.</param>
    public static IResult HumanOk(this IResultExtensions results, object? value, int decimalPlaces = 2)
    {
        return results.Human(value, decimalPlaces, StatusCodes.Status200OK);
    }

    /// <summary>Returns a 200 OK JSON result with human-readable numeric formatting using a fluent configuration delegate.</summary>
    /// <param name="results">The <see cref="IResultExtensions"/>.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="configure">An action to configure <see cref="HumanNumberFormatOptions"/>.</param>
    public static IResult HumanOk(this IResultExtensions results, object? value, Action<HumanNumberFormatOptions> configure)
    {
        return results.Human(value, configure, StatusCodes.Status200OK);
    }
}

internal class HumanJsonResult : IResult
{
    private readonly object? _value;
    private readonly HumanNumberFormatOptions _options;
    private readonly int _statusCode;

    public HumanJsonResult(object? value, HumanNumberFormatOptions options, int statusCode)
    {
        _value = value;
        _options = options;
        _statusCode = statusCode;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new HumanNumberJsonConverterFactory(_options));

        return httpContext.Response.WriteAsJsonAsync(_value, options);
    }
}
