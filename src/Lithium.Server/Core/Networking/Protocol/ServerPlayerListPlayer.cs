using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 37,
    VariableFieldCount = 0,
    VariableBlockStart = 37,
    MaxSize = 16384042
)]
public sealed class ServerPlayerListPlayer : INetworkSerializable
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("worldUuid")]
    public Guid? WorldUuid { get; set; }

    [JsonPropertyName("ping")]
    public int Ping { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (WorldUuid is not null) bits.SetBit(1);
        if (Username is not null) bits.SetBit(2);
        
        writer.WriteBits(bits);
        writer.WriteGuid(Uuid);
        
        if (WorldUuid is not null)
        {
            writer.WriteGuid(WorldUuid.Value);
        }
        else
        {
            writer.WriteZero(16);
        }

        writer.WriteInt32(Ping);

        if (Username is not null)
        {
            writer.WriteVarUtf8String(Username, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Uuid = reader.ReadGuid();

        if (bits.IsSet(1))
        {
            WorldUuid = reader.ReadGuid();
        }
        else
        {
            for (var i = 0; i < 16; i++) reader.ReadUInt8();
        }

        Ping = reader.ReadInt32();

        if (bits.IsSet(2))
        {
            Username = reader.ReadUtf8String();
        }
    }
}
