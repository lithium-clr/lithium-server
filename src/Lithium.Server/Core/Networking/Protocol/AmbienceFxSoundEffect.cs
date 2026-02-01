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
public sealed class AmbienceFxSoundEffect : INetworkSerializable
{
    [JsonPropertyName("reverbEffectIndex")]
    public int ReverbEffectIndex { get; set; }

    [JsonPropertyName("equalizerEffectIndex")]
    public int EqualizerEffectIndex { get; set; }

    [JsonPropertyName("isInstant")] public bool IsInstant { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt32(ReverbEffectIndex);
        writer.WriteInt32(EqualizerEffectIndex);
        writer.WriteBoolean(IsInstant);
    }

    public void Deserialize(PacketReader reader)
    {
        ReverbEffectIndex = reader.ReadInt32();
        EqualizerEffectIndex = reader.ReadInt32();
        IsInstant = reader.ReadBoolean();
    }
}