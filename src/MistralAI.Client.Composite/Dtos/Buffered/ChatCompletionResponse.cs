// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;
using MistralAi.Composite.Client.Dtos.Shared;

namespace MistralAi.Composite.Client.Dtos.Buffered;

public sealed record ChatCompletionResponse
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
    public required IReadOnlyList<ChatCompletionChoice> Choices { get; init; }

    [JsonPropertyName("usage")]
    public required UsageInfo Usage { get; init; }
}

public sealed record ChatCompletionChoice
{
    [JsonPropertyName("index")]
    public required int Index { get; init; }

    [JsonPropertyName("message")]
    public required AssistantMessage Message { get; init; }

    [JsonPropertyName("finish_reason")]
    public FinishReason? FinishReason { get; init; }
}
