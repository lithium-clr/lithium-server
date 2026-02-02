using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 21,
    VariableFieldCount = 1,
    VariableBlockStart = 21,
    MaxSize = 1677721600
)]
public sealed class AngledDamage : INetworkSerializable
{
    [JsonPropertyName("angle")]          public double          Angle          { get; set; }
    [JsonPropertyName("angleDistance")]  public double          AngleDistance  { get; set; }
    [JsonPropertyName("damageEffects")]  public DamageEffects? DamageEffects { get; set; }
    [JsonPropertyName("next")]           public int             Next           { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (DamageEffects is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteFloat64(Angle);
        writer.WriteFloat64(AngleDistance);
        writer.WriteInt32(Next);

        if (DamageEffects is not null) DamageEffects.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Angle         = reader.ReadFloat64();
        AngleDistance = reader.ReadFloat64();
        Next          = reader.ReadInt32();

        if (bits.IsSet(1)) { DamageEffects = new DamageEffects(); DamageEffects.Deserialize(reader); }
    }

    public int ComputeSize()
    {
        int size = 21;
        if (DamageEffects is not null) size += DamageEffects.ComputeSize();
        return size;
    }
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 9,
    VariableFieldCount = 1,
    VariableBlockStart = 9,
    MaxSize = 1677721600
)]
public sealed class TargetedDamage : INetworkSerializable
{
    [JsonPropertyName("index")]         public int             Index         { get; set; }
    [JsonPropertyName("damageEffects")] public DamageEffects? DamageEffects { get; set; }
    [JsonPropertyName("next")]          public int             Next          { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (DamageEffects is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteInt32(Index);
        writer.WriteInt32(Next);

        if (DamageEffects is not null) DamageEffects.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Index = reader.ReadInt32();
        Next  = reader.ReadInt32();

        if (bits.IsSet(1)) { DamageEffects = new DamageEffects(); DamageEffects.Deserialize(reader); }
    }

    public int ComputeSize()
    {
        int size = 9;
        if (DamageEffects is not null) size += DamageEffects.ComputeSize();
        return size;
    }
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 13,
    VariableFieldCount = 1,
    VariableBlockStart = 13,
    MaxSize = 16384018
)]
public sealed class EntityStatOnHit : INetworkSerializable
{
    [JsonPropertyName("entityStatIndex")]           public int     EntityStatIndex           { get; set; }
    [JsonPropertyName("amount")]                    public float   Amount                    { get; set; }
    [JsonPropertyName("multipliersPerEntitiesHit")] public float[]? MultipliersPerEntitiesHit { get; set; }
    [JsonPropertyName("multiplierPerExtraEntityHit")] public float   MultiplierPerExtraEntityHit { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (MultipliersPerEntitiesHit is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteInt32(EntityStatIndex);
        writer.WriteFloat32(Amount);
        writer.WriteFloat32(MultiplierPerExtraEntityHit);

        if (MultipliersPerEntitiesHit is not null)
        {
            writer.WriteVarInt(MultipliersPerEntitiesHit.Length);
            foreach (var m in MultipliersPerEntitiesHit) writer.WriteFloat32(m);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        EntityStatIndex           = reader.ReadInt32();
        Amount                    = reader.ReadFloat32();
        MultiplierPerExtraEntityHit = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            MultipliersPerEntitiesHit = new float[count];
            for (var i = 0; i < count; i++) MultipliersPerEntitiesHit[i] = reader.ReadFloat32();
        }
    }

    public int ComputeSize()
    {
        int size = 13;
        if (MultipliersPerEntitiesHit is not null) size += PacketWriter.GetVarIntSize(MultipliersPerEntitiesHit.Length) + MultipliersPerEntitiesHit.Length * 4;
        return size;
    }
}
