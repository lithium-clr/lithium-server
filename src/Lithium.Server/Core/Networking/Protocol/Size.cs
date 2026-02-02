using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

[JsonConverter(typeof(SizeJsonConverter))]
public sealed class Size : INetworkSerializable
{
    [JsonPropertyName("width")] public int Width { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }

    public Size()
    {
    }

    public Size(int size)
    {
        Width = Height = size;
    }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Width);
        writer.WriteInt32(Height);
    }

    public void Deserialize(PacketReader reader)
    {
        Width = reader.ReadInt32();
        Height = reader.ReadInt32();
    }
}