// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<FinishReason>))]
public enum FinishReason
{
    [JsonStringEnumMemberName("stop")]
    Stop,

    [JsonStringEnumMemberName("length")]
    Length,

    [JsonStringEnumMemberName("model_length")]
    ModelLength,

    [JsonStringEnumMemberName("error")]
    Error,

    [JsonStringEnumMemberName("tool_calls")]
    ToolCalls,
}
