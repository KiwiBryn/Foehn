// Copyright (c) August 2026, devMobile Software
//
using System.ComponentModel.DataAnnotations;

namespace MistralAi.Composite.Client;

public sealed class MistralClientOptions
{
    [Required]
    public required string ApiKey { get; set; }

    public Uri BaseAddress { get; set; } = new("https://api.mistral.ai/");

    /// <summary>Used by the convenience overloads in <see cref="ChatCompletionClientExtensions"/> when no model is specified.</summary>
    public string DefaultModel { get; set; } = "mistral-large-latest";

    /// <summary>
    /// Per-attempt timeout enforced by the resilience pipeline. Unlike <c>HttpClient.Timeout</c>,
    /// this does not need to cover the full duration of a streamed response.
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Overall budget for a request, including retries. Streaming responses can legitimately run
    /// for a while, so this is deliberately generous rather than using HttpClient's 100s default.
    /// </summary>
    public TimeSpan TotalRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
