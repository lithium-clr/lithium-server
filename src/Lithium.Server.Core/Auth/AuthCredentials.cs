using Lithium.Codecs;

namespace Lithium.Server.Core.Auth;

[Codec]
public sealed class AuthCredentials
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid ProfileUuid { get; set; }
}