namespace Lithium.Server.Core.Networking.Authentication;

/// <summary>
/// Represents the claims extracted from a Hytale access token.
/// </summary>
public sealed record JwtClaims(
    string Issuer,
    string? Audience,
    Guid? Subject,
    string Username,
    string? IpAddress,
    long IssuedAt,
    long ExpiresAt,
    long? NotBefore,
    string? CertificateFingerprint
);