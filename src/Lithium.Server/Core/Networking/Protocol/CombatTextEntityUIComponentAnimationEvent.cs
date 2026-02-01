using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class CombatTextEntityUIComponentAnimationEvent : INetworkSerializable
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<CombatTextEntityUIAnimationEventType>))]
    public CombatTextEntityUIAnimationEventType Type { get; set; } = CombatTextEntityUIAnimationEventType.Scale;

    [JsonPropertyName("startAt")]
    public float StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public float EndAt { get; set; }

    [JsonPropertyName("startScale")]
    public float StartScale { get; set; }

    [JsonPropertyName("endScale")]
    public float EndScale { get; set; }

    [JsonPropertyName("positionOffset")]
    public Vector2Float? PositionOffset { get; set; }

    [JsonPropertyName("startOpacity")]
    public float StartOpacity { get; set; }

    [JsonPropertyName("endOpacity")]
    public float EndOpacity { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (PositionOffset is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteEnum(Type);
        writer.WriteFloat32(StartAt);
        writer.WriteFloat32(EndAt);
        writer.WriteFloat32(StartScale);
        writer.WriteFloat32(EndScale);

        if (PositionOffset is not null)
        {
            PositionOffset.Serialize(writer);
        }
        else
        {
            writer.WriteZero(8);
        }

        writer.WriteFloat32(StartOpacity);
        writer.WriteFloat32(EndOpacity);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Type = reader.ReadEnum<CombatTextEntityUIAnimationEventType>();
        StartAt = reader.ReadFloat32();
        EndAt = reader.ReadFloat32();
        StartScale = reader.ReadFloat32();
        EndScale = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            PositionOffset = reader.ReadObject<Vector2Float>();
        }
        else
        {
            reader.ReadFloat32();
            reader.ReadFloat32();
        }

        StartOpacity = reader.ReadFloat32();
        EndOpacity = reader.ReadFloat32();
    }
}
