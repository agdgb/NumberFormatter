using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HumanNumbers.AspNetCore.Financial;

internal sealed class NullableConverterWrapper<T> : JsonConverter<T?> where T : struct
{
    private readonly JsonConverter<T> _inner;

    public NullableConverterWrapper(JsonConverter<T> inner)
    {
        _inner = inner;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return _inner.Read(ref reader, typeof(T), options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            _inner.Write(writer, value.Value, options);
        }
    }
}

internal sealed class ArrayConverterWrapper<T> : JsonConverter<T[]>
{
    private readonly JsonConverter<T> _inner;

    public ArrayConverterWrapper(JsonConverter<T> inner)
    {
        _inner = inner;
    }

    public override T[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null!;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token.");

        var list = new List<T>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(_inner.Read(ref reader, typeof(T), options)!);
        }

        return list.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        writer.WriteStartArray();
        foreach (var item in value) _inner.Write(writer, item, options);
        writer.WriteEndArray();
    }
}

internal sealed class ListConverterWrapper<T> : JsonConverter<List<T>>
{
    private readonly JsonConverter<T> _inner;

    public ListConverterWrapper(JsonConverter<T> inner) { _inner = inner; }

    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null!;
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected StartArray token.");

        var list = new List<T>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(_inner.Read(ref reader, typeof(T), options)!);
        }
        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        writer.WriteStartArray();
        foreach (var item in value) _inner.Write(writer, item, options);
        writer.WriteEndArray();
    }
}

internal sealed class EnumerableConverterWrapper<T> : JsonConverter<IEnumerable<T>>
{
    private readonly JsonConverter<T> _inner;

    public EnumerableConverterWrapper(JsonConverter<T> inner) { _inner = inner; }

    public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null!;
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected StartArray token.");

        var list = new List<T>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(_inner.Read(ref reader, typeof(T), options)!);
        }
        return list;
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        writer.WriteStartArray();
        foreach (var item in value) _inner.Write(writer, item, options);
        writer.WriteEndArray();
    }
}

internal sealed class DictionaryConverterWrapper<T> : JsonConverter<Dictionary<string, T>>
{
    private readonly JsonConverter<T> _inner;

    public DictionaryConverterWrapper(JsonConverter<T> inner) { _inner = inner; }

    public override Dictionary<string, T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null!;
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject token.");

        var dict = new Dictionary<string, T>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token.");

            var key = reader.GetString()!;
            reader.Read(); // move to value
            
            var value = _inner.Read(ref reader, typeof(T), options)!;
            dict.Add(key, value);
        }
        return dict;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, T> value, JsonSerializerOptions options)
    {
        if (value == null) { writer.WriteNullValue(); return; }
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            _inner.Write(writer, kvp.Value, options);
        }
        writer.WriteEndObject();
    }
}
