using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 0, NullableBitFieldSize = 1, FixedBlockSize = 46, VariableFieldCount = 5, VariableBlockStart = 66,
    MaxSize = 38013)]
public sealed class ConnectPacket : INetworkSerializable
{
    public int ProtocolCrc { get; set; }
    public int ProtocolBuildNumber { get; set; }
    public string ClientVersion { get; set; } = "";
    public ClientType ClientType { get; set; }
    public Guid Uuid { get; set; }
    public string Username { get; set; } = "";
    public string? IdentityToken { get; set; }
    public string Language { get; set; } = "";
    public byte[]? ReferralData { get; set; }
    public HostAddress? ReferralSource { get; set; }

    // public void Deserialize(PacketReader reader)
    // {
    //     var bits = reader.ReadBits();
    //
    //     ProtocolCrc = reader.ReadInt32();
    //     ProtocolBuildNumber = reader.ReadInt32();
    //     ClientVersion = reader.ReadFixedAsciiString(20);
    //     ClientType = reader.ReadEnum<ClientType>();
    //     Uuid = reader.ReadGuid();
    //
    //     Username = reader.ReadVarAsciiString();
    //
    //     if (bits.IsSet(1))
    //     {
    //         IdentityToken = reader.ReadVarUtf8String();
    //     }
    //
    //     Language = reader.ReadVarAsciiString();
    //
    //     if (bits.IsSet(2))
    //     {
    //         ReferralData = reader.ReadVarBytes();
    //     }
    //
    //     if (bits.IsSet(4))
    //     {
    //         ReferralSource = reader.ReadObject<HostAddress>();
    //     }
    // }
    
    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Champs fixes
        ProtocolCrc = reader.ReadInt32();
        ProtocolBuildNumber = reader.ReadInt32();
        ClientVersion = reader.ReadFixedAsciiString(20);
        ClientType = reader.ReadEnum<ClientType>();
        Uuid = reader.ReadGuid();

        // Lire les offsets
        var offsets = reader.ReadOffsets(5);

        // Lire les champs variables avec offsets
        Username = reader.ReadVarAsciiStringAt(offsets[0]);
        if (bits.IsSet(1)) IdentityToken = reader.ReadVarUtf8StringAt(offsets[1]);
        Language = reader.ReadVarAsciiStringAt(offsets[2]);
        if (bits.IsSet(2)) ReferralData = reader.ReadVarBytesAt(offsets[3]);
        if (bits.IsSet(4)) ReferralSource = reader.ReadObjectAt<HostAddress>(offsets[4]);
    }
}