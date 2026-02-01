using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelAttachment : INetworkSerializable
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("texture")] public string? Texture { get; set; }
    [JsonPropertyName("gradientSet")] public string? GradientSet { get; set; }
    [JsonPropertyName("gradientId")] public string? GradientId { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);

        if (Model is not null) bits.SetBit(1);
        if (Texture is not null) bits.SetBit(2);
        if (GradientSet is not null) bits.SetBit(4);
        if (GradientId is not null) bits.SetBit(8);

        // 1. BITS
        writer.WriteBits(bits);

        // 2. OFFSETS (pas de fixed block)
        var modelOffsetSlot = writer.ReserveOffset();
        var textureOffsetSlot = writer.ReserveOffset();
        var gradientSetOffsetSlot = writer.ReserveOffset();
        var gradientIdOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        // 3. VARIABLE BLOCK
        if (Model is not null)
        {
            writer.WriteOffsetAt(modelOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Model, 4096000);
        }
        else writer.WriteOffsetAt(modelOffsetSlot, -1);

        if (Texture is not null)
        {
            writer.WriteOffsetAt(textureOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else writer.WriteOffsetAt(textureOffsetSlot, -1);

        if (GradientSet is not null)
        {
            writer.WriteOffsetAt(gradientSetOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientSet, 4096000);
        }
        else writer.WriteOffsetAt(gradientSetOffsetSlot, -1);

        if (GradientId is not null)
        {
            writer.WriteOffsetAt(gradientIdOffsetSlot, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientId, 4096000);
        }
        else writer.WriteOffsetAt(gradientIdOffsetSlot, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();

        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(1))
            Model = reader.ReadVarUtf8StringAt(offsets[0]);

        if (bits.IsSet(2))
            Texture = reader.ReadVarUtf8StringAt(offsets[1]);

        if (bits.IsSet(4))
            GradientSet = reader.ReadVarUtf8StringAt(offsets[2]);

        if (bits.IsSet(8))
            GradientId = reader.ReadVarUtf8StringAt(offsets[3]);
    }
}