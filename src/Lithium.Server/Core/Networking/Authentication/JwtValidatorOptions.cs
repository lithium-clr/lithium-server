namespace Lithium.Server.Core.Networking.Authentication;

public sealed class JwtValidatorOptions
{
    public string JwksUri { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string? Audience { get; set; }
}