namespace Lithium.Server.Core;

public sealed record ServerConfiguration
{
    public IReadOnlyList<string> Plugins { get; init; } = [];

    public static ServerConfiguration Default => new();
}