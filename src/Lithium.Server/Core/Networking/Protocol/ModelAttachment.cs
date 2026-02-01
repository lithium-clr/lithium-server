using System.Text.Json.Serialization;
using Lithium.Server.Core.Networking.Protocol.Attributes;

namespace Lithium.Server.Core.Networking.Protocol;

[Packet(
    NullableBitFieldSize = 1,
    FixedBlockSize = 1,
    VariableFieldCount = 4,
    VariableBlockStart = 17,
    MaxSize = 65536037
)]
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

        writer.WriteBits(bits);

        var modelOffset = writer.ReserveOffset();
        var textureOffset = writer.ReserveOffset();
        var gradientSetOffset = writer.ReserveOffset();
        var gradientIdOffset = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        if (Model is not null)
        {
            writer.WriteOffsetAt(modelOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Model, 4096000);
        }
        else writer.WriteOffsetAt(modelOffset, -1);

        if (Texture is not null)
        {
            writer.WriteOffsetAt(textureOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(Texture, 4096000);
        }
        else writer.WriteOffsetAt(textureOffset, -1);

        if (GradientSet is not null)
        {
            writer.WriteOffsetAt(gradientSetOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientSet, 4096000);
        }
        else writer.WriteOffsetAt(gradientSetOffset, -1);

        if (GradientId is not null)
        {
            writer.WriteOffsetAt(gradientIdOffset, writer.Position - varBlockStart);
            writer.WriteVarUtf8String(GradientId, 4096000);
        }
        else writer.WriteOffsetAt(gradientIdOffset, -1);
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = reader.ReadBits();
        var offsets = reader.ReadOffsets(4);

        if (bits.IsSet(1)) Model = reader.ReadVarUtf8StringAt(offsets[0]);
        if (bits.IsSet(2)) Texture = reader.ReadVarUtf8StringAt(offsets[1]);
        if (bits.IsSet(4)) GradientSet = reader.ReadVarUtf8StringAt(offsets[2]);
        if (bits.IsSet(8)) GradientId = reader.ReadVarUtf8StringAt(offsets[3]);
    }
}