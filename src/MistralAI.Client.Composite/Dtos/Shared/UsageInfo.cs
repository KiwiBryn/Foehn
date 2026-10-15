// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

public sealed record UsageInfo
{
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public required int CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; init; }
}
