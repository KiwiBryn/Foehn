// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

public sealed record ChatResponseFormat
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("json_schema")]
    public JsonSchemaDefinition? JsonSchema { get; init; }

    public static ChatResponseFormat Text { get; } = new() { Type = "text" };

    public static ChatResponseFormat JsonObject { get; } = new() { Type = "json_object" };

    public static ChatResponseFormat FromJsonSchema(JsonSchemaDefinition schema) =>
        new() { Type = "json_schema", JsonSchema = schema };
}

public sealed record JsonSchemaDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("schema")]
    public required JsonElement Schema { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("strict")]
    public bool? Strict { get; init; }
}
