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
[JsonConverter(typeof(ColorJsonConverter))]
public sealed class Color : INetworkSerializable
{
    [JsonPropertyName("red")] public byte Red { get; set; }
    [JsonPropertyName("green")] public byte Green { get; set; }
    [JsonPropertyName("blue")] public byte Blue { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt8(Red);
        writer.WriteUInt8(Green);
        writer.WriteUInt8(Blue);
    }

    public void Deserialize(PacketReader reader)
    {
        Red = reader.ReadUInt8();
        Green = reader.ReadUInt8();
        Blue = reader.ReadUInt8();
    }
}