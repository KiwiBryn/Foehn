// Copyright (c) August 2026, devMobile Software
//
using MistralAi.Composite.Client.Dtos.Buffered;
using MistralAi.Composite.Client.Dtos.Shared;
using MistralAi.Composite.Client.Dtos.Streaming;

namespace MistralAi.Composite.Client;

public interface IChatCompletionClient
{
    /// <summary>Sends a chat completion request and waits for the full, buffered response.</summary>
    Task<ChatCompletionResponse> CreateChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Sends a chat completion request and streams back incremental chunks as they arrive.</summary>
    IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Best-effort connection warm-up: establishes DNS + TCP + TLS ahead of the first real request
    /// (e.g. while a CLI is printing its banner or waiting on user input) without doing any model
    /// inference. Failures are swallowed — the real error, if any, will surface on the first actual
    /// call — so this is meant to be fired without awaiting the result.
    /// </summary>
    Task WarmupConnectionAsync(CancellationToken cancellationToken = default);
}
