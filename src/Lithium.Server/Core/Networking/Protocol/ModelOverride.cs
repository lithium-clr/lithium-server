using System.Text.Json.Serialization;

namespace Lithium.Server.Core.Networking.Protocol;

public sealed class ModelOverride : INetworkSerializable
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("texture")]
    public string? Texture { get; set; }

    [JsonPropertyName("animationSets")]
    public Dictionary<string, AnimationSet>? AnimationSets { get; set; }

    public void Serialize(PacketWriter writer)
    {
        var bits = new BitSet(1);
        if (Model is not null) bits.SetBit(1);
        if (Texture is not null) bits.SetBit(2);
        if (AnimationSets is not null) bits.SetBit(4);
        writer.WriteBits(bits);

        var modelOffsetSlot = writer.ReserveOffset();
        var textureOffsetSlot = writer.ReserveOffset();
        var animationSetsOffsetSlot = writer.ReserveOffset();

        var varBlockStart = writer.Position;

        writer.WriteOffsetAt(modelOffsetSlot, Model is not null ? writer.Position - varBlockStart : -1);
        if (Model is not null) writer.WriteVarUtf8String(Model, 4096000);

        writer.WriteOffsetAt(textureOffsetSlot, Texture is not null ? writer.Position - varBlockStart : -1);
        if (Texture is not null) writer.WriteVarUtf8String(Texture, 4096000);

        writer.WriteOffsetAt(animationSetsOffsetSlot, AnimationSets is not null ? writer.Position - varBlockStart : -1);
        if (AnimationSets is not null)
        {
            writer.WriteVarInt(AnimationSets.Count);
            foreach (var (key, value) in AnimationSets)
            {
                writer.WriteVarUtf8String(key, 4096000);
                value.Serialize(writer);
            }
        }
    }

    public void Deserialize(PacketReader reader)
    {
        var bits = new BitSet(reader.ReadUInt8());
        var offsets = reader.ReadOffsets(3);

        if (bits.IsSet(1))
        {
            Model = reader.ReadVarUtf8StringAt(offsets[0]);
        }

        if (bits.IsSet(2))
        {
            Texture = reader.ReadVarUtf8StringAt(offsets[1]);
        }

        if (bits.IsSet(4))
        {
            AnimationSets = reader.ReadDictionaryAt(
                offsets[2],
                r => r.ReadUtf8String(),
                r => r.ReadObject<AnimationSet>()
            );
        }
    }
}
