using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class HitboxCollisionConfig : INetworkSerializable
{
    [JsonPropertyName("collisionType")]
    [JsonConverter(typeof(JsonStringEnumConverter<CollisionType>))]
    public CollisionType CollisionType { get; set; } = CollisionType.Hard;

    [JsonPropertyName("softCollisionOffsetRatio")]
    public float SoftCollisionOffsetRatio { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(CollisionType);
        writer.WriteFloat32(SoftCollisionOffsetRatio);
    }

    public void Deserialize(PacketReader reader)
    {
        CollisionType = reader.ReadEnum<CollisionType>();
        SoftCollisionOffsetRatio = reader.ReadFloat32();
    }
}
