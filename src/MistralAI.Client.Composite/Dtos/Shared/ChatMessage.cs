// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(SystemMessage), "system")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
[JsonDerivedType(typeof(ToolMessage), "tool")]
public abstract record ChatMessage
{
    [JsonIgnore]
    public string Role => this switch
    {
        SystemMessage => "system",
        UserMessage => "user",
        AssistantMessage => "assistant",
        ToolMessage => "tool",
        _ => throw new InvalidOperationException("Unknown message type")
    };

    [JsonIgnore]
    public string? Content => this switch
    {
        SystemMessage sm => sm.Content,
        UserMessage um => um.Content?.Text,
        AssistantMessage am => am.Content?.Text,
        ToolMessage tm => tm.Content,
        _ => null
    };
}

public sealed record SystemMessage : ChatMessage
{
    [JsonPropertyName("content")]
    public new required string Content { get; init; }
}

public sealed record UserMessage : ChatMessage
{
    [JsonPropertyName("content")]
    public new required ChatMessageContent Content { get; init; }
}

public sealed record AssistantMessage : ChatMessage
{
    [JsonPropertyName("content")]
    public new ChatMessageContent? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    public IReadOnlyList<ToolCall>? ToolCalls { get; init; }

    /// <summary>When true, marks this message as a prefix the model should continue from rather than a completed turn.</summary>
    [JsonPropertyName("prefix")]
    public bool? Prefix { get; init; }
}

public sealed record ToolMessage : ChatMessage
{
    [JsonPropertyName("content")]
    public new required string Content { get; init; }

    [JsonPropertyName("tool_call_id")]
    public required string ToolCallId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
