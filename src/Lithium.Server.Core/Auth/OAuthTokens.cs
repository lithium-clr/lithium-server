namespace Lithium.Server.Core.Auth;

public sealed record OAuthTokens(string? AccessToken, string? RefreshToken, DateTimeOffset? AccessTokenExpiresAt)
{
    public bool IsValid() => RefreshToken is not null;
}