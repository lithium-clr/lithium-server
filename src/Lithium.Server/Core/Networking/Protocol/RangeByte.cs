using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 2,
    VariableFieldCount = 0,
    VariableBlockStart = 2,
    MaxSize = 2
)]
public sealed class RangeByte : INetworkSerializable
{
    [JsonPropertyName("min")] public byte Min { get; set; }
    [JsonPropertyName("max")] public byte Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt8(Min);
        writer.WriteUInt8(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadUInt8();
        Max = reader.ReadUInt8();
    }
}