// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

/// <summary>
/// Request body for POST /v1/chat/completions. Shared by both the buffered and streaming
/// client calls; <see cref="Stream"/> is overridden by <c>ChatCompletionClient</c> to match the call made.
/// </summary>
public sealed record ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; init; }

    [JsonPropertyName("stop")]
    public StopSequence? Stop { get; init; }

    [JsonPropertyName("random_seed")]
    public int? RandomSeed { get; init; }

    [JsonPropertyName("response_format")]
    public ChatResponseFormat? ResponseFormat { get; init; }

    [JsonPropertyName("tools")]
    public IReadOnlyList<ChatTool>? Tools { get; init; }

    [JsonPropertyName("tool_choice")]
    public ToolChoice? ToolChoice { get; init; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; init; }

    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; init; }

    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; init; }

    [JsonPropertyName("n")]
    public int? N { get; init; }

    [JsonPropertyName("safe_prompt")]
    public bool? SafePrompt { get; init; }

    [JsonPropertyName("prompt_cache_key")]
    public string? PromptCacheKey { get; init; }

    [JsonPropertyName("reasoning_effort")]
    public string? ReasoningEffort { get; init; }
}
