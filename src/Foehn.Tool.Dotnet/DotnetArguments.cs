// Copyright (c) August 2026, devMobile Software
// 
namespace Foehn.Tool.Dotnet;

internal sealed class DotnetArguments
{
    public required string Arguments { get; init; }
    public string? WorkingDirectory { get; init; }
    public int? TimeoutSeconds { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DotnetArguments))]
internal sealed partial class DotnetJsonContext : JsonSerializerContext
{
}
