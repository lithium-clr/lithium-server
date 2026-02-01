using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 9,
    VariableFieldCount = 0,
    VariableBlockStart = 9,
    MaxSize = 9
)]
public sealed class ClampConfig : INetworkSerializable
{
    [JsonPropertyName("min")] public float Min { get; set; }
    [JsonPropertyName("max")] public float Max { get; set; }
    [JsonPropertyName("normalize")] public bool Normalize { get; set; }

    public ClampConfig()
    {
    }

    public ClampConfig(float min, float max, bool normalize)
    {
        Min = min;
        Max = max;
        Normalize = normalize;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat32(Min);
        writer.WriteFloat32(Max);
        writer.WriteBoolean(Normalize);
    }

    public void Deserialize(PacketReader reader)
    {
        Min = reader.ReadFloat32();
        Max = reader.ReadFloat32();
        Normalize = reader.ReadBoolean();
    }
}