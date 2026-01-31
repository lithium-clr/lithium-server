using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 12, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 2, VariableBlockStart = 9,
    MaxSize = 49171)]
public sealed class AuthTokenPacket : INetworkSerializable
{
    public string? AccessToken { get; set; }
    public string? ServerAuthorizationGrant { get; set; }
    
    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
            AccessToken = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            ServerAuthorizationGrant = reader.ReadVarUtf8StringAt(offsets[1]);
    }
}