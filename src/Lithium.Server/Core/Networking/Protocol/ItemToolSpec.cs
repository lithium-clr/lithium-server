using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemToolSpec : INetworkSerializable
{
    [JsonPropertyName("gatherType")]
    public string? GatherType { get; set; }

    [JsonPropertyName("power")]
    public float Power { get; set; }

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (GatherType is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteFloat32(Power);
        writer.WriteInt32(Quality);

        if (GatherType is not null)
        {
            writer.WriteVarUtf8String(GatherType, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Power = reader.ReadFloat32();
        Quality = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            GatherType = reader.ReadUtf8String();
        }
    }
}
