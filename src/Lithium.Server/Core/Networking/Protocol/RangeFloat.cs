using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 8,
    VariableFieldCount = 0,
    VariableBlockStart = 8,
    MaxSize = 8
)]
public sealed class RangeFloat : INetworkSerializable
{
    [JsonPropertyName("min")] public float Min { get; set; }
    [JsonPropertyName("max")] public float Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
    }
}