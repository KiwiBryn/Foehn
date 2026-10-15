// Copyright (c) August 2026, devMobile Software
// 
namespace Foehn.Tool.ExecuteAsync;

internal sealed class ExecuteDetachedArguments
{
    public required string Command { get; init; }
    public string? Arguments { get; init; }
    public string? WorkingDirectory { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExecuteDetachedArguments))]
internal sealed partial class ExecuteDetachedJsonContext : JsonSerializerContext
{
}
