using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolIntArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public int DefaultValue { get; set; }

    [JsonPropertyName("min")]
    public int Min { get; set; }

    [JsonPropertyName("max")]
    public int Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(DefaultValue);
        writer.WriteInt32(Min);
        writer.WriteInt32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadInt32();
        Min = reader.ReadInt32();
        Max = reader.ReadInt32();
    }
}