namespace Lithium.Server.Core.Auth;

/// <summary>
/// Represents the claims extracted from a Hytale access token.
/// </summary>
public record JwtClaims(
    string Issuer,
    string? Audience,
    Guid Subject,
    string Username,
    string? IpAddress,
    long IssuedAt,
    long ExpiresAt,
    long? NotBefore,
    string? CertificateFingerprint
);