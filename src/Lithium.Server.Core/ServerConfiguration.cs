namespace Lithium.Server.Core;

public sealed record ServerConfiguration
{
    public string ServerName { get; set; } = "Lithium Server";
    public string Motd { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int MaxPlayers { get; set; } = 100;
    public int MaxViewRadius { get; set; } = 32;
    public int WorldHeight { get; set; } = 320;
    public string AssetsPath { get; set; } = "assets";
    public IReadOnlyList<string> Plugins { get; init; } = [];
    public string CertificatePassword { get; init; } = null!;

    public static ServerConfiguration Default => new();
}