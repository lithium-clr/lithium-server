using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 13,
    VariableFieldCount = 1,
    VariableBlockStart = 13,
    MaxSize = 16384018
)]
public sealed class WeatherParticle : INetworkSerializable
{
    [JsonPropertyName("systemId")]
    public string? SystemId { get; set; }

    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    [JsonPropertyName("scale")]
    public float Scale { get; set; }

    [JsonPropertyName("isOvergroundOnly")]
    public bool IsOvergroundOnly { get; set; }

    [JsonPropertyName("positionOffsetMultiplier")]
    public float PositionOffsetMultiplier { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Color is not null) bits.SetBit(1);
        if (SystemId is not null) bits.SetBit(2);

        writer.WriteBits(bits);

        if (Color is not null)
        {
            Color.Serialize(writer);
        }
        else
        {
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
        }

        writer.WriteFloat32(Scale);
        writer.WriteBoolean(IsOvergroundOnly);
        writer.WriteFloat32(PositionOffsetMultiplier);

        if (SystemId is not null)
        {
            writer.WriteVarUtf8String(SystemId, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        if (bits.IsSet(1))
        {
            Color = new Color();
            Color.Deserialize(reader);
        }
        else
        {
            reader.ReadUInt8();
            reader.ReadUInt8();
            reader.ReadUInt8();
        }

        Scale = reader.ReadFloat32();
        IsOvergroundOnly = reader.ReadBoolean();
        PositionOffsetMultiplier = reader.ReadFloat32();

        if (bits.IsSet(2))
        {
            SystemId = reader.ReadUtf8String();
        }
    }
}