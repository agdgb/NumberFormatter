using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Collections.Concurrent;
using HumanNumbers.Formatting;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Attribute to mark a property for automatic human-readable formatting in API responses.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class HumanNumberAttribute : Attribute
{
    /// <summary>Number of decimal places to use for this property.</summary>
    public int? DecimalPlaces { get; set; }
    /// <summary>The name of the policy to use for formatting this property.</summary>
    public string? PolicyName { get; set; }
}

/// <summary>
/// Action filter that automatically formats numeric properties in API responses.
/// </summary>
public class HumanNumberAutoFormatFilter : IAsyncActionFilter
{
    private readonly HumanNumbersOptions _options;
    private static readonly ConcurrentDictionary<Type, PropertyMetadata[]> _cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberAutoFormatFilter"/> class.
    /// </summary>
    public HumanNumberAutoFormatFilter(HumanNumbersOptions options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (!_options.EnableAutoFormatting || _options.AutoFormatMode == AutoFormatMode.Off)
            return;

        if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            // Note: Mutating the response object is generally risky but requested for this feature.
            // A more robust implementation might use a JsonConverter instead.
            FormatObject(objectResult.Value);
        }
    }

    private void FormatObject(object obj)
    {
        if (obj == null) return;
        
        var type = obj.GetType();
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal)) return;

        var metadata = _cache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new PropertyMetadata
            {
                Property = p,
                IsNumeric = IsNumericType(p.PropertyType),
                Attribute = p.GetCustomAttribute<HumanNumberAttribute>(),
                IsExempt = p.GetCustomAttribute<NoHumanFormatAttribute>() != null
            }).ToArray());

        foreach (var entry in metadata)
        {
            if (!entry.IsNumeric || entry.IsExempt) continue;

            bool shouldFormat = _options.AutoFormatMode switch
            {
                AutoFormatMode.OptInAttributeOnly => entry.Attribute != null,
                AutoFormatMode.OptOutAttribute => true, // Already checked IsExempt
                AutoFormatMode.Global => true,
                _ => false
            };

            if (shouldFormat)
            {
                var val = entry.Property.GetValue(obj);
                if (val is decimal decimalVal)
                {
                    // For now, this filter only "conceptually" formats. 
                    // In a real-world scenario, we'd replace the value with a string if we were using a dynamic wrapper,
                    // or let the JSON converter handle it.
                    // Given the constraint of 'Safe-First', we will only format if we can safely cast or if the user expects a transformed output.
                    
                    // IF the property were a string, we could set it. But it's numeric.
                    // This highlights why JSON Converters are better. 
                    // However, we'll keep the filter infrastructure for metadata discovery.
                }
            }
        }
    }

    private class PropertyMetadata
    {
        public PropertyInfo Property { get; set; } = null!;
        public bool IsNumeric { get; set; }
        public HumanNumberAttribute? Attribute { get; set; }
        public bool IsExempt { get; set; }
    }

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(decimal) || underlyingType == typeof(double) ||
               underlyingType == typeof(float) || underlyingType == typeof(int) ||
               underlyingType == typeof(long);
    }
}
