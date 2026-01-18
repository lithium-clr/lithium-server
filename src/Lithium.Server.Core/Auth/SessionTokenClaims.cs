namespace Lithium.Server.Core.Auth;

/// <summary>
/// Represents the claims extracted from a Hytale session token.
/// </summary>
public sealed record SessionTokenClaims(
    string Issuer,
    string Subject,
    long IssuedAt,
    long ExpiresAt,
    long? NotBefore
);