namespace Lithium.Server.Core;

public sealed class Manifest
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string Author { get; set; }
    public required string Github { get; set; }
    public required HashSet<string> Dependencies { get; set; } = [];
}