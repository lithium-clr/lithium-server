using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 24,
    VariableFieldCount = 0,
    VariableBlockStart = 24,
    MaxSize = 24
)]
public sealed class Tint : INetworkSerializable
{
    [JsonPropertyName("top")] public int Top { get; set; }
    [JsonPropertyName("bottom")] public int Bottom { get; set; }
    [JsonPropertyName("front")] public int Front { get; set; }
    [JsonPropertyName("back")] public int Back { get; set; }
    [JsonPropertyName("left")] public int Left { get; set; }
    [JsonPropertyName("right")] public int Right { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Top);
        writer.WriteInt32(Bottom);
        writer.WriteInt32(Front);
        writer.WriteInt32(Back);
        writer.WriteInt32(Left);
        writer.WriteInt32(Right);
    }

    public void Deserialize(PacketReader reader)
    {
        Top = reader.ReadInt32();
        Bottom = reader.ReadInt32();
        Front = reader.ReadInt32();
        Back = reader.ReadInt32();
        Left = reader.ReadInt32();
        Right = reader.ReadInt32();
    }
}