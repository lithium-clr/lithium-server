using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(EnumStringConverter<EntityMatcherType>))]
public enum EntityMatcherType : byte
{
    Server = 0,
    VulnerableMatcher = 1,
    Player = 2
}

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 2,
    VariableFieldCount = 0,
    VariableBlockStart = 2,
    MaxSize = 2
)]
public sealed class EntityMatcher : INetworkSerializable
{
    [JsonPropertyName("type")]   public EntityMatcherType Type { get; set; } = EntityMatcherType.Server;
    [JsonPropertyName("invert")] public bool Invert { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(Type);
        writer.WriteBoolean(Invert);
    }

    public void Deserialize(PacketReader reader)
    {
        Type = reader.ReadEnum<EntityMatcherType>();
        Invert = reader.ReadBoolean();
    }

    public int ComputeSize() => 2;
}

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 5,
    VariableFieldCount = 1,
    VariableBlockStart = 5,
    MaxSize = 8192010
)]
public sealed class HitEntity : INetworkSerializable
{
    [JsonPropertyName("next")]     public int Next { get; set; }
    [JsonPropertyName("matchers")] public EntityMatcher[]? Matchers { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Matchers is not null) bits.SetBit(1);
        writer.WriteBits(bits);

        writer.WriteInt32(Next);

        if (Matchers is not null)
        {
            writer.WriteVarInt(Matchers.Length);
            foreach (var m in Matchers) m.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        Next = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Matchers = new EntityMatcher[count];
            for (var i = 0; i < count; i++)
            {
                Matchers[i] = new EntityMatcher();
                Matchers[i].Deserialize(reader);
            }
        }
    }

    public int ComputeSize()
    {
        int size = 5;
        if (Matchers is not null) size += PacketWriter.GetVarIntSize(Matchers.Length) + Matchers.Length * 2;
        return size;
    }
}
