using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ResourceType : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (Icon is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        var idOffsetSlot = writer.ReserveOffset();
        var iconOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(iconOffsetSlot, Icon is not null ? writer.Position - varBlockStart : -1);
        if (Icon is not null) writer.WriteVarUtf8String(Icon, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Id = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Icon = reader.ReadVarUtf8StringAt(offsets[1]);
        }
    }
}
