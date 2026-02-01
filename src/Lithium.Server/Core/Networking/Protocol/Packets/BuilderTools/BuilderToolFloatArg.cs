using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolFloatArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public float DefaultValue { get; set; }
    
    [JsonPropertyName("min")]
    public float Min { get; set; }

    [JsonPropertyName("max")]
    public float Max { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(DefaultValue);
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
    }

    public void Deserialize(PacketReader reader)
    {
        DefaultValue = reader.ReadFloat32();
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
    }
}
