using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 163,
    VariableFieldCount = 2,
    VariableBlockStart = 171,
    MaxSize = 1677721600
)]
public sealed class ProjectileConfig : INetworkSerializable
{
    [JsonPropertyName("physicsConfig")] public PhysicsConfig? PhysicsConfig { get; set; }
    [JsonPropertyName("model")] public Model? Model { get; set; }
    [JsonPropertyName("launchForce")] public double LaunchForce { get; set; }
    [JsonPropertyName("spawnOffset")] public Vector3Float? SpawnOffset { get; set; }
    [JsonPropertyName("rotationOffset")] public Direction? RotationOffset { get; set; }

    [JsonPropertyName("interactions")]
    [JsonConverter(typeof(InteractionTypeKeyDictionaryConverter<int>))]
    public Dictionary<InteractionType, int>? Interactions { get; set; }

    [JsonPropertyName("launchLocalSoundEventIndex")]
    public int LaunchLocalSoundEventIndex { get; set; }

    [JsonPropertyName("projectileSoundEventIndex")]
    public int ProjectileSoundEventIndex { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (PhysicsConfig is not null) bits.SetBit(1);
        if (SpawnOffset is not null) bits.SetBit(2);
        if (RotationOffset is not null) bits.SetBit(4);
        if (Model is not null) bits.SetBit(8);
        if (Interactions is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        // Fixed Block
        if (PhysicsConfig is not null) PhysicsConfig.Serialize(writer);
        else writer.WriteZero(122);
        writer.WriteFloat64(LaunchForce);
        if (SpawnOffset is not null) SpawnOffset.Value.Serialize(writer);
        else writer.WriteZero(12);
        if (RotationOffset is not null) RotationOffset.Value.Serialize(writer);
        else writer.WriteZero(12);
        writer.WriteInt32(LaunchLocalSoundEventIndex);
        writer.WriteInt32(ProjectileSoundEventIndex);

        // Reserve Offsets
        var modelOffset = writer.ReserveOffset();
        var interactionsOffset = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        if (Model is not null)
        {
            writer.WriteOffsetAt(modelOffset, writer.Position - varBlockStart);
            Model.Serialize(writer);
        }
        else writer.WriteOffsetAt(modelOffset, -1);

        if (Interactions is not null)
        {
            writer.WriteOffsetAt(interactionsOffset, writer.Position - varBlockStart);
            writer.WriteVarInt(Interactions.Count);
            foreach (var (key, value) in Interactions)
            {
                writer.WriteEnum(key);
                writer.WriteInt32(value);
            }
        }
        else writer.WriteOffsetAt(interactionsOffset, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        if (bits.IsSet(1)) PhysicsConfig = reader.ReadObject<PhysicsConfig>();
        else reader.ReadBytes(122);
        LaunchForce = reader.ReadFloat64();
        if (bits.IsSet(2)) SpawnOffset = reader.ReadObject<Vector3Float>();
        else reader.ReadBytes(12);
        if (bits.IsSet(4)) RotationOffset = reader.ReadObject<Direction>();
        else reader.ReadBytes(12);
        LaunchLocalSoundEventIndex = reader.ReadInt32();
        ProjectileSoundEventIndex = reader.ReadInt32();

        // Read Offsets
        var offsets = reader.ReadOffsets(2);

        // Variable Block
        if (bits.IsSet(8)) Model = reader.ReadObjectAt<Model>(offsets[0]);
        if (bits.IsSet(16))
            Interactions = reader.ReadDictionaryAt(offsets[1], r => r.ReadEnum<InteractionType>(), r => r.ReadInt32());
    }
}