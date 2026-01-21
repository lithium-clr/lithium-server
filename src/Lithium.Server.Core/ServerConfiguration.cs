namespace Lithium.Server.Core;

public sealed record ServerConfiguration
{
    public string? Password { get; set; }
    public IReadOnlyList<string> Plugins { get; init; } = [];

    public static ServerConfiguration Default => new();
}