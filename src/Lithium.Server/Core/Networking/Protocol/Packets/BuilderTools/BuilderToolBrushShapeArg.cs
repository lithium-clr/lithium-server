using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBrushShapeArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    [JsonConverter(typeof(JsonStringEnumConverter<BrushShape>))]
    public BrushShape DefaultValue { get; set; } = BrushShape.Cube;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(DefaultValue);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadEnum<BrushShape>();
    }
}
