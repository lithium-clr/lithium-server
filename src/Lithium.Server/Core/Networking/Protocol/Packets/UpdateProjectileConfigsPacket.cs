using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 85,
    IsCompressed = true,
    NullableBitFieldSize = 1,
    FixedBlockSize = 2,
    VariableFieldCount = 2,
    VariableBlockStart = 10,
    MaxSize = 1677721600
)]
public sealed class UpdateProjectileConfigsPacket : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<UpdateType>))]
    public UpdateType Type { get; set; } = UpdateType.Init;

    [JsonPropertyName("configs")] public Dictionary<string, ProjectileConfig>? Configs { get; set; }
    [JsonPropertyName("removedConfigs")] public string[]? RemovedConfigs { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Configs is not null) bits.SetBit(1);
        if (RemovedConfigs is not null) bits.SetBit(2);

        writer.WriteBits(bits);
        writer.WriteEnum(Type);

        var configsOffsetSlot = writer.ReserveOffset();
        var removedConfigsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Configs is not null)
        {
            writer.WriteOffsetAt(configsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(Configs.Count);
            foreach (var kvp in Configs)
            {
                writer.WriteVarUtf8String(kvp.Key, 4096000);
                kvp.Value.Serialize(writer);
            }
        }
        else writer.WriteOffsetAt(configsOffsetSlot, -1);

        if (RemovedConfigs is not null)
        {
            writer.WriteOffsetAt(removedConfigsOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarInt(RemovedConfigs.Length);
            foreach (var item in RemovedConfigs)
                writer.WriteVarUtf8String(item, 4096000);
        }
        else writer.WriteOffsetAt(removedConfigsOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Type = reader.ReadEnum<UpdateType>();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            // Manual dictionary deserialization for complex objects
            var savedPos = reader.GetPosition();
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            
            var count = reader.ReadVarInt32();
            Configs = new Dictionary<string, ProjectileConfig>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadUtf8String();
                var value = new ProjectileConfig();
                value.Deserialize(reader);
                Configs[key] = value;
            }
            
            reader.SeekTo(savedPos);
        }

        if (bits.IsSet(2))
        {
            RemovedConfigs = reader.ReadArrayAt(offsets[1], r => r.ReadUtf8String());
        }
    }
}