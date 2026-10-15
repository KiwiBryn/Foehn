// Copyright (c) August 2026, devMobile Software
// 
namespace Foehn.Tool.Mkdir;

internal sealed class MkdirArguments
{
    public required string Path { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MkdirArguments))]
internal sealed partial class MkdirJsonContext : JsonSerializerContext
{
}
