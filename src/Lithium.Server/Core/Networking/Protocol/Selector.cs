using System.Text.Json.Serialization;
using Lithium.Server.Core.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(SelectorConverter))]
[Packet(MaxSize = 42)]
public abstract class Selector : INetworkSerializable
{
    [JsonIgnore]
    [JsonPropertyName("typeId")]
    public abstract int TypeId { get; }

    public abstract void Serialize(PacketWriter writer);
    public abstract void Deserialize(PacketReader reader);
    public abstract int ComputeSize();

    public void SerializeWithTypeId(PacketWriter writer)
    {
        writer.WriteVarInt(TypeId);
        Serialize(writer);
    }

    public int ComputeSizeWithTypeId() => PacketWriter.GetVarIntSize(TypeId) + ComputeSize();

    public static Selector ReadPolymorphic(PacketReader reader)
    {
        var typeId = reader.ReadVarInt32();
        Selector selector = typeId switch
        {
            0 => new AOECircleSelector(),
            1 => new AOECylinderSelector(),
            2 => new RaycastSelector(),
            3 => new HorizontalSelector(),
            4 => new StabSelector(),
            _ => throw new NotSupportedException($"Selector type {typeId} not supported.")
        };
        selector.Deserialize(reader);
        return selector;
    }
}

public sealed class SelectorConverter : PolymorphicConverter<Selector>
{
    protected override string DiscriminatorPropertyName => "typeId";

    protected override Dictionary<int, Type> DerivedTypes => new()
    {
        { 0, typeof(AOECircleSelector) },
        { 1, typeof(AOECylinderSelector) },
        { 2, typeof(RaycastSelector) },
        { 3, typeof(HorizontalSelector) },
        { 4, typeof(StabSelector) }
    };
}

public sealed class AOECircleSelector : Selector
{
    public override int TypeId => 0;
    [JsonPropertyName("range")]  public float Range { get; set; }
    [JsonPropertyName("offset")] public Vector3Float? Offset { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Offset is not null) bits.SetBit(1);
        writer.WriteBits(bits);
        writer.WriteFloat32(Range);
        if (Offset is not null) Offset.Serialize(writer);
        else writer.WriteZero(12);
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Range = reader.ReadFloat32();
        if (bits.IsSet(1)) Offset = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);
    }

    public override int ComputeSize() => 17;
}

public sealed class AOECylinderSelector : Selector
{
    public override int TypeId => 1;
    [JsonPropertyName("range")]  public float Range { get; set; }
    [JsonPropertyName("height")] public float Height { get; set; }
    [JsonPropertyName("offset")] public Vector3Float? Offset { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Offset is not null) bits.SetBit(1);
        writer.WriteBits(bits);
        writer.WriteFloat32(Range);
        writer.WriteFloat32(Height);
        if (Offset is not null) Offset.Serialize(writer);
        else writer.WriteZero(12);
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Range = reader.ReadFloat32();
        Height = reader.ReadFloat32();
        if (bits.IsSet(1)) Offset = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);
    }

    public override int ComputeSize() => 21;
}

public sealed class RaycastSelector : Selector
{
    public override int TypeId => 2;
    [JsonPropertyName("offset")]                       public Vector3Float? Offset { get; set; }
    [JsonPropertyName("distance")]                     public int Distance { get; set; }
    [JsonPropertyName("blockTagIndex")]                public int BlockTagIndex { get; set; } = int.MinValue;
    [JsonPropertyName("ignoreFluids")]                 public bool IgnoreFluids { get; set; }
    [JsonPropertyName("ignoreEmptyCollisionMaterial")] public bool IgnoreEmptyCollisionMaterial { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Offset is not null) bits.SetBit(1);
        writer.WriteBits(bits);
        if (Offset is not null) Offset.Serialize(writer);
        else writer.WriteZero(12);
        writer.WriteInt32(Distance);
        writer.WriteInt32(BlockTagIndex);
        writer.WriteBoolean(IgnoreFluids);
        writer.WriteBoolean(IgnoreEmptyCollisionMaterial);
    }

    public override void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        if (bits.IsSet(1)) Offset = reader.ReadObject<Vector3Float>();
        else reader.SeekTo(reader.GetPosition() + 12);
        Distance = reader.ReadInt32();
        BlockTagIndex = reader.ReadInt32();
        IgnoreFluids = reader.ReadBoolean();
        IgnoreEmptyCollisionMaterial = reader.ReadBoolean();
    }

    public override int ComputeSize() => 23;
}

