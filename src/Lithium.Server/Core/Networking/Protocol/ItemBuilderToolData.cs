using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ItemBuilderToolData : INetworkSerializable
{
    [JsonPropertyName("ui")]
    public string[]? Ui { get; set; }

    [JsonPropertyName("tools")]
    public BuilderToolState[]? Tools { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Ui is not null) bits.SetBit(1);
        if (Tools is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        var uiOffsetSlot = writer.ReserveOffset();
        var toolsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(uiOffsetSlot, Ui is not null ? writer.Position - varBlockStart : -1);
        if (Ui is not null)
        {
            writer.WriteVarInt(Ui.Length);
            foreach (var item in Ui)
            {
                writer.WriteVarUtf8String(item, 4096000);
            }
        }

        writer.WriteOffsetAt(toolsOffsetSlot, Tools is not null ? writer.Position - varBlockStart : -1);
        if (Tools is not null)
        {
            writer.WriteVarInt(Tools.Length);
            foreach (var item in Tools)
            {
                item.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(2);
        var currentPos = reader.GetPosition();

        if (bits.IsSet(1))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[0]);
            var count = reader.ReadVarInt32();
            Ui = new string[count];
            for (var i = 0; i < count; i++)
            {
                Ui[i] = reader.ReadUtf8String();
            }
        }

        if (bits.IsSet(2))
        {
            reader.SeekTo(reader.VariableBlockStart + offsets[1]);
            var count = reader.ReadVarInt32();
            Tools = new BuilderToolState[count];
            for (var i = 0; i < count; i++)
            {
                Tools[i] = reader.ReadObject<BuilderToolState>();
            }
        }
        
        reader.SeekTo(currentPos);
    }
}
