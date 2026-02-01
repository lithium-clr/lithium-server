using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class AudioCategory : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volume")]
    public float Volume { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteFloat32(Volume);

        if (Id is not null)
        {
            writer.WriteVarUtf8String(Id, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Volume = reader.ReadFloat32();

        if (bits.IsSet(1))
        {
            Id = reader.ReadUtf8String();
        }
    }
}
