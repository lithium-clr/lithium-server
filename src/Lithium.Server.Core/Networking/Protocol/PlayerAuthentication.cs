namespace Lithium.Server.Core.Networking.Protocol;

public sealed class PlayerAuthentication(Guid uuid, string username)
{
    public const int MaxReferralDataSize = 4096;

    public Guid Uuid { get; } = uuid;
    public string Username { get; set; } = username;
    public byte[] ReferralData { get; set; } = [];
    public HostAddress? ReferralSource { get; set; }
}