using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 5,
    VariableFieldCount = 0,
    VariableBlockStart = 5,
    MaxSize = 5
)]
public sealed class EasingConfig : INetworkSerializable
{
    [JsonPropertyName("time")] public float Time { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<EasingType>))]
    public EasingType Type { get; set; } = EasingType.Linear;

    public EasingConfig()
    {
    }

    public EasingConfig(float time, EasingType type)
    {
        Time = time;
        Type = type;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Time);
        writer.WriteEnum(Type);
    }

    public void Deserialize(PacketReader reader)
    {
        Time = reader.ReadFloat32();
        Type = reader.ReadEnum<EasingType>();
    }
}