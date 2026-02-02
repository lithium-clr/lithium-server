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
public sealed class RangeInt : INetworkSerializable
{
    [JsonPropertyName("min")] public int Min { get; set; }
    [JsonPropertyName("max")] public int Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(Min);
        writer.WriteInt32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadInt32();
        Max = reader.ReadInt32();
    }
}