using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolState : INetworkSerializable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("isBrush")]
    public bool IsBrush { get; set; }

    [JsonPropertyName("brushData")]
    public BuilderToolBrushData? BrushData { get; set; }

    [JsonPropertyName("args")]
    public Dictionary<string, BuilderToolArg>? Args { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Id is not null) bits.SetBit(1);
        if (BrushData is not null) bits.SetBit(2);
        if (Args is not null) bits.SetBit(4);
        writer.WriteBits(bits);

        writer.WriteBoolean(IsBrush);

        var idOffsetSlot = writer.ReserveOffset();
        var brushDataOffsetSlot = writer.ReserveOffset();
        var argsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(idOffsetSlot, Id is not null ? writer.Position - varBlockStart : -1);
        if (Id is not null) writer.WriteVarUtf8String(Id, 4096000);

        writer.WriteOffsetAt(brushDataOffsetSlot, BrushData is not null ? writer.Position - varBlockStart : -1);
        if (BrushData is not null) BrushData.Serialize(writer);

        writer.WriteOffsetAt(argsOffsetSlot, Args is not null ? writer.Position - varBlockStart : -1);
        if (Args is not null)
        {
            writer.WriteVarInt(Args.Count);
            foreach (var (key, value) in Args)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var currentPos = reader.GetPosition();

        IsBrush = reader.ReadBoolean();

        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1)) Id = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) BrushData = reader.ReadObjectAt<BuilderToolBrushData>(offsets[1]);

        if (bits.IsSet(4))
        {
            Args = reader.ReadDictionaryAt(
                offsets[2],
                r => r.ReadUtf8String(),
                r => r.ReadObject<BuilderToolArg>()
            );
        }
        
        reader.SeekTo(currentPos);
    }
}
