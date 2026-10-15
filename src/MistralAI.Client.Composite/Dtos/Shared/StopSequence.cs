// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

/// <summary>The "stop" request field: either a single stop string or a list of them.</summary>
[JsonConverter(typeof(StopSequenceJsonConverter))]
public sealed class StopSequence
{
    private StopSequence(IReadOnlyList<string> values)
    {
        Values = values;
    }

    public IReadOnlyList<string> Values { get; }

    public static StopSequence Of(string value) => new([value]);

    public static StopSequence Of(params IReadOnlyList<string> values) => new(values);

    public static implicit operator StopSequence(string value) => Of(value);

    public static implicit operator StopSequence(string[] values) => Of(values);
}

internal sealed class StopSequenceJsonConverter : JsonConverter<StopSequence>
{
    public override StopSequence Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return StopSequence.Of(reader.GetString()!);
        }

        var values = JsonSerializer.Deserialize<List<string>>(ref reader, options)
            ?? throw new JsonException("Expected 'stop' to be a string or an array of strings.");
        return StopSequence.Of(values);
    }

    public override void Write(Utf8JsonWriter writer, StopSequence value, JsonSerializerOptions options)
    {
        if (value.Values.Count == 1)
        {
            writer.WriteStringValue(value.Values[0]);
            return;
        }

        JsonSerializer.Serialize(writer, value.Values, options);
    }
}
