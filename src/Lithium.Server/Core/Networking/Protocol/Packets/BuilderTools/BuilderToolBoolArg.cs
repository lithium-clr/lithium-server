using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBoolArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public bool DefaultValue { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(DefaultValue);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadBoolean();
    }
}