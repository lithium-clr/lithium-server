using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 32,
    VariableFieldCount = 0,
    VariableBlockStart = 32,
    MaxSize = 32
)]
public sealed class ServerPlayerListUpdate : INetworkSerializable
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("worldUuid")]
    public Guid WorldUuid { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteGuid(Uuid);
        writer.WriteGuid(WorldUuid);
    }

    public void Deserialize(PacketReader reader)
    {
        Uuid = reader.ReadGuid();
        WorldUuid = reader.ReadGuid();
    }
}
