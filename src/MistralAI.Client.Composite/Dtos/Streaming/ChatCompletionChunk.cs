// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;
using MistralAi.Composite.Client.Dtos.Shared;

namespace MistralAi.Composite.Client.Dtos.Streaming;

/// <summary>One "data: {...}" event from the chat completions SSE stream.</summary>
public sealed record ChatCompletionChunk
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("created")]
    public required long Created { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("choices")]
    public required IReadOnlyList<ChatCompletionStreamChoice> Choices { get; init; }

    /// <summary>Only populated on the final chunk of the stream.</summary>
    [JsonPropertyName("usage")]
    public UsageInfo? Usage { get; init; }
}

public sealed record ChatCompletionStreamChoice
{
    [JsonPropertyName("index")]
    public required int Index { get; init; }

    [JsonPropertyName("delta")]
    public required ChatCompletionDelta Delta { get; init; }

    [JsonPropertyName("finish_reason")]
    public FinishReason? FinishReason { get; init; }
}

public sealed record ChatCompletionDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }
}
