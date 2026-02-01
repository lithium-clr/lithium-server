using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemResourceType : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null)
        {
            bits.SetBit(1);
        }
        writer.WriteBits(bits);

        writer.WriteInt32(Quantity);

        if (Id is not null)
        {
            writer.WriteVarUtf8String(Id, 4096000);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());

        Quantity = reader.ReadInt32();

        if (bits.IsSet(1))
        {
            Id = reader.ReadUtf8String();
        }
    }
}
