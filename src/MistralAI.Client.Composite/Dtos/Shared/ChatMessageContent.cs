// Copyright (c) August 2026, devMobile Software
//
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MistralAi.Composite.Client.Dtos.Shared;

/// <summary>
/// Mistral accepts message content as either a plain string or an array of
/// content parts (text/image). This wraps both shapes behind one JSON-serializable type.
/// </summary>
[JsonConverter(typeof(ChatMessageContentJsonConverter))]
public sealed class ChatMessageContent
{
    private ChatMessageContent(string? text, IReadOnlyList<ContentPart>? parts)
    {
        Text = text;
        Parts = parts;
    }

    public string? Text { get; }

    public IReadOnlyList<ContentPart>? Parts { get; }

    public static ChatMessageContent FromText(string text) => new(text, null);

    public static ChatMessageContent FromParts(IReadOnlyList<ContentPart> parts) => new(null, parts);

    public static implicit operator ChatMessageContent(string text) => FromText(text);

    public static implicit operator ChatMessageContent(ContentPart[] parts) => FromParts(parts);

    public static implicit operator ChatMessageContent(List<ContentPart> parts) => FromParts(parts);
}

internal sealed class ChatMessageContentJsonConverter : JsonConverter<ChatMessageContent>
{
    public override ChatMessageContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return ChatMessageContent.FromText(reader.GetString()!);
        }

        var parts = JsonSerializer.Deserialize<List<ContentPart>>(ref reader, options)
            ?? throw new JsonException("Expected message content to be a string or an array of content parts.");
        return ChatMessageContent.FromParts(parts);
    }

    public override void Write(Utf8JsonWriter writer, ChatMessageContent value, JsonSerializerOptions options)
    {
        if (value.Text is not null)
        {
            writer.WriteStringValue(value.Text);
            return;
        }

        JsonSerializer.Serialize(writer, value.Parts, options);
    }
}
