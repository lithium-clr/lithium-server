using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBrushOriginArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    [JsonConverter(typeof(JsonStringEnumConverter<BrushOrigin>))]
    public BrushOrigin DefaultValue { get; set; } = BrushOrigin.Center;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(DefaultValue);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadEnum<BrushOrigin>();
    }
}
