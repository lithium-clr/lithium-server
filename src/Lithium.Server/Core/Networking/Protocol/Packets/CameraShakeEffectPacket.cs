using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol.Packets;

[Packet(
    Id = 281,
    IsCompressed = false,
    NullableBitFieldSize = 0,
    FixedBlockSize = 9,
    VariableFieldCount = 0,
    VariableBlockStart = 9,
    MaxSize = 9
)]
public sealed class CameraShakeEffectPacket : INetworkSerializable
{
    [JsonPropertyName("cameraShakeId")]
    public int CameraShakeId { get; set; }

    [JsonPropertyName("intensity")]
    public float Intensity { get; set; }

    [JsonPropertyName("mode")]
    [JsonConverter(typeof(EnumStringConverter<AccumulationMode>))]
    public AccumulationMode Mode { get; set; } = AccumulationMode.Set;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(CameraShakeId);
        writer.WriteFloat32(Intensity);
        writer.WriteEnum(Mode);
    }

    public void Deserialize(PacketReader reader)
    {
        CameraShakeId = reader.ReadInt32();
        Intensity = reader.ReadFloat32();
        Mode = reader.ReadEnum<AccumulationMode>();
    }
}