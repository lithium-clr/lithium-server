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
public sealed class Hitbox : INetworkSerializable
{
    [JsonPropertyName("minX")] public float MinX { get; set; }
    [JsonPropertyName("minY")] public float MinY { get; set; }
    [JsonPropertyName("minZ")] public float MinZ { get; set; }
    [JsonPropertyName("maxX")] public float MaxX { get; set; }
    [JsonPropertyName("maxY")] public float MaxY { get; set; }
    [JsonPropertyName("maxZ")] public float MaxZ { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(MinX);
        writer.WriteFloat32(MinY);
        writer.WriteFloat32(MinZ);
        writer.WriteFloat32(MaxX);
        writer.WriteFloat32(MaxY);
        writer.WriteFloat32(MaxZ);
    }

    public void Deserialize(PacketReader reader)
    {
        MinX = reader.ReadFloat32();
        MinY = reader.ReadFloat32();
        MinZ = reader.ReadFloat32();
        MaxX = reader.ReadFloat32();
        MaxY = reader.ReadFloat32();
        MaxZ = reader.ReadFloat32();
    }
}