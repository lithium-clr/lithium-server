using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public record struct Color : INetworkSerializable
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