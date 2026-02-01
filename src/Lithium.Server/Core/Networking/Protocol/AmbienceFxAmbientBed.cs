using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 6,
    VariableFieldCount = 1,
    VariableBlockStart = 6,
    MaxSize = 16384011
)]
public sealed class AmbienceFxAmbientBed : INetworkSerializable
{
    [JsonPropertyName("track")] public string? Track { get; set; }
    [JsonPropertyName("volume")] public float Volume { get; set; }

    [JsonPropertyName("transitionSpeed")]
    public AmbienceTransitionSpeed TransitionSpeed { get; set; } = AmbienceTransitionSpeed.Default;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Track is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteFloat32(Volume);
        writer.WriteEnum(TransitionSpeed);

        if (Track is not null)
        {
            writer.WriteVarUtf8String(Track, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Volume = reader.ReadFloat32();
        TransitionSpeed = reader.ReadEnum<AmbienceTransitionSpeed>();

        if (bits.IsSet(1))
        {
            Track = reader.ReadUtf8String();
        }
    }
}