using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NumberFormatter.AspNetCore.Financial;

internal static class ConverterFactoryHelper
{
    public static JsonConverter CreateGenericWrapper(Type typeToConvert, JsonConverter<decimal> baseConverter)
    {
        if (typeToConvert == typeof(decimal))
            return baseConverter;

        if (typeToConvert == typeof(decimal?))
            return new NullableConverterWrapper<decimal>(baseConverter);

        if (typeToConvert == typeof(decimal[]))
            return new ArrayConverterWrapper<decimal>(baseConverter);

        if (typeToConvert == typeof(List<decimal>))
            return new ListConverterWrapper<decimal>(baseConverter);

        if (typeToConvert == typeof(IEnumerable<decimal>))
            return new EnumerableConverterWrapper<decimal>(baseConverter);

        if (typeToConvert == typeof(Dictionary<string, decimal>))
            return new DictionaryConverterWrapper<decimal>(baseConverter);

        // Nullable variants
        if (typeToConvert == typeof(decimal?[]))
        {
            var nullableInner = new NullableConverterWrapper<decimal>(baseConverter);
            return new ArrayConverterWrapper<decimal?>(nullableInner);
        }

        if (typeToConvert == typeof(List<decimal?>))
        {
            var nullableInner = new NullableConverterWrapper<decimal>(baseConverter);
            return new ListConverterWrapper<decimal?>(nullableInner);
        }

        if (typeToConvert == typeof(IEnumerable<decimal?>))
        {
            var nullableInner = new NullableConverterWrapper<decimal>(baseConverter);
            return new EnumerableConverterWrapper<decimal?>(nullableInner);
        }

        if (typeToConvert == typeof(Dictionary<string, decimal?>))
        {
            var nullableInner = new NullableConverterWrapper<decimal>(baseConverter);
            return new DictionaryConverterWrapper<decimal?>(nullableInner);
        }

        throw new NotSupportedException($"The type {typeToConvert.Name} is not supported by this converter.");
    }
}