[JsonConverter(typeof(EnumStringConverter<HorizontalSelectorDirection>))]
public enum HorizontalSelectorDirection : byte
{
    ToLeft = 0,
    ToRight = 1
}

public sealed class HorizontalSelector : Selector
{
    public override int TypeId => 3;
    [JsonPropertyName("extendTop")]      public float ExtendTop      { get; set; }
    [JsonPropertyName("extendBottom")]   public float ExtendBottom   { get; set; }
    [JsonPropertyName("yawLength")]      public float YawLength      { get; set; }
    [JsonPropertyName("yawStartOffset")] public float YawStartOffset { get; set; }
    [JsonPropertyName("pitchOffset")]    public float PitchOffset    { get; set; }
    [JsonPropertyName("rollOffset")]     public float RollOffset     { get; set; }
    [JsonPropertyName("startDistance")]  public float StartDistance  { get; set; }
    [JsonPropertyName("endDistance")]    public float EndDistance    { get; set; }
    [JsonPropertyName("direction")]      public HorizontalSelectorDirection Direction { get; set; } = HorizontalSelectorDirection.ToLeft;
    [JsonPropertyName("testLineOfSight")] public bool TestLineOfSight { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(ExtendTop);
        writer.WriteFloat32(ExtendBottom);
        writer.WriteFloat32(YawLength);
        writer.WriteFloat32(YawStartOffset);
        writer.WriteFloat32(PitchOffset);
        writer.WriteFloat32(RollOffset);
        writer.WriteFloat32(StartDistance);
        writer.WriteFloat32(EndDistance);
        writer.WriteEnum(Direction);
        writer.WriteBoolean(TestLineOfSight);
    }

    public override void Deserialize(PacketReader reader)
    {
        ExtendTop       = reader.ReadFloat32();
        ExtendBottom    = reader.ReadFloat32();
        YawLength       = reader.ReadFloat32();
        YawStartOffset  = reader.ReadFloat32();
        PitchOffset     = reader.ReadFloat32();
        RollOffset      = reader.ReadFloat32();
        StartDistance   = reader.ReadFloat32();
        EndDistance     = reader.ReadFloat32();
        Direction       = reader.ReadEnum<HorizontalSelectorDirection>();
        TestLineOfSight = reader.ReadBoolean();
    }

    public override int ComputeSize() => 34;
}

public sealed class StabSelector : Selector
{
    public override int TypeId => 4;
    [JsonPropertyName("extendTop")]      public float ExtendTop      { get; set; }
    [JsonPropertyName("extendBottom")]   public float ExtendBottom   { get; set; }
    [JsonPropertyName("extendLeft")]     public float ExtendLeft     { get; set; }
    [JsonPropertyName("extendRight")]    public float ExtendRight    { get; set; }
    [JsonPropertyName("yawOffset")]      public float YawOffset      { get; set; }
    [JsonPropertyName("pitchOffset")]    public float PitchOffset    { get; set; }
    [JsonPropertyName("rollOffset")]     public float RollOffset     { get; set; }
    [JsonPropertyName("startDistance")]  public float StartDistance  { get; set; }
    [JsonPropertyName("endDistance")]    public float EndDistance    { get; set; }
    [JsonPropertyName("testLineOfSight")] public bool TestLineOfSight { get; set; }

    public override void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(ExtendTop);
        writer.WriteFloat32(ExtendBottom);
        writer.WriteFloat32(ExtendLeft);
        writer.WriteFloat32(ExtendRight);
        writer.WriteFloat32(YawOffset);
        writer.WriteFloat32(PitchOffset);
        writer.WriteFloat32(RollOffset);
        writer.WriteFloat32(StartDistance);
        writer.WriteFloat32(EndDistance);
        writer.WriteBoolean(TestLineOfSight);
    }

    public override void Deserialize(PacketReader reader)
    {
        ExtendTop       = reader.ReadFloat32();
        ExtendBottom    = reader.ReadFloat32();
        ExtendLeft      = reader.ReadFloat32();
        ExtendRight     = reader.ReadFloat32();
        YawOffset       = reader.ReadFloat32();
        PitchOffset     = reader.ReadFloat32();
        RollOffset      = reader.ReadFloat32();
        StartDistance   = reader.ReadFloat32();
        EndDistance     = reader.ReadFloat32();
        TestLineOfSight = reader.ReadBoolean();
    }

    public override int ComputeSize() => 37;
}
