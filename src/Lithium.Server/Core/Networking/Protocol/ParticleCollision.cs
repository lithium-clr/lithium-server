using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 3,
    VariableFieldCount = 0,
    VariableBlockStart = 3,
    MaxSize = 3
)]
public sealed class ParticleCollision : INetworkSerializable
{
    [JsonPropertyName("blockType")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleCollisionBlockType>))]
    public ParticleCollisionBlockType BlockType { get; set; } = ParticleCollisionBlockType.None;

    [JsonPropertyName("action")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleCollisionAction>))]
    public ParticleCollisionAction Action { get; set; } = ParticleCollisionAction.Expire;

    [JsonPropertyName("particleRotationInfluence")]
    [JsonConverter(typeof(JsonStringEnumConverter<ParticleRotationInfluence>))]
    public ParticleRotationInfluence ParticleRotationInfluence { get; set; } = ParticleRotationInfluence.None;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(BlockType);
        writer.WriteEnum(Action);
        writer.WriteEnum(ParticleRotationInfluence);
    }

    public void Deserialize(PacketReader reader)
    {
        BlockType = reader.ReadEnum<ParticleCollisionBlockType>();
        Action = reader.ReadEnum<ParticleCollisionAction>();
        ParticleRotationInfluence = reader.ReadEnum<ParticleRotationInfluence>();
    }
}