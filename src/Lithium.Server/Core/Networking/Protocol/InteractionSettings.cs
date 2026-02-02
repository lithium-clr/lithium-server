using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class InteractionSettings : INetworkSerializable
{
    [JsonPropertyName("allowSkipOnClick")]
    public bool AllowSkipOnClick { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(AllowSkipOnClick);
    }

    public void Deserialize(PacketReader reader)
    {
        AllowSkipOnClick = reader.ReadBoolean();
    }
}
