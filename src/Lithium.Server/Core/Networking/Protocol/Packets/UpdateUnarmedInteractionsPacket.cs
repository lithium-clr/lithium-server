using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 68,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 1,
    VariableBlockStart = 2,
    MaxSize = 20480007
)]
public sealed class UpdateUnarmedInteractionsPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("interactions")] public Dictionary<InteractionType, int>? Interactions { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Interactions is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        if (Interactions is not null)
        {
            writer.WriteVarInt(Interactions.Count);
            foreach (var (key, value) in Interactions)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Interactions = new Dictionary<InteractionType, int>(count);
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadEnum<InteractionType>();
                var value = reader.ReadInt32();
                Interactions[key] = value;
            }
        }
    }
}