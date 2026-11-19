using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 104,
    IsCompressed = false,
    NullableBitFieldSize = 0,
    FixedBlockSize = 18,
    VariableFieldCount = 0,
    VariableBlockStart = 18,
    MaxSize = 18
)]
public sealed class JoinWorldPacket : INetworkSerializable
{
    [JsonPropertyName("clearWorld")]
    public bool ClearWorld { get; set; }

    [JsonPropertyName("fadeInOut")]
    public bool FadeInOut { get; set; }

    [JsonPropertyName("worldUuid")]
    public Guid WorldUuid { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(ClearWorld);
        writer.WriteBoolean(FadeInOut);
        writer.WriteGuid(WorldUuid);
    }

    public void Deserialize(PacketReader reader)
    {
        ClearWorld = reader.ReadBoolean();
        FadeInOut = reader.ReadBoolean();
        WorldUuid = reader.ReadGuid();
    }
}
