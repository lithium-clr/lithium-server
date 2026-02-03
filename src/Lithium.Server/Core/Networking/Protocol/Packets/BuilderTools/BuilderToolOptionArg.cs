using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol.Packets.BuilderTools;

public sealed class BuilderToolOptionArg : INetworkSerializable
{
    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("options")]
    public string[]? Options { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (DefaultValue is not null) bits.SetBit(1);
        if (Options is not null) bits.SetBit(2);
        writer.WriteBits(bits);

        // Fixed Block (8 bytes for 2 offsets)
        var defaultValueOffsetSlot = writer.ReserveOffset();
        var optionsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // Variable Block
        writer.WriteOffsetAt(defaultValueOffsetSlot, DefaultValue is not null ? writer.Position - varBlockStart : -1);
        if (DefaultValue is not null)
        {
            writer.WriteVarUtf8String(DefaultValue, 4096000);
        }

        writer.WriteOffsetAt(optionsOffsetSlot, Options is not null ? writer.Position - varBlockStart : -1);
        if (Options is not null)
        {
            writer.WriteVarInt(Options.Length);
            foreach (var option in Options)
            {
                writer.WriteVarUtf8String(option, 4096000);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var instanceStart = reader.GetPosition();
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(2);

        if (bits.IsSet(1))
        {
            DefaultValue = reader.ReadVarStringAtAbsolute(instanceStart + 9 + offsets[0]);
        }

        if (bits.IsSet(2))
        {
            var savedPos = reader.GetPosition();
            reader.SeekTo(instanceStart + 9 + offsets[1]);
            var count = reader.ReadVarInt32();
            Options = new string[count];
            for (var i = 0; i < count; i++)
            {
                Options[i] = reader.ReadUtf8String();
            }
            reader.SeekTo(savedPos);
        }
    }
}