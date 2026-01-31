using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 41,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 1677721600
)]
public sealed class UpdateBlockHitboxesPacket : INetworkSerializable
{
    [JsonPropertyName("type")] public UpdateType Type { get; set; } = UpdateType.Init;
    [JsonPropertyName("maxId")] public int MaxId { get; set; }

    [JsonPropertyName("blockBaseHitboxes")]
    public Dictionary<int, Hitbox[]>? BlockBaseHitboxes { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (BlockBaseHitboxes is not null) bits.SetBit(1);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);
        writer.WriteInt32(MaxId);

        if (BlockBaseHitboxes is not null)
        {
            writer.WriteVarInt(BlockBaseHitboxes.Count);
            foreach (var (key, value) in BlockBaseHitboxes)
            {
                writer.WriteInt32(key);
                writer.WriteVarInt(value.Length);
                foreach (var hitbox in value)
                {
                    hitbox.Serialize(writer);
                }
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Type = reader.ReadEnum<UpdateType>();
        MaxId = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            var dictCount = reader.ReadVarInt32();
            BlockBaseHitboxes = new Dictionary<int, Hitbox[]>(dictCount);
            for (var i = 0; i < dictCount; i++)
            {
                var key = reader.ReadInt32();
                var arrayCount = reader.ReadVarInt32();
                var hitboxes = new Hitbox[arrayCount];
                for (var j = 0; j < arrayCount; j++)
                {
                    hitboxes[j] = reader.ReadObject<Hitbox>();
                }

                BlockBaseHitboxes[key] = hitboxes;
            }
        }
    }
}