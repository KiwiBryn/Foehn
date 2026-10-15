// Copyright (c) August 2026, devMobile Software
//
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace MistralAi.Composite.Client;

public static class ServiceCollectionExtensions
{
    public const string ResiliencePipelineName = "mistral-chat-completion";

    /// <summary>
    /// How long a pooled connection (and the DNS lookup behind it) is kept alive before being torn
    /// down and re-established. Kept in sync with <see cref="IHttpClientBuilder.SetHandlerLifetime"/>
    /// below so IHttpClientFactory's own handler rotation doesn't cut this short.
    /// </summary>
    private static readonly TimeSpan PooledConnectionLifetime = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Registers <see cref="IChatCompletionClient"/> as an <c>IHttpClientFactory</c>-backed typed client
    /// with a resilience pipeline (retry, circuit breaker, timeouts) for transient HTTP failures.
    /// </summary>
    public static IHttpClientBuilder AddMistralChatCompletionClient(
        this IServiceCollection services,
        Action<MistralClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<MistralClientOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var httpClientBuilder = services
            .AddHttpClient<IChatCompletionClient, ChatCompletionClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<MistralClientOptions>>().Value;

                client.BaseAddress = options.BaseAddress;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Streaming completions can legitimately run longer than HttpClient's 100s default,
                // and HttpClient.Timeout applies to the whole request (not just the headers) even with
                // ResponseHeadersRead. Time budgeting is delegated to the resilience pipeline below instead.
                client.Timeout = Timeout.InfiniteTimeSpan;

                // Opportunistically use HTTP/2 (falls back to 1.1 automatically if the server/proxy
                // doesn't negotiate it via ALPN) so concurrent requests can share one connection
                // instead of each paying a fresh TCP+TLS handshake — a bigger cost the further away
                // the API region is.
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                // Avoid paying an extra round trip waiting for a "100 Continue" before the request
                // body is sent. SocketsHttpHandler already defaults to this, but pin it explicitly
                // rather than depend on that default over a link where every RTT counts.
                client.DefaultRequestHeaders.ExpectContinue = false;
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                // Keep the TCP/TLS connection (and the DNS lookup that established it) alive across
                // requests instead of paying that round-trip cost again on every call. Matched by
                // SetHandlerLifetime below so IHttpClientFactory's own rotation doesn't undercut it.
                PooledConnectionLifetime = PooledConnectionLifetime,
                EnableMultipleHttp2Connections = true,

                // Transparently request and decompress gzip/br/deflate response bodies — off by
                // default on SocketsHttpHandler, but worth it for completions and tool-heavy payloads
                // over a slow link.
                AutomaticDecompression = DecompressionMethods.All,
            });

        // IHttpClientFactory recycles the primary handler (and its connection pool) on this schedule
        // regardless of PooledConnectionLifetime above; keep the two aligned.
        httpClientBuilder.SetHandlerLifetime(PooledConnectionLifetime);

        httpClientBuilder.AddResilienceHandler(ResiliencePipelineName, (builder, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<MistralClientOptions>>().Value;

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome)),
            });

            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(Math.Max(30, options.AttemptTimeout.TotalSeconds * 2)),
                BreakDuration = TimeSpan.FromSeconds(15),
                ShouldHandle = args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome)),
            });

            // Applies per attempt; a slow model response shouldn't be treated as a transient failure
            // as aggressively as a genuinely hung connection.
            builder.AddTimeout(options.AttemptTimeout);
        });

        httpClientBuilder.AddResilienceHandler($"{ResiliencePipelineName}-total-timeout", (builder, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<MistralClientOptions>>().Value;
            builder.AddTimeout(options.TotalRequestTimeout);
        });

        return httpClientBuilder;
    }
}
