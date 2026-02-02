using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 0,
    FixedBlockSize = 18,
    VariableFieldCount = 0,
    VariableBlockStart = 18,
    MaxSize = 18
)]
public sealed class FogOptions : INetworkSerializable
{
    [JsonPropertyName("ignoreFogLimits")]
    public bool IgnoreFogLimits { get; set; }

    [JsonPropertyName("effectiveViewDistanceMultiplier")]
    public float EffectiveViewDistanceMultiplier { get; set; }

    [JsonPropertyName("fogFarViewDistance")]
    public float FogFarViewDistance { get; set; }

    [JsonPropertyName("fogHeightCameraOffset")]
    public float FogHeightCameraOffset { get; set; }

    [JsonPropertyName("fogHeightCameraOverriden")]
    public bool FogHeightCameraOverriden { get; set; }

    [JsonPropertyName("fogHeightCameraFixed")]
    public float FogHeightCameraFixed { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBoolean(IgnoreFogLimits);
        writer.WriteFloat32(EffectiveViewDistanceMultiplier);
        writer.WriteFloat32(FogFarViewDistance);
        writer.WriteFloat32(FogHeightCameraOffset);
        writer.WriteBoolean(FogHeightCameraOverriden);
        writer.WriteFloat32(FogHeightCameraFixed);
    }

    public void Deserialize(PacketReader reader)
    {
        IgnoreFogLimits = reader.ReadBoolean();
        EffectiveViewDistanceMultiplier = reader.ReadFloat32();
        FogFarViewDistance = reader.ReadFloat32();
        FogHeightCameraOffset = reader.ReadFloat32();
        FogHeightCameraOverriden = reader.ReadBoolean();
        FogHeightCameraFixed = reader.ReadFloat32();
    }
}