using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 85,
    VariableFieldCount = 0,
    VariableBlockStart = 85,
    MaxSize = 85
)]
public sealed class ParticleAttractor : INetworkSerializable
{
    [JsonPropertyName("position")]
    public Vector3Float? Position { get; set; }

    [JsonPropertyName("radialAxis")]
    public Vector3Float? RadialAxis { get; set; }

    [JsonPropertyName("trailPositionMultiplier")]
    public float TrailPositionMultiplier { get; set; }

    [JsonPropertyName("radius")]
    public float Radius { get; set; }

    [JsonPropertyName("radialAcceleration")]
    public float RadialAcceleration { get; set; }

    [JsonPropertyName("radialTangentAcceleration")]
    public float RadialTangentAcceleration { get; set; }

    [JsonPropertyName("linearAcceleration")]
    public Vector3Float? LinearAcceleration { get; set; }

    [JsonPropertyName("radialImpulse")]
    public float RadialImpulse { get; set; }

    [JsonPropertyName("radialTangentImpulse")]
    public float RadialTangentImpulse { get; set; }

    [JsonPropertyName("linearImpulse")]
    public Vector3Float? LinearImpulse { get; set; }

    [JsonPropertyName("dampingMultiplier")]
    public Vector3Float? DampingMultiplier { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Position is not null) bits.SetBit(1);
        if (RadialAxis is not null) bits.SetBit(2);
        if (LinearAcceleration is not null) bits.SetBit(4);
        if (LinearImpulse is not null) bits.SetBit(8);
        if (DampingMultiplier is not null) bits.SetBit(16);

        writer.WriteBits(bits);

        if (Position is not null) Position.Serialize(writer);
        else writer.WriteZero(12);

        if (RadialAxis is not null) RadialAxis.Serialize(writer);
        else writer.WriteZero(12);

        writer.WriteFloat32(TrailPositionMultiplier);
        writer.WriteFloat32(Radius);
        writer.WriteFloat32(RadialAcceleration);
        writer.WriteFloat32(RadialTangentAcceleration);

        if (LinearAcceleration is not null) LinearAcceleration.Serialize(writer);
        else writer.WriteZero(12);

        writer.WriteFloat32(RadialImpulse);
        writer.WriteFloat32(RadialTangentImpulse);

        if (LinearImpulse is not null) LinearImpulse.Serialize(writer);
        else writer.WriteZero(12);

        if (DampingMultiplier is not null) DampingMultiplier.Serialize(writer);
        else writer.WriteZero(12);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1)) Position = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        if (bits.IsSet(2)) RadialAxis = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        TrailPositionMultiplier = reader.ReadFloat32();
        Radius = reader.ReadFloat32();
        RadialAcceleration = reader.ReadFloat32();
        RadialTangentAcceleration = reader.ReadFloat32();

        if (bits.IsSet(4)) LinearAcceleration = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        RadialImpulse = reader.ReadFloat32();
        RadialTangentImpulse = reader.ReadFloat32();

        if (bits.IsSet(8)) LinearImpulse = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        if (bits.IsSet(16)) DampingMultiplier = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);
    }
}