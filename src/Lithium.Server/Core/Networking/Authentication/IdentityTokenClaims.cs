namespace Lithium.Server.Core.Networking.Authentication;

/// <summary>
/// Represents the claims extracted from a Hytale identity token.
/// </summary>
public sealed record IdentityTokenClaims(
    string Issuer,
    Guid Subject,
    string Username,
    long IssuedAt,
    long ExpiresAt,
    long? NotBefore,
    string[] Scopes
)
{
    /// <summary>
    /// Checks if the token has a specific scope.
    /// </summary>
    /// <param name="targetScope">The scope to check for.</param>
    /// <returns><c>true</c> if the scope is present; otherwise, <c>false</c>.</returns>
    public bool HasScope(string targetScope) => Scopes.Contains(targetScope);
}