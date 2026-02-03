using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolRotationArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    [JsonConverter(typeof(JsonStringEnumConverter<Rotation>))]
    public Rotation DefaultValue { get; set; } = Rotation.None;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(DefaultValue);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadEnum<Rotation>();
    }
}