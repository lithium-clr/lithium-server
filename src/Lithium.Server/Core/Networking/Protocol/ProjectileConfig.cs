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
        // 1. Nullable bits
        byte nullBits = 0;
        if (PhysicsConfig != null) nullBits |= 1;
        if (SpawnOffset != null) nullBits |= 2;
        if (RotationOffset != null) nullBits |= 4;
        if (Model != null) nullBits |= 8;
        if (Interactions != null) nullBits |= 16;

        writer.WriteUInt8(nullBits);

        // 2. Fixed block
        if (PhysicsConfig != null)
            PhysicsConfig.Serialize(writer);
        else
            writer.WriteZero(122); // PhysicsConfig padding

        writer.WriteFloat64(LaunchForce);

        if (SpawnOffset != null)
            SpawnOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        if (RotationOffset != null)
            RotationOffset.Serialize(writer);
        else
            writer.WriteZero(12);

        writer.WriteInt32(LaunchLocalSoundEventIndex);
        writer.WriteInt32(ProjectileSoundEventIndex);

        // 3. Reserve offset slots
        int modelOffsetSlot = writer.ReserveOffset();
        int interactionsOffsetSlot = writer.ReserveOffset();

        int varBlockStart = writer.Position;

        // 4. Variable block
        if (Model != null)
        {
            writer.WriteOffsetAt(modelOffsetSlot, writer.Position - varBlockStart);
            Model.Serialize(writer);
        }
        else
        {
            writer.WriteOffsetAt(modelOffsetSlot, -1);
        }

        if (Interactions != null)
        {
            writer.WriteOffsetAt(interactionsOffsetSlot, writer.Position - varBlockStart);

            if (Interactions.Count > 4096000)
                throw new Exception("Interactions too large");

            writer.WriteVarInt(Interactions.Count);

            foreach (var kvp in Interactions)
            {
                writer.WriteUInt8((byte)kvp.Key);
                writer.WriteInt32(kvp.Value);
            }
        }
        else
        {
            writer.WriteOffsetAt(interactionsOffsetSlot, -1);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        // Fixed Block
        if (bits.IsSet(1))
        {
            PhysicsConfig = reader.ReadObject<PhysicsConfig>();
        }
        else
        {
            reader.ReadBytes(122);
        }

        LaunchForce = reader.ReadFloat64();

        if (bits.IsSet(2))
        {
            SpawnOffset = reader.ReadObject<Vector3Float>();
        }
        else
        {
            reader.ReadBytes(12);
        }

        if (bits.IsSet(4))
        {
            RotationOffset = reader.ReadObject<Direction>();
        }
        else
        {
            reader.ReadBytes(12);
        }

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