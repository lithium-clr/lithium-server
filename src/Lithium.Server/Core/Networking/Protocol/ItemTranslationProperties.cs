using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemTranslationProperties : INetworkSerializable
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Name is not null) bits.SetBit(1);
        if (Description is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        var nameOffsetSlot = writer.ReserveOffset();
        var descriptionOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(nameOffsetSlot, Name is not null ? writer.Position - varBlockStart : -1);
        if (Name is not null) writer.WriteVarUtf8String(Name, 4096000);

        writer.WriteOffsetAt(descriptionOffsetSlot, Description is not null ? writer.Position - varBlockStart : -1);
        if (Description is not null) writer.WriteVarUtf8String(Description, 4096000);
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            Name = reader.ReadVarStringAtAbsolute(instanceStart + 9 + offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Description = reader.ReadVarStringAtAbsolute(instanceStart + 9 + offsets[1]);
        }
    }
}
