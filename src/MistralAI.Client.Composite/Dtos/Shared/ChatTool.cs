// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

public sealed record ChatTool
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    [JsonPropertyName("function")]
    public required FunctionDefinition Function { get; init; }
}

public sealed record FunctionDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>JSON Schema object describing the function's parameters.</summary>
    [JsonPropertyName("parameters")]
    public JsonElement? Parameters { get; init; }

    [JsonPropertyName("strict")]
    public bool? Strict { get; init; }
}

public sealed record ToolCall
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = "function";

    [JsonPropertyName("function")]
    public required ToolCallFunction Function { get; init; }

    /// <summary>Position of this call within the delta; only populated on streaming chunks.</summary>
    [JsonPropertyName("index")]
    public int? Index { get; init; }
}

public sealed record ToolCallFunction
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>JSON-encoded arguments string (assembled incrementally across streaming chunks).</summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; init; }
}
