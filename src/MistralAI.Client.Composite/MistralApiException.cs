// Copyright (c) August 2026, devMobile Software
//
using System.Net;

namespace MistralAi.Composite.Client;

public sealed class MistralApiException : Exception
{
    public MistralApiException(HttpStatusCode statusCode, string? responseBody)
        : base($"Mistral API request failed with status code {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }
}
