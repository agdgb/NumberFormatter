using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HumanNumbers.AspNetCore.Serialization;
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
        return new HumanJsonResult(value, decimalPlaces, statusCode);
    }

    /// <summary>
    /// Returns a 200 OK JSON result with human-readable numeric formatting.
    /// </summary>
    public static IResult HumanOk(this IResultExtensions results, object? value, int decimalPlaces = 2)
    {
        return results.Human(value, decimalPlaces, StatusCodes.Status200OK);
    }
}

internal class HumanJsonResult : IResult
{
    private readonly object? _value;
    private readonly int _decimalPlaces;
    private readonly int _statusCode;

    public HumanJsonResult(object? value, int decimalPlaces, int statusCode)
    {
        _value = value;
        _decimalPlaces = decimalPlaces;
        _statusCode = statusCode;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        
        // Use default web options as base
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        
        // Add our human numbers factory
        options.Converters.Add(new HumanNumberJsonConverterFactory(_decimalPlaces));

        return httpContext.Response.WriteAsJsonAsync(_value, options);
    }
}
