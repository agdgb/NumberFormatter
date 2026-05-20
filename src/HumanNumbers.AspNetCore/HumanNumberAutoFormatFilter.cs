using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections;
using System.Dynamic;
using HumanNumbers.Formatting;

namespace HumanNumbers.AspNetCore;


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
            // Transform the response object into a dynamic representation with formatted values
            objectResult.Value = ProcessValue(objectResult.Value);
        }
    }

    private object? ProcessValue(object? value, HashSet<object>? visited = null)
    {
        if (value == null) return null;

        var type = value.GetType();

        // 1. Handle base types that don't need transformation
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return value;
        }

        // Detect circular references
        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!visited.Add(value))
        {
            // Circular reference detected, return original value to avoid stack overflow
            return value;
        }

        try
        {
            // 2. Handle collections (Arrays, Lists, etc.)
            if (value is IEnumerable enumerable && !(value is string))
            {
                var list = new List<object?>();
                foreach (var item in enumerable)
                {
                    list.Add(ProcessValue(item, visited));
                }
                return list;
            }

            // 3. Handle complex objects via ExpandoObject transformation
            return TransformObject(value, visited);
        }
        finally
        {
            visited.Remove(value);
        }
    }

    private object TransformObject(object obj, HashSet<object> visited)
    {
        var type = obj.GetType();
        var expando = new ExpandoObject();
        var dictionary = (IDictionary<string, object?>)expando;

        var metadata = _cache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new PropertyMetadata
            {
                Name = p.Name,
                Property = p,
                IsNumeric = IsNumericType(p.PropertyType),
                Attribute = p.GetCustomAttribute<HumanNumberAttribute>(),
                IsExempt = p.GetCustomAttribute<NoHumanFormatAttribute>() != null
            }).ToArray());

        foreach (var entry in metadata)
        {
            var rawValue = entry.Property.GetValue(obj);
            bool shouldFormat = _options.AutoFormatMode switch
            {
                AutoFormatMode.OptInAttributeOnly => entry.Attribute != null,
                AutoFormatMode.OptOutAttribute => !entry.IsExempt,
                AutoFormatMode.Global => !entry.IsExempt,
                _ => false
            };

            if (entry.IsNumeric && shouldFormat && rawValue != null)
            {
                dictionary[entry.Name] = FormatNumericValue(rawValue, entry.Attribute);
            }
            else
            {
                // Recursively process nested objects
                dictionary[entry.Name] = ProcessValue(rawValue, visited);
            }
        }

        return expando;
    }

    private string FormatNumericValue(object value, HumanNumberAttribute? attr)
    {
        decimal decimalValue = Convert.ToDecimal(value);
        
        // Use attribute settings if available, otherwise use global defaults
        var options = _options.CoreOptions with { }; // Snapshot

        if (attr != null)
        {
            if (!string.IsNullOrEmpty(attr.PolicyName))
            {
                options = options.UsingPolicy(attr.PolicyName);
            }
            else
            {
                options = options with { DecimalPlaces = attr.DecimalPlaces };
            }

            if (attr.IsCurrency)
            {
                var currencyCode = attr.CurrencyCode ?? options.CurrencySymbol ?? "$";
                return decimalValue.ToHumanCurrency(currencyCode, options.DecimalPlaces);
            }
        }

        return decimalValue.ToHuman(options);
    }

    private class PropertyMetadata
    {
        public string Name { get; set; } = null!;
        public PropertyInfo Property { get; set; } = null!;
        public bool IsNumeric { get; set; }
        public HumanNumberAttribute? Attribute { get; set; }
        public bool IsExempt { get; set; }
    }

    private static bool IsNumericType(Type type) => NumberUtils.IsNumericType(type);
}
