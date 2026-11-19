using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 226,
    IsCompressed = false,
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 0,
    VariableBlockStart = 1,
    MaxSize = 131072006
)]
public sealed class UpdateServerPlayerListPacket : INetworkSerializable
{
    [JsonPropertyName("players")]
    public ServerPlayerListUpdate[]? Players { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Players is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        if (Players is not null)
        {
            writer.WriteVarInt(Players.Length);
            foreach (var player in Players)
            {
                player.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Players = new ServerPlayerListUpdate[count];
            for (var i = 0; i < count; i++)
            {
                Players[i] = new ServerPlayerListUpdate();
                Players[i].Deserialize(reader);
            }
        }
    }
}
