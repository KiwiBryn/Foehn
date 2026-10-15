// Copyright (c) August 2026, devMobile Software
//
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MistralAi.Composite.Client.Dtos.Buffered;
using MistralAi.Composite.Client.Dtos.Shared;
using MistralAi.Composite.Client.Dtos.Streaming;

namespace MistralAi.Composite.Client;

/// <summary>
/// Typed client for the Mistral "/v1/chat/completions" endpoint. Intended to be constructed via
/// <c>IHttpClientFactory</c> (see <see cref="ServiceCollectionExtensions.AddMistralChatCompletionClient"/>),
/// which supplies the base address, auth header, and resilience pipeline.
/// </summary>
public sealed class ChatCompletionClient : IChatCompletionClient
{
    private const string ChatCompletionsPath = "v1/chat/completions";
    private const string ModelsPath = "v1/models";
    private const string SseDataPrefix = "data:";
    private const string SseDoneSentinel = "[DONE]";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;

    public ChatCompletionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var bufferedRequest = request with { Stream = false };

        using var httpResponse = await _httpClient
            .PostAsJsonAsync(ChatCompletionsPath, bufferedRequest, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessAsync(httpResponse, cancellationToken).ConfigureAwait(false);

        var response = await httpResponse.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(SerializerOptions, cancellationToken)
            .ConfigureAwait(false);

        return response ?? throw new InvalidOperationException("Mistral API returned an empty chat completion response.");
    }

    public async IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var streamingRequest = request with { Stream = true };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath)
        {
            Content = JsonContent.Create(streamingRequest, options: SerializerOptions),
        };
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var httpResponse = await _httpClient
            .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        await EnsureSuccessAsync(httpResponse, cancellationToken).ConfigureAwait(false);

        await using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (line.Length == 0 || !line.StartsWith(SseDataPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line[SseDataPrefix.Length..].Trim();
            if (payload.Length == 0)
            {
                continue;
            }

            if (payload == SseDoneSentinel)
            {
                yield break;
            }

            ChatCompletionChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(payload, SerializerOptions);
            }
            catch (JsonException)
            {
                // Skip malformed chunks rather than aborting the whole stream.
                continue;
            }

            if (chunk is not null)
            {
                yield return chunk;
            }
        }
    }

    public async Task WarmupConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient
                .GetAsync(ModelsPath, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Best-effort only: a warm-up failure (bad key, DNS, offline, etc.) isn't reported here.
            // The same failure will resurface with proper context on the first real request.
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new MistralApiException(response.StatusCode, body);
    }
}
