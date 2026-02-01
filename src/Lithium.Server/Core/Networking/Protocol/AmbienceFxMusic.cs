using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 5,
    VariableFieldCount = 1,
    VariableBlockStart = 5,
    MaxSize = 1677721600
)]
public sealed class AmbienceFxMusic : INetworkSerializable
{
    [JsonPropertyName("tracks")] public string[]? Tracks { get; set; }
    [JsonPropertyName("volume")] public float Volume { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Tracks is not null) bits.SetBit(1);

        writer.WriteBits(bits);

        writer.WriteFloat32(Volume);

        if (Tracks is not null)
        {
            writer.WriteVarInt(Tracks.Length);
            foreach (var track in Tracks)
            {
                writer.WriteVarUtf8String(track, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        Volume = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            var count = reader.ReadVarInt32();
            Tracks = new string[count];
            for (var i = 0; i < count; i++)
            {
                Tracks[i] = reader.ReadUtf8String();
            }
        }
    }
}