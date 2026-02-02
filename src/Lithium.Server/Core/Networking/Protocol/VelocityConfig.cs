using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<VelocityThresholdStyle>))]
public enum VelocityThresholdStyle : byte
{
    Linear = 0,
    Exp = 1
}

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 21,
    VariableFieldCount = 0,
    VariableBlockStart = 21,
    MaxSize = 21
)]
public sealed class VelocityConfig : INetworkSerializable
{
    [JsonPropertyName("groundResistance")]    public float                  GroundResistance    { get; set; }
    [JsonPropertyName("groundResistanceMax")] public float                  GroundResistanceMax { get; set; }
    [JsonPropertyName("airResistance")]       public float                  AirResistance       { get; set; }
    [JsonPropertyName("airResistanceMax")]    public float                  AirResistanceMax    { get; set; }
    [JsonPropertyName("threshold")]           public float                  Threshold           { get; set; }
    [JsonPropertyName("style")]               public VelocityThresholdStyle Style               { get; set; } = VelocityThresholdStyle.Linear;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(GroundResistance);
        writer.WriteFloat32(GroundResistanceMax);
        writer.WriteFloat32(AirResistance);
        writer.WriteFloat32(AirResistanceMax);
        writer.WriteFloat32(Threshold);
        writer.WriteEnum(Style);
    }

    public void Deserialize(PacketReader reader)
    {
        GroundResistance    = reader.ReadFloat32();
        GroundResistanceMax = reader.ReadFloat32();
        AirResistance       = reader.ReadFloat32();
        AirResistanceMax    = reader.ReadFloat32();
        Threshold           = reader.ReadFloat32();
        Style               = reader.ReadEnum<VelocityThresholdStyle>();
    }

    public int ComputeSize() => 21;
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 18,
    VariableFieldCount = 0,
    VariableBlockStart = 18,
    MaxSize = 18
)]
public sealed class AppliedForce : INetworkSerializable
{
    [JsonPropertyName("direction")]      public Vector3Float? Direction      { get; set; }
    [JsonPropertyName("adjustVertical")] public bool         AdjustVertical { get; set; }
    [JsonPropertyName("force")]          public float        Force          { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Direction is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        if (Direction is not null) Direction.Serialize(writer);
        else writer.WriteZero(12);

        writer.WriteBoolean(AdjustVertical);
        writer.WriteFloat32(Force);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        if (bits.IsSet(1)) Direction = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);

        AdjustVertical = reader.ReadBoolean();
        Force          = reader.ReadFloat32();
    }

    public int ComputeSize() => 18;
}
