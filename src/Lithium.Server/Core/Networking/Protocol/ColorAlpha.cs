using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 4,
    VariableFieldCount = 0,
    VariableBlockStart = 4,
    MaxSize = 4
)]
[JsonConverter(typeof(ColorAlphaJsonConverter))]
public sealed class ColorAlpha : INetworkSerializable
{
    [JsonPropertyName("alpha")]
    public sbyte Alpha { get; set; }

    [JsonPropertyName("red")]
    public sbyte Red { get; set; }

    [JsonPropertyName("green")]
    public sbyte Green { get; set; }

    [JsonPropertyName("blue")]
    public sbyte Blue { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt8(Alpha);
        writer.WriteInt8(Red);
        writer.WriteInt8(Green);
        writer.WriteInt8(Blue);
    }

    public void Deserialize(PacketReader reader)
    {
        Alpha = reader.ReadInt8();
        Red = reader.ReadInt8();
        Green = reader.ReadInt8();
        Blue = reader.ReadInt8();
    }
}