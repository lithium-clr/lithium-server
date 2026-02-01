using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class BlockSelectorToolData : INetworkSerializable
{
    [JsonPropertyName("durabilityLossOnUse")]
    public float DurabilityLossOnUse { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(DurabilityLossOnUse);
    }

    public void Deserialize(PacketReader reader)
    {
        DurabilityLossOnUse = reader.ReadFloat32();
    }
}
