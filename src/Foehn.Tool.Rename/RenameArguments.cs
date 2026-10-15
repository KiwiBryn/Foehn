// Copyright (c) August 2026, devMobile Software
// 
namespace Foehn.Tool.Rename;

internal sealed class RenameArguments
{
    public required string SourcePath { get; init; }
    public required string DestinationPath { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RenameArguments))]
internal sealed partial class RenameJsonContext : JsonSerializerContext
{
}
