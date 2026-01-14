namespace Lithium.Server.Core.Auth;

public sealed record TokenResponse(
    string? AccessToken,
    string? RefreshToken,
    string? IdToken,
    string? Error,
    int ExpiresIn)
{
    public bool IsSuccess()
    {
        return Error is null && AccessToken is not null;
    }
}