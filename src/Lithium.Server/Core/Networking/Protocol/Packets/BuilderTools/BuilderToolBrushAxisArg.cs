using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolBrushAxisArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    [JsonConverter(typeof(JsonStringEnumConverter<BrushAxis>))]
    public BrushAxis DefaultValue { get; set; } = BrushAxis.None;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteEnum(DefaultValue);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadEnum<BrushAxis>();
    }
}