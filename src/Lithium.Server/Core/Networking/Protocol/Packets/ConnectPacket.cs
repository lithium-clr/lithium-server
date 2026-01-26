using Lithium.Server.Core.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 0, IsCompressed = false, VariableBlockStart = 66, MaxSize = 38013)]
public sealed class ConnectPacket : Packet
{
    // Fixed fields (total 45 bytes after the 1 byte nullBits, so 46 physical bytes)
    // Java: protocolCrc (int)
    [PacketProperty(FixedIndex = 0)]
    public int ProtocolCrc { get; set; }

    // Java: protocolBuildNumber (int)
    [PacketProperty(FixedIndex = 4)]
    public int ProtocolBuildNumber { get; set; }

    // Java: clientVersion (fixed ASCII string, 20 bytes)
    [PacketProperty(FixedIndex = 8, FixedSize = 20)]
    public string ClientVersion { get; set; } = ""; 

    // Java: clientType (byte enum)
    [PacketProperty(FixedIndex = 28)]
    public ClientType ClientType { get; set; }

    // Java: uuid (UUID)
    [PacketProperty(FixedIndex = 29)]
    public Guid Uuid { get; set; }

    // Variable fields (Offset indices)
    // Java: username (string, not nullable), OffsetIndex 0
    [PacketProperty(OffsetIndex = 0)]
    public string Username { get; set; } = "";

    // Java: identityToken (string, nullable, bit 0), OffsetIndex 1
    [PacketProperty(BitIndex = 0, OffsetIndex = 1)]
    public string? IdentityToken { get; set; }

    // Java: language (string, not nullable), OffsetIndex 2
    [PacketProperty(OffsetIndex = 2)]
    public string Language { get; set; } = "";

    // Java: referralData (byte[], nullable, bit 1), OffsetIndex 3
    [PacketProperty(BitIndex = 1, OffsetIndex = 3)]
    public byte[]? ReferralData { get; set; }

    // Java: referralSource (HostAddress, nullable, bit 2), OffsetIndex 4
    [PacketProperty(BitIndex = 2, OffsetIndex = 4)]
    public HostAddress? ReferralSource { get; set; }
}
