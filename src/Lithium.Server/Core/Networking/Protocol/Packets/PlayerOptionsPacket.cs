using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(Id = 33, NullableBitFieldSize = 1, FixedBlockSize = 1, VariableFieldCount = 1, VariableBlockStart = 1, MaxSize = 327680184)]
public sealed class PlayerOptionsPacket : INetworkSerializable
{
    [JsonPropertyName("skin")]
    public PlayerSkin? Skin { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        
        if (Skin is not null)
            bits.SetBit(1);
        
        writer.WriteBits(bits);
        
        if (Skin is not null)
            Skin.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
            Skin = new PlayerSkin();
        
        if (Skin is not null)
            Skin.Deserialize(reader);
    }
}