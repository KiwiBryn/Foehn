// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

/// <summary>
/// Controls tool selection. Serializes as either a bare mode string ("auto"/"none"/"any"/"required")
/// or as <c>{ "type": "function", "function": { "name": "..." } }</c> to force a specific tool.
/// </summary>
[JsonConverter(typeof(ToolChoiceJsonConverter))]
public sealed class ToolChoice
{
    private ToolChoice(string? mode, string? functionName)
    {
        Mode = mode;
        FunctionName = functionName;
    }

    public string? Mode { get; }

    public string? FunctionName { get; }

    public static ToolChoice Auto { get; } = new("auto", null);

    public static ToolChoice None { get; } = new("none", null);

    public static ToolChoice Any { get; } = new("any", null);

    public static ToolChoice Required { get; } = new("required", null);

    public static ToolChoice ForFunction(string name) => new(null, name);
}

internal sealed class ToolChoiceJsonConverter : JsonConverter<ToolChoice>
{
    public override ToolChoice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var mode = reader.GetString()!;
            return mode switch
            {
                "auto" => ToolChoice.Auto,
                "none" => ToolChoice.None,
                "any" => ToolChoice.Any,
                "required" => ToolChoice.Required,
                _ => throw new JsonException($"Unknown tool_choice mode '{mode}'."),
            };
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var name = document.RootElement
            .GetProperty("function")
            .GetProperty("name")
            .GetString();

        return ToolChoice.ForFunction(name ?? throw new JsonException("tool_choice.function.name was null."));
    }

    public override void Write(Utf8JsonWriter writer, ToolChoice value, JsonSerializerOptions options)
    {
        if (value.FunctionName is not null)
        {
            writer.WriteStartObject();
            writer.WriteString("type", "function");
            writer.WriteStartObject("function");
            writer.WriteString("name", value.FunctionName);
            writer.WriteEndObject();
            writer.WriteEndObject();
            return;
        }

        writer.WriteStringValue(value.Mode);
    }
}
