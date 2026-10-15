// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContentPart), "text")]
[JsonDerivedType(typeof(ImageUrlContentPart), "image_url")]
public abstract record ContentPart;

public sealed record TextContentPart : ContentPart
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

public sealed record ImageUrlContentPart : ContentPart
{
    [JsonPropertyName("image_url")]
    public required ImageUrlDetail ImageUrl { get; init; }
}

public sealed record ImageUrlDetail
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}
