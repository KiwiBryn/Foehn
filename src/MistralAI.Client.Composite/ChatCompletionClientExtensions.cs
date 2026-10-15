// Copyright (c) August 2026, devMobile Software
//
using System.Runtime.CompilerServices;
using MistralAi.Composite.Client.Dtos.Shared;

namespace MistralAi.Composite.Client;

/// <summary>
/// Convenience overloads for one-shot prompts, layered on top of <see cref="IChatCompletionClient"/>
/// so the core interface stays a thin, two-method wrapper around the HTTP API.
/// </summary>
public static class ChatCompletionClientExtensions
{
    /// <summary>Sends a single user message (with an optional system prompt) and returns the assistant's reply text.</summary>
    public static async Task<string?> CompleteAsync(
        this IChatCompletionClient client,
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = BuildMessages(systemMessage, userMessage),
            Temperature = temperature,
            MaxTokens = maxTokens,
        };

        var response = await client.CreateChatCompletionAsync(request, cancellationToken).ConfigureAwait(false);
        return response.Choices.Count > 0 ? response.Choices[0].Message.Content?.Text : null;
    }

    /// <summary>Sends a single user message (with an optional system prompt) and streams back the assistant's reply text.</summary>
    public static async IAsyncEnumerable<string> StreamTextAsync(
        this IChatCompletionClient client,
        string model,
        string userMessage,
        string? systemMessage = null,
        double? temperature = null,
        int? maxTokens = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        var request = new ChatCompletionRequest
        {
            Model = model,
            Messages = BuildMessages(systemMessage, userMessage),
            Temperature = temperature,
            MaxTokens = maxTokens,
        };

        await foreach (var chunk in client.StreamChatCompletionAsync(request, cancellationToken).ConfigureAwait(false))
        {
            var delta = chunk.Choices.Count > 0 ? chunk.Choices[0].Delta.Content : null;
            if (!string.IsNullOrEmpty(delta))
            {
                yield return delta;
            }
        }
    }

    private static IReadOnlyList<ChatMessage> BuildMessages(string? systemMessage, string userMessage)
    {
        List<ChatMessage> messages = [];

        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            messages.Add(new SystemMessage { Content = systemMessage });
        }

        messages.Add(new UserMessage { Content = userMessage });
        return messages;
    }
}
